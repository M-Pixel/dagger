using GraphQL;

namespace Dagger;

record GraphQLRequestErrorExceptionOptions
(
	IGraphQLResponse Response,
	string Request
) : DaggerExceptionOptions;

/// <summary>
/// This error originates from the dagger engine. It means that some error was thrown and sent back via GraphQL.
/// </summary>
public class GraphQLRequestErrorException : DaggerException
{
	public override ErrorCode Code => ErrorCode.GraphQLRequestError;

	/// <summary>The query and variables which caused the error.</summary>
	public string RequestContext { get; }

	/// <summary>The GraphQL response containing the error.</summary>
	public IGraphQLResponse Response { get; }


	internal GraphQLRequestErrorException(string message, GraphQLRequestErrorExceptionOptions options)
		: base(message, options)
	{
		RequestContext = options.Request;
		Response = options.Response;
	}
}
