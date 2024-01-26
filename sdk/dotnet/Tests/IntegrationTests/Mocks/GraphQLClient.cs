using System.Reactive.Linq;
using GraphQL;
using GraphQL.Client.Abstractions;

namespace IntegrationTests;

class GraphQLClientMock : IGraphQLClient
{
	public SendQueryAsyncDelegate? SendQueryAsyncOverride { get; init; }
	public delegate Task<GraphQLResponse<object>> SendQueryAsyncDelegate(Type responseType, GraphQLRequest request, CancellationToken cancellationToken);
	public async Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default)
		=> SendQueryAsyncOverride != null
			? (await SendQueryAsyncOverride(typeof(TResponse), request, cancellationToken)).CastData<TResponse>()
			: new GraphQLResponse<TResponse>();

	public SendMutationAsyncDelegate? SendMutationAsyncOverride { get; init; }
	public delegate Task<GraphQLResponse<object>> SendMutationAsyncDelegate(Type responseType, GraphQLRequest request, CancellationToken cancellationToken);
	public async Task<GraphQLResponse<TResponse>> SendMutationAsync<TResponse>(GraphQLRequest request, CancellationToken cancellationToken = default)
		=> SendMutationAsyncOverride != null
			? (await SendMutationAsyncOverride(typeof(TResponse), request, cancellationToken)).CastData<TResponse>()
			: new GraphQLResponse<TResponse>();

	public CreateSubscriptionStreamDelegate? CreateSubscriptionStreamOverride { get; init; }
	public delegate IObservable<GraphQLResponse<object>> CreateSubscriptionStreamDelegate(Type responseType, GraphQLRequest request);

	public IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(GraphQLRequest request)
		=> CreateSubscriptionStreamOverride?.Invoke(typeof(TResponse), request)
			.Select(response => response.CastData<TResponse>())
			?? Observable.Empty<GraphQLResponse<TResponse>>();

	public CreateSubscriptionStreamWithErrorHandlerDelegate? CreateSubscriptionStreamWithErrorHandlerOverride { get; init; }
	public delegate IObservable<GraphQLResponse<object>> CreateSubscriptionStreamWithErrorHandlerDelegate(Type responseType, GraphQLRequest request, Action<Exception> exceptionHandler);
	public IObservable<GraphQLResponse<TResponse>> CreateSubscriptionStream<TResponse>(GraphQLRequest request, Action<Exception> exceptionHandler)
		=> CreateSubscriptionStreamWithErrorHandlerOverride?.Invoke(typeof(TResponse), request, exceptionHandler)
			.Select(response => response.CastData<TResponse>())
		   ?? Observable.Empty<GraphQLResponse<TResponse>>();
}

static class GraphQLResponseExtensions
{
	public static GraphQLResponse<TResponse> CastData<TResponse>(this GraphQLResponse<object> self)
		=> new()
		{
			Data = (TResponse)self.Data,
			Errors = self.Errors,
			Extensions = self.Extensions
		};
}
