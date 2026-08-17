using OrderManagement.SharedKernel;

namespace OrderManagement.Application.Abstractions.Messaging;

public interface ICustomMediator
{
    // Command không trả về dữ liệu -> Trả về Task<Result>
    Task<Result> SendAsync(ICommand command, CancellationToken cancellationToken = default);

    // Command có trả về dữ liệu -> Trả về Task<Result<TResponse>>
    Task<Result<TResponse>> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);

    // Query trả về dữ liệu -> Trả về Task<Result<TResponse>>
    Task<Result<TResponse>> QueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
