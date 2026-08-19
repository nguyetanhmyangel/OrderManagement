using System.Data.Common;
using Dapper;
using OrderManagement.Application.Abstractions.Persistence;
using OrderManagement.SharedKernel;

namespace OrderManagement.Infrastructure.Inbox;


public abstract class IdempotentDomainEventHandler<TEvent>(
    ISqlConnectionFactory connectionFactory)
    : IDomainEventHandler<TEvent>
    where TEvent : IDomainEvent
{
    protected virtual string HandlerName =>
        GetType().FullName ?? GetType().Name;

    public async Task Handle(
        TEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        await using var connection =
            await connectionFactory
                .CreateOpenConnectionAsync(cancellationToken);

        await using var transaction =
            await connection.BeginTransactionAsync(
                cancellationToken);

        try
        {
            const string sql = """
                INSERT INTO inbox_messages
                (
                    event_id,
                    handler_name,
                    processed_on_utc
                )
                VALUES
                (
                    @EventId,
                    @HandlerName,
                    @ProcessedOnUtc
                )
                ON CONFLICT
                (
                    event_id,
                    handler_name
                )
                DO NOTHING
                RETURNING event_id;
                """;

            var insertedEventId =
                await connection.ExecuteScalarAsync<Guid?>(
                    new CommandDefinition(
                        sql,
                        new
                        {
                            EventId = domainEvent.EventId,
                            HandlerName,
                            ProcessedOnUtc = DateTime.UtcNow
                        },
                        transaction: transaction,
                        cancellationToken: cancellationToken));

            // Handler này đã xử lý event trước đó.
            if (insertedEventId is null)
            {
                await transaction.RollbackAsync(
                    CancellationToken.None);

                return;
            }

            // Business logic sử dụng cùng transaction.
            await HandleInternalAsync(
                domainEvent,
                connection,
                transaction,
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(
                CancellationToken.None);

            throw;
        }
    }

    protected abstract Task HandleInternalAsync(
        TEvent domainEvent,
        DbConnection connection,
        DbTransaction transaction,
        CancellationToken cancellationToken);
}
