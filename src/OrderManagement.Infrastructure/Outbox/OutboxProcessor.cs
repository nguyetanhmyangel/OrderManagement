using System.Collections.Concurrent;
using System.Text.Json;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderManagement.Application.Abstractions.DomainEvents;
using OrderManagement.Application.Abstractions.Persistence;
using OrderManagement.Infrastructure.Events;
using OrderManagement.SharedKernel;

namespace OrderManagement.Infrastructure.Outbox;

public sealed class OutboxProcessor(
    ISqlConnectionFactory connectionFactory,
    IServiceProvider serviceProvider,
    IEventTypeRegistry eventRegistry,
    IOptions<OutboxOptions> options,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private readonly OutboxOptions _options = options.Value;

    private readonly string _instanceId =
        $"{Environment.MachineName}-{Guid.NewGuid():N}"[..32];

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(
                _options.PollingIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Lỗi không xác định trong Outbox Worker");
            }
        }
    }

    private async Task ProcessBatchAsync(
        CancellationToken ct)
    {
        var claimedMessages =
            await ClaimMessagesAsync(ct);

        if (claimedMessages.Count == 0)
            return;

        var results =
            new ConcurrentBag<OutboxProcessingResult>();

        await Parallel.ForEachAsync(
            claimedMessages,
            new ParallelOptions
            {
                MaxDegreeOfParallelism =
                    _options.MaxDegreeOfParallelism,

                CancellationToken = ct
            },
            async (message, token) =>
            {
                using var scope =
                    serviceProvider.CreateScope();

                var dispatcher =
                    scope.ServiceProvider
                        .GetRequiredService<IDomainEventsDispatcher>();

                try
                {
                    var eventType =
                        eventRegistry.GetEventType(message.Type);

                    var domainEvent =
                        (IDomainEvent?)JsonSerializer.Deserialize(
                            message.Content,
                            eventType);

                    if (domainEvent is null)
                    {
                        throw new InvalidOperationException(
                            $"Không thể deserialize event '{message.Type}'.");
                    }

                    await dispatcher.DispatchAsync(
                        domainEvent,
                        token);

                    results.Add(
                        new OutboxProcessingResult(
                            message.Id,
                            true,
                            null,
                            message.RetryCount));
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Dispatch Outbox Message {Id} thất bại",
                        message.Id);

                    results.Add(
                        new OutboxProcessingResult(
                            message.Id,
                            false,
                            ex.Message,
                            message.RetryCount));
                }
            });

        await UpdateStatusesBatchAsync(
            results.ToList(),
            ct);
    }

    private async Task<List<OutboxMessageDto>> ClaimMessagesAsync(
        CancellationToken ct)
    {
        await using var connection =
            await connectionFactory
                .CreateOpenConnectionAsync(ct);

        await using var transaction =
            await connection.BeginTransactionAsync(ct);

        const string sql = """
            WITH cte AS (
                SELECT id
                FROM outbox_messages
                WHERE
                    (
                        status = @Pending
                        OR (
                            status = @Processing
                            AND locked_until_utc < @Now
                        )
                    )
                    AND (
                        next_attempt_on_utc IS NULL
                        OR next_attempt_on_utc <= @Now
                    )
                    AND retry_count < @MaxRetry
                ORDER BY occurred_on_utc
                LIMIT @BatchSize
                FOR UPDATE SKIP LOCKED
            )
            UPDATE outbox_messages AS m
            SET
                status = @Processing,
                locked_until_utc = @LockedUntil,
                locked_by = @LockedBy
            FROM cte
            WHERE m.id = cte.id
            RETURNING
                m.id,
                m.type,
                m.content,
                m.retry_count;
            """;

        var now = DateTime.UtcNow;

        var messages =
            (await connection.QueryAsync<OutboxMessageDto>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Pending = OutboxStatus.Pending,
                        Processing = OutboxStatus.Processing,
                        Now = now,
                        LockedUntil = now.AddSeconds(
                            _options.LockTimeoutSeconds),
                        LockedBy = _instanceId,
                        MaxRetry = _options.MaxRetryCount,
                        BatchSize = _options.BatchSize
                    },
                    transaction: transaction,
                    cancellationToken: ct)))
            .ToList();

        await transaction.CommitAsync(ct);

        return messages;
    }

    private async Task UpdateStatusesBatchAsync(
        List<OutboxProcessingResult> results,
        CancellationToken ct)
    {
        if (results.Count == 0)
            return;

        await using var connection =
            await connectionFactory
                .CreateOpenConnectionAsync(ct);

        const string sql = """
            UPDATE outbox_messages
            SET
                status = @Status,

                processed_on_utc =
                    CASE
                        WHEN @Status = @Processed
                        THEN @Now
                        ELSE processed_on_utc
                    END,

                failed_on_utc =
                    CASE
                        WHEN @Status = @Failed
                        THEN @Now
                        ELSE failed_on_utc
                    END,

                last_error = @Error,

                next_attempt_on_utc = @NextAttempt,

                retry_count =
                    CASE
                        WHEN @Increment = 1
                        THEN retry_count + 1
                        ELSE retry_count
                    END,

                locked_until_utc = NULL,
                locked_by = NULL

            WHERE id = @Id;
            """;

        foreach (var result in results)
        {
            var nextRetry =
                result.RetryCount + 1;

            var isPermanentFail =
                !result.Success &&
                nextRetry >= _options.MaxRetryCount;

            DateTime? nextAttempt = null;

            if (!result.Success && !isPermanentFail)
            {
                var delay =
                    Math.Pow(2, nextRetry) * 5;

                var jitter =
                    Random.Shared.NextDouble();

                nextAttempt =
                    DateTime.UtcNow.AddSeconds(
                        delay + jitter);
            }

            var status =
                result.Success
                    ? OutboxStatus.Processed
                    : isPermanentFail
                        ? OutboxStatus.Failed
                        : OutboxStatus.Pending;

            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Id = result.Id,
                        Status = status,
                        Processed = OutboxStatus.Processed,
                        Failed = OutboxStatus.Failed,
                        Error = result.Error,
                        NextAttempt = nextAttempt,
                        Increment = result.Success ? 0 : 1,
                        Now = DateTime.UtcNow
                    },
                    cancellationToken: ct));
        }
    }

    private sealed record OutboxMessageDto(
        Guid Id,
        string Type,
        string Content,
        int RetryCount);

    private sealed record OutboxProcessingResult(
        Guid Id,
        bool Success,
        string? Error,
        int RetryCount);
}
