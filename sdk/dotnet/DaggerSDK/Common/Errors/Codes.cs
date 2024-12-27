namespace Dagger;

public enum ErrorCode
{
	GraphQLRequestError = 100,
	UnknownDaggerError = 101,
	TooManyNestedObjectsError = 102,
	EngineSessionConnectParamsParseError = 103,
	EngineSessionConnectionTimeoutError = 104,
	EngineSessionError = 105,
	InitEngineSessionBinaryError = 106,
	DockerImageRefValidationError = 107,
	NotAwaitedRequestError = 108,
	ExecError = 109
}
