using System.Data;
using System.Text.Json;
using Dapper;
using OrderManagement.Application.Abstractions.Persistence;
using OrderManagement.SharedKernel;

namespace OrderManagement.Infrastructure.Persistence;

public sealed class DapperUnitOfWork(ISqlConnectionFactory connectionFactory) : IUnitOfWork
{
    private readonly List<Func<IDbConnection, IDbTransaction, Task>> _pendingCommands = [];

    // Đổi List<dynamic> thành List<IEntity> để an toàn về kiểu (Type-safe)
    private readonly List<IEntity> _trackedEntities = [];

    public void TrackEntity(object entity)
    {
        if (entity is IEntity entityWithEvents && !_trackedEntities.Contains(entityWithEvents))
        {
            _trackedEntities.Add(entityWithEvents);
        }
    }

    public void EnqueueCommand(Func<IDbConnection, IDbTransaction, Task> sqlOperation)
        => _pendingCommands.Add(sqlOperation);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.GetOpenConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Thực thi các câu lệnh CUD nghiệp vụ
            foreach (var command in _pendingCommands)
            {
                await command(connection, transaction);
            }

            // 2. Thu thập Domain Events từ các Entity đã được Track (Trực tiếp, không cast lỗi)
            var domainEvents = _trackedEntities
                .SelectMany(e => e.DomainEvents)
                .ToList();

            // Clear events sau khi đã gom
            _trackedEntities.ForEach(e => e.ClearDomainEvents());

            // 3. Ghi Outbox Messages trong CÙNG TRANSACTION
            foreach (var @event in domainEvents)
            {
                const string sqlOutbox = @"
                    INSERT INTO OutboxMessages (Id, Type, Content, OccurredOnUtc, RetryCount)
                    VALUES (@Id, @Type, @Content, @OccurredOnUtc, 0)";

                await connection.ExecuteAsync(sqlOutbox, new
                {
                    Id = Guid.NewGuid(),
                    Type = @event.GetType().AssemblyQualifiedName,
                    Content = JsonSerializer.Serialize(@event, @event.GetType()),
                    @event.OccurredOnUtc
                }, transaction);
            }

            transaction.Commit();
            var affectedRows = _pendingCommands.Count + domainEvents.Count;

            _pendingCommands.Clear();
            _trackedEntities.Clear();

            return affectedRows;
        }
        catch
        {
            transaction.Rollback();
            _pendingCommands.Clear();
            _trackedEntities.Clear();
            throw;
        }
    }
}
