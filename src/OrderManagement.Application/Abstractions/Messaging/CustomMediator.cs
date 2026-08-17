using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.SharedKernel;

namespace OrderManagement.Application.Abstractions.Messaging;

public sealed class CustomMediator(IServiceProvider serviceProvider) : ICustomMediator
{
    private static readonly ConcurrentDictionary<Type, object> NonGenericCommandWrapperCache = new();
    private static readonly ConcurrentDictionary<Type, object> CommandWrapperCache = new();
    private static readonly ConcurrentDictionary<Type, object> QueryWrapperCache = new();

    // 1. Send Command (Non-Generic -> Result)
    public Task<Result> SendAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        var commandType = command.GetType();
        var wrapper = (INonGenericCommandWrapper)NonGenericCommandWrapperCache.GetOrAdd(
            commandType,
            type => Activator.CreateInstance(typeof(NonGenericCommandWrapper<>).MakeGenericType(type))!
        );

        return wrapper.Handle(serviceProvider, command, cancellationToken);
    }

    // 2. Send Command (Generic -> Result<TResponse>)
    public Task<Result<TResponse>> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        var commandType = command.GetType();
        var wrapper = (ICommandWrapper<TResponse>)CommandWrapperCache.GetOrAdd(
            commandType,
            type => Activator.CreateInstance(typeof(CommandWrapper<,>).MakeGenericType(type, typeof(TResponse)))!
        );

        return wrapper.Handle(serviceProvider, command, cancellationToken);
    }

    // 3. Query (Generic -> Result<TResponse>)
    public Task<Result<TResponse>> QueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        var queryType = query.GetType();
        var wrapper = (IQueryWrapper<TResponse>)QueryWrapperCache.GetOrAdd(
            queryType,
            type => Activator.CreateInstance(typeof(QueryWrapper<,>).MakeGenericType(type, typeof(TResponse)))!
        );

        return wrapper.Handle(serviceProvider, query, cancellationToken);
    }

    // --- WRAPPERS IMPLEMENTATION ---

    private interface INonGenericCommandWrapper
    {
        Task<Result> Handle(IServiceProvider provider, object command, CancellationToken cancellationToken);
    }

    private sealed class NonGenericCommandWrapper<TCommand> : INonGenericCommandWrapper
        where TCommand : ICommand
    {
        public Task<Result> Handle(IServiceProvider provider, object command, CancellationToken cancellationToken)
        {
            var handler = provider.GetRequiredService<ICommandHandler<TCommand>>();
            return handler.Handle((TCommand)command, cancellationToken);
        }
    }

    private interface ICommandWrapper<TResponse>
    {
        Task<Result<TResponse>> Handle(IServiceProvider provider, object command, CancellationToken cancellationToken);
    }

    private sealed class CommandWrapper<TCommand, TResponse> : ICommandWrapper<TResponse>
        where TCommand : ICommand<TResponse>
    {
        public Task<Result<TResponse>> Handle(IServiceProvider provider, object command, CancellationToken cancellationToken)
        {
            var handler = provider.GetRequiredService<ICommandHandler<TCommand, TResponse>>();
            return handler.Handle((TCommand)command, cancellationToken);
        }
    }

    private interface IQueryWrapper<TResponse>
    {
        Task<Result<TResponse>> Handle(IServiceProvider provider, object query, CancellationToken cancellationToken);
    }

    private sealed class QueryWrapper<TQuery, TResponse> : IQueryWrapper<TResponse>
        where TQuery : IQuery<TResponse>
    {
        public Task<Result<TResponse>> Handle(IServiceProvider provider, object query, CancellationToken cancellationToken)
        {
            var handler = provider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
            return handler.Handle((TQuery)query, cancellationToken);
        }
    }
}
