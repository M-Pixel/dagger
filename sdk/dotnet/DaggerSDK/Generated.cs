using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Dagger.APIUtils;

namespace Dagger;
///<param name = "Name">The build argument name.</param>
///<param name = "Value">The build argument value.</param>
public sealed record BuildArg(string Name, string Value)
{
	internal OperationArgument AsOperationArguments()
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(Name), _arguments_);
		_arguments_ = new OperationArgument("value", new StringOperationArgumentValue(Value), _arguments_);
		return _arguments_;
	}
};
///<summary>Sharing mode of the cache volume.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<CacheSharingMode>))]
public enum CacheSharingMode
{
	LOCKED,
	PRIVATE,
	SHARED
}

///<summary>The `CacheVolumeID` scalar type represents an identifier for an object of type CacheVolume.</summary>
public sealed record CacheVolumeID(string Value);
///<summary>The `ContainerID` scalar type represents an identifier for an object of type Container.</summary>
public sealed record ContainerID(string Value);
///<summary>The `DirectoryID` scalar type represents an identifier for an object of type Directory.</summary>
public sealed record DirectoryID(string Value);
///<summary>The `EnvVariableID` scalar type represents an identifier for an object of type EnvVariable.</summary>
public sealed record EnvVariableID(string Value);
///<summary>The `FieldTypeDefID` scalar type represents an identifier for an object of type FieldTypeDef.</summary>
public sealed record FieldTypeDefID(string Value);
///<summary>The `FileID` scalar type represents an identifier for an object of type File.</summary>
public sealed record FileID(string Value);
///<summary>The `FunctionArgID` scalar type represents an identifier for an object of type FunctionArg.</summary>
public sealed record FunctionArgID(string Value);
///<summary>The `FunctionCallArgValueID` scalar type represents an identifier for an object of type FunctionCallArgValue.</summary>
public sealed record FunctionCallArgValueID(string Value);
///<summary>The `FunctionCallID` scalar type represents an identifier for an object of type FunctionCall.</summary>
public sealed record FunctionCallID(string Value);
///<summary>The `FunctionID` scalar type represents an identifier for an object of type Function.</summary>
public sealed record FunctionID(string Value);
///<summary>The `GeneratedCodeID` scalar type represents an identifier for an object of type GeneratedCode.</summary>
public sealed record GeneratedCodeID(string Value);
///<summary>The `GitRefID` scalar type represents an identifier for an object of type GitRef.</summary>
public sealed record GitRefID(string Value);
///<summary>The `GitRepositoryID` scalar type represents an identifier for an object of type GitRepository.</summary>
public sealed record GitRepositoryID(string Value);
///<summary>The `HostID` scalar type represents an identifier for an object of type Host.</summary>
public sealed record HostID(string Value);
///<summary>Compression algorithm to use for image layers.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ImageLayerCompression>))]
public enum ImageLayerCompression
{
	EStarGZ,
	Gzip,
	Uncompressed,
	Zstd
}

///<summary>Mediatypes to use in published or exported image metadata.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ImageMediaTypes>))]
public enum ImageMediaTypes
{
	DockerMediaTypes,
	OCIMediaTypes
}

///<summary>The `InterfaceTypeDefID` scalar type represents an identifier for an object of type InterfaceTypeDef.</summary>
public sealed record InterfaceTypeDefID(string Value);
///<summary>An arbitrary JSON-encoded value.</summary>
public sealed record JSON(string Value);
///<summary>The `LabelID` scalar type represents an identifier for an object of type Label.</summary>
public sealed record LabelID(string Value);
///<summary>The `ListTypeDefID` scalar type represents an identifier for an object of type ListTypeDef.</summary>
public sealed record ListTypeDefID(string Value);
///<summary>The `ModuleConfigID` scalar type represents an identifier for an object of type ModuleConfig.</summary>
public sealed record ModuleConfigID(string Value);
///<summary>The `ModuleID` scalar type represents an identifier for an object of type Module.</summary>
public sealed record ModuleID(string Value);
///<summary>Transport layer network protocol associated to a port.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<NetworkProtocol>))]
public enum NetworkProtocol
{
	TCP,
	UDP
}

///<summary>The `ObjectTypeDefID` scalar type represents an identifier for an object of type ObjectTypeDef.</summary>
public sealed record ObjectTypeDefID(string Value);
///<param name = "Name">Label name.</param>
///<param name = "Value">Label value.</param>
public sealed record PipelineLabel(string Name, string Value)
{
	internal OperationArgument AsOperationArguments()
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(Name), _arguments_);
		_arguments_ = new OperationArgument("value", new StringOperationArgumentValue(Value), _arguments_);
		return _arguments_;
	}
};
///<summary><para>The platform config OS and architecture in a Container.</para><para>The format is [os]/[platform]/[version] (e.g., "darwin/arm64/v7", "windows/amd64", "linux/arm64").</para></summary>
public sealed record Platform(string Value);
///<param name = "Frontend">Port to expose to clients. If unspecified, a default will be chosen.</param>
///<param name = "Backend">Destination port for traffic.</param>
///<param name = "Protocol">Transport layer protocol to use for traffic.</param>
public sealed record PortForward(int Backend, int? Frontend = null, NetworkProtocol? Protocol = null)
{
	internal OperationArgument AsOperationArguments()
	{
		OperationArgument? _arguments_ = null;
		if (Frontend != null)
			_arguments_ = new OperationArgument("frontend", EnumOperationArgumentValue.Create(Frontend.Value), _arguments_);
		_arguments_ = new OperationArgument("backend", EnumOperationArgumentValue.Create(Backend), _arguments_);
		if (Protocol != null)
			_arguments_ = new OperationArgument("protocol", EnumOperationArgumentValue.Create(Protocol), _arguments_);
		return _arguments_;
	}
};
///<summary>The `PortID` scalar type represents an identifier for an object of type Port.</summary>
public sealed record PortID(string Value);
///<summary>The `SecretID` scalar type represents an identifier for an object of type Secret.</summary>
public sealed record SecretID(string Value);
///<summary>The `ServiceID` scalar type represents an identifier for an object of type Service.</summary>
public sealed record ServiceID(string Value);
///<summary>The `SocketID` scalar type represents an identifier for an object of type Socket.</summary>
public sealed record SocketID(string Value);
///<summary>The `TypeDefID` scalar type represents an identifier for an object of type TypeDef.</summary>
public sealed record TypeDefID(string Value);
///<summary>Distinguishes the different kinds of TypeDefs.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<TypeDefKind>))]
public enum TypeDefKind
{
	BOOLEAN_KIND,
	INTEGER_KIND,
	INTERFACE_KIND,
	LIST_KIND,
	OBJECT_KIND,
	STRING_KIND,
	VOID_KIND
}

///<summary><para>The absence of a value.</para><para>A Null Void is used as a placeholder for resolvers that do not return anything.</para></summary>
public sealed record Void(string Value);
///<summary>A directory whose contents persist across runs.</summary>
public sealed class CacheVolume : BaseClient
{
	internal CacheVolumeID? CachedId { private get; init; }

	///<summary>A unique identifier for this CacheVolume.</summary>
	public async Task<CacheVolumeID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}
}

///<summary>An OCI-compatible container, also known as a Docker container.</summary>
public sealed class Container : BaseClient
{
	internal ContainerID? CachedId { private get; init; }
	internal string? CachedEnvVariable { private get; init; }
	internal bool? CachedExport { private get; init; }
	internal string? CachedImageRef { private get; init; }
	internal string? CachedLabel { private get; init; }
	internal Platform? CachedPlatform { private get; init; }
	internal string? CachedPublish { private get; init; }
	internal string? CachedShellEndpoint { private get; init; }
	internal string? CachedStderr { private get; init; }
	internal string? CachedStdout { private get; init; }
	internal ContainerID? CachedSync { private get; init; }
	internal string? CachedUser { private get; init; }
	internal string? CachedWorkdir { private get; init; }

	///<summary>A unique identifier for this Container.</summary>
	public async Task<ContainerID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary><para>Turn the container into a Service.</para><para>Be sure to set any exposed ports before this conversion.</para></summary>
	public Service AsService()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("asService", _arguments_);
		return new Service
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Returns a File representing the container serialized to a tarball.</summary>
	///<param name = "PlatformVariants"><para>Identifiers for other platform specific containers.</para><para>Used for multi-platform images.</para></param>
	///<param name = "ForcedCompression"><para>Force each layer of the image to use the specified compression algorithm.</para><para>If this is unset, then if a layer already has a compressed blob in the engine's cache, that will be used (this can result in a mix of compression algorithms for different layers). If this is unset and a layer has no compressed blob in the engine's cache, then it will be compressed using Gzip.</para></param>
	///<param name = "MediaTypes"><para>Use the specified media types for the image's layers.</para><para>Defaults to OCI, which is largely compatible with most recent container runtimes, but Docker may be needed for older runtimes without OCI support.</para></param>
	public File AsTarball(IEnumerable<Container>? platformVariants = null, ImageLayerCompression? forcedCompression = null, ImageMediaTypes? mediaTypes = null)
	{
		OperationArgument? _arguments_ = null;
		if (platformVariants != null)
			_arguments_ = new OperationArgument("platformVariants", ArrayOperationArgumentValue.Create(platformVariants, element => new ReferenceOperationArgumentValue(element)), _arguments_);
		if (forcedCompression != null)
			_arguments_ = new OperationArgument("forcedCompression", EnumOperationArgumentValue.Create(forcedCompression), _arguments_);
		if (mediaTypes != null)
			_arguments_ = new OperationArgument("mediaTypes", EnumOperationArgumentValue.Create(mediaTypes), _arguments_);
		var _newQueryTree_ = QueryTree.Add("asTarball", _arguments_);
		return new File
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Initializes this container from a Dockerfile build.</summary>
	///<param name = "Context">Directory context used by the Dockerfile.</param>
	///<param name = "Dockerfile">Path to the Dockerfile to use.</param>
	///<param name = "Target">Target build stage to build.</param>
	///<param name = "BuildArgs">Additional build arguments.</param>
	///<param name = "Secrets"><para>Secrets to pass to the build.</para><para>They will be mounted at /run/secrets/[secret-name] in the build container</para><para>They can be accessed in the Dockerfile using the "secret" mount type and mount path /run/secrets/[secret-name], e.g. RUN --mount=type=secret,id=my-secret curl http://example.com?token=$(cat /run/secrets/my-secret)</para></param>
	public Container Build(Directory context, string? dockerfile = null, string? target = null, IEnumerable<BuildArg>? buildArgs = null, IEnumerable<Secret>? secrets = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("context", new ReferenceOperationArgumentValue(context), _arguments_);
		if (dockerfile != null)
			_arguments_ = new OperationArgument("dockerfile", new StringOperationArgumentValue(dockerfile), _arguments_);
		if (target != null)
			_arguments_ = new OperationArgument("target", new StringOperationArgumentValue(target), _arguments_);
		if (buildArgs != null)
			_arguments_ = new OperationArgument("buildArgs", ArrayOperationArgumentValue.Create(buildArgs, element => new ObjectOperationArgumentValue(element.AsOperationArguments())), _arguments_);
		if (secrets != null)
			_arguments_ = new OperationArgument("secrets", ArrayOperationArgumentValue.Create(secrets, element => new ReferenceOperationArgumentValue(element)), _arguments_);
		var _newQueryTree_ = QueryTree.Add("build", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves default arguments for future commands.</summary>
	public async Task<ImmutableArray<string>> DefaultArgs()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("defaultArgs", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary><para>Retrieves a directory at the given path.</para><para>Mounts are included.</para></summary>
	///<param name = "Path">The path of the directory to retrieve (e.g., "./src").</param>
	public Directory Directory(string path)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		var _newQueryTree_ = QueryTree.Add("directory", _arguments_);
		return new Directory
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves entrypoint to be prepended to the arguments of all commands.</summary>
	public async Task<ImmutableArray<string>> Entrypoint()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("entrypoint", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary>Retrieves the value of the specified environment variable.</summary>
	///<param name = "Name">The name of the environment variable to retrieve (e.g., "PATH").</param>
	public async Task<string?> EnvVariable(string name)
	{
		if (CachedEnvVariable != null)
			return CachedEnvVariable;
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		var _newQueryTree_ = QueryTree.Add("envVariable", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string?>();
	}

	///<summary>Retrieves the list of environment variables passed to commands.</summary>
	public async Task<ImmutableArray<EnvVariable>> EnvVariables()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("envVariables", _arguments_);
		_newQueryTree_ = _newQueryTree_.Add("id");
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).EnumerateArray().Select(json => new EnvVariable { QueryTree = ImmutableList.Create(new Operation("loadEnvVariableFromID", new OperationArgument("id", new StringOperationArgumentValue(json.GetProperty("id").Deserialize<string>())))), Context = Context, CachedId = new EnvVariableID(json.GetProperty("id").Deserialize<string>()) }).ToImmutableArray();
	}

	///<summary><para>EXPERIMENTAL API! Subject to change/removal at any time.</para><para>Configures all available GPUs on the host to be accessible to this container.</para><para>This currently works for Nvidia devices only.</para></summary>
	public Container ExperimentalWithAllGPUs()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("experimentalWithAllGPUs", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary><para>EXPERIMENTAL API! Subject to change/removal at any time.</para><para>Configures the provided list of devices to be accesible to this container.</para><para>This currently works for Nvidia devices only.</para></summary>
	///<param name = "Devices">List of devices to be accessible to this container.</param>
	public Container ExperimentalWithGPU(IEnumerable<string> devices)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("devices", ArrayOperationArgumentValue.Create(devices, element => new StringOperationArgumentValue(element)), _arguments_);
		var _newQueryTree_ = QueryTree.Add("experimentalWithGPU", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary><para>Writes the container as an OCI tarball to the destination file path on the host.</para><para>Return true on success.</para><para>It can also export platform variants.</para></summary>
	///<param name = "Path"><para>Host's destination path (e.g., "./tarball").</para><para>Path can be relative to the engine's workdir or absolute.</para></param>
	///<param name = "PlatformVariants"><para>Identifiers for other platform specific containers.</para><para>Used for multi-platform image.</para></param>
	///<param name = "ForcedCompression"><para>Force each layer of the exported image to use the specified compression algorithm.</para><para>If this is unset, then if a layer already has a compressed blob in the engine's cache, that will be used (this can result in a mix of compression algorithms for different layers). If this is unset and a layer has no compressed blob in the engine's cache, then it will be compressed using Gzip.</para></param>
	///<param name = "MediaTypes"><para>Use the specified media types for the exported image's layers.</para><para>Defaults to OCI, which is largely compatible with most recent container runtimes, but Docker may be needed for older runtimes without OCI support.</para></param>
	public async Task<bool> Export(string path, IEnumerable<Container>? platformVariants = null, ImageLayerCompression? forcedCompression = null, ImageMediaTypes? mediaTypes = null)
	{
		if (CachedExport != null)
			return CachedExport.Value;
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		if (platformVariants != null)
			_arguments_ = new OperationArgument("platformVariants", ArrayOperationArgumentValue.Create(platformVariants, element => new ReferenceOperationArgumentValue(element)), _arguments_);
		if (forcedCompression != null)
			_arguments_ = new OperationArgument("forcedCompression", EnumOperationArgumentValue.Create(forcedCompression), _arguments_);
		if (mediaTypes != null)
			_arguments_ = new OperationArgument("mediaTypes", EnumOperationArgumentValue.Create(mediaTypes), _arguments_);
		var _newQueryTree_ = QueryTree.Add("export", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<bool>();
	}

	///<summary><para>Retrieves the list of exposed ports.</para><para>This includes ports already exposed by the image, even if not explicitly added with dagger.</para></summary>
	public async Task<ImmutableArray<Port>> ExposedPorts()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("exposedPorts", _arguments_);
		_newQueryTree_ = _newQueryTree_.Add("id");
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).EnumerateArray().Select(json => new Port { QueryTree = ImmutableList.Create(new Operation("loadPortFromID", new OperationArgument("id", new StringOperationArgumentValue(json.GetProperty("id").Deserialize<string>())))), Context = Context, CachedId = new PortID(json.GetProperty("id").Deserialize<string>()) }).ToImmutableArray();
	}

	///<summary><para>Retrieves a file at the given path.</para><para>Mounts are included.</para></summary>
	///<param name = "Path">The path of the file to retrieve (e.g., "./README.md").</param>
	public File File(string path)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		var _newQueryTree_ = QueryTree.Add("file", _arguments_);
		return new File
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Initializes this container from a pulled base image.</summary>
	///<param name = "Address"><para>Image's address from its registry.</para><para>Formatted as [host]/[user]/[repo]:[tag] (e.g., "docker.io/dagger/dagger:main").</para></param>
	public Container From(string address)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("address", new StringOperationArgumentValue(address), _arguments_);
		var _newQueryTree_ = QueryTree.Add("from", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>The unique image reference which can only be retrieved immediately after the 'Container.From' call.</summary>
	public async Task<string> ImageRef()
	{
		if (CachedImageRef != null)
			return CachedImageRef;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("imageRef", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary>Reads the container from an OCI tarball.</summary>
	///<param name = "Source">File to read the container from.</param>
	///<param name = "Tag">Identifies the tag to import from the archive, if the archive bundles multiple tags.</param>
	public Container Import(File source, string? tag = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("source", new ReferenceOperationArgumentValue(source), _arguments_);
		if (tag != null)
			_arguments_ = new OperationArgument("tag", new StringOperationArgumentValue(tag), _arguments_);
		var _newQueryTree_ = QueryTree.Add("import", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves the value of the specified label.</summary>
	///<param name = "Name">The name of the label (e.g., "org.opencontainers.artifact.created").</param>
	public async Task<string?> Label(string name)
	{
		if (CachedLabel != null)
			return CachedLabel;
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		var _newQueryTree_ = QueryTree.Add("label", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string?>();
	}

	///<summary>Retrieves the list of labels passed to container.</summary>
	public async Task<ImmutableArray<Label>> Labels()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("labels", _arguments_);
		_newQueryTree_ = _newQueryTree_.Add("id");
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).EnumerateArray().Select(json => new Label { QueryTree = ImmutableList.Create(new Operation("loadLabelFromID", new OperationArgument("id", new StringOperationArgumentValue(json.GetProperty("id").Deserialize<string>())))), Context = Context, CachedId = new LabelID(json.GetProperty("id").Deserialize<string>()) }).ToImmutableArray();
	}

	///<summary>Retrieves the list of paths where a directory is mounted.</summary>
	public async Task<ImmutableArray<string>> Mounts()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("mounts", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary>Creates a named sub-pipeline.</summary>
	///<param name = "Name">Name of the sub-pipeline.</param>
	///<param name = "Description">Description of the sub-pipeline.</param>
	///<param name = "Labels">Labels to apply to the sub-pipeline.</param>
	public Container Pipeline(string name, string? description = null, IEnumerable<PipelineLabel>? labels = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		if (description != null)
			_arguments_ = new OperationArgument("description", new StringOperationArgumentValue(description), _arguments_);
		if (labels != null)
			_arguments_ = new OperationArgument("labels", ArrayOperationArgumentValue.Create(labels, element => new ObjectOperationArgumentValue(element.AsOperationArguments())), _arguments_);
		var _newQueryTree_ = QueryTree.Add("pipeline", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>The platform this container executes and publishes as.</summary>
	public async Task<Platform> GetPlatform()
	{
		if (CachedPlatform != null)
			return CachedPlatform;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("platform", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary><para>Publishes this container as a new image to the specified address.</para><para>Publish returns a fully qualified ref.</para><para>It can also publish platform variants.</para></summary>
	///<param name = "Address"><para>Registry's address to publish the image to.</para><para>Formatted as [host]/[user]/[repo]:[tag] (e.g. "docker.io/dagger/dagger:main").</para></param>
	///<param name = "PlatformVariants"><para>Identifiers for other platform specific containers.</para><para>Used for multi-platform image.</para></param>
	///<param name = "ForcedCompression"><para>Force each layer of the published image to use the specified compression algorithm.</para><para>If this is unset, then if a layer already has a compressed blob in the engine's cache, that will be used (this can result in a mix of compression algorithms for different layers). If this is unset and a layer has no compressed blob in the engine's cache, then it will be compressed using Gzip.</para></param>
	///<param name = "MediaTypes"><para>Use the specified media types for the published image's layers.</para><para>Defaults to OCI, which is largely compatible with most recent registries, but Docker may be needed for older registries without OCI support.</para></param>
	public async Task<string> Publish(string address, IEnumerable<Container>? platformVariants = null, ImageLayerCompression? forcedCompression = null, ImageMediaTypes? mediaTypes = null)
	{
		if (CachedPublish != null)
			return CachedPublish;
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("address", new StringOperationArgumentValue(address), _arguments_);
		if (platformVariants != null)
			_arguments_ = new OperationArgument("platformVariants", ArrayOperationArgumentValue.Create(platformVariants, element => new ReferenceOperationArgumentValue(element)), _arguments_);
		if (forcedCompression != null)
			_arguments_ = new OperationArgument("forcedCompression", EnumOperationArgumentValue.Create(forcedCompression), _arguments_);
		if (mediaTypes != null)
			_arguments_ = new OperationArgument("mediaTypes", EnumOperationArgumentValue.Create(mediaTypes), _arguments_);
		var _newQueryTree_ = QueryTree.Add("publish", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary>Retrieves this container's root filesystem. Mounts are not included.</summary>
	public Directory Rootfs()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("rootfs", _arguments_);
		return new Directory
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary><para>Return a websocket endpoint that, if connected to, will start the container with a TTY streamed over the websocket.</para><para>Primarily intended for internal use with the dagger CLI.</para></summary>
	public async Task<string> ShellEndpoint()
	{
		if (CachedShellEndpoint != null)
			return CachedShellEndpoint;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("shellEndpoint", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary><para>The error stream of the last executed command.</para><para>Will execute default command if none is set, or error if there's no default.</para></summary>
	public async Task<string> Stderr()
	{
		if (CachedStderr != null)
			return CachedStderr;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("stderr", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary><para>The output stream of the last executed command.</para><para>Will execute default command if none is set, or error if there's no default.</para></summary>
	public async Task<string> Stdout()
	{
		if (CachedStdout != null)
			return CachedStdout;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("stdout", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary><para>Forces evaluation of the pipeline in the engine.</para><para>It doesn't run the default command if no exec has been set.</para></summary>
	public async Task<Container> Sync()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("sync", _arguments_);
		await ComputeQuery(_newQueryTree_, await Context.Connection());
		return this;
	}

	///<summary>Retrieves the user to be set for all commands.</summary>
	public async Task<string> User()
	{
		if (CachedUser != null)
			return CachedUser;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("user", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary>Configures default arguments for future commands.</summary>
	///<param name = "Args">Arguments to prepend to future executions (e.g., ["-v", "--no-cache"]).</param>
	public Container WithDefaultArgs(IEnumerable<string> args)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("args", ArrayOperationArgumentValue.Create(args, element => new StringOperationArgumentValue(element)), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withDefaultArgs", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container plus a directory written at the given path.</summary>
	///<param name = "Path">Location of the written directory (e.g., "/tmp/directory").</param>
	///<param name = "Directory">Identifier of the directory to write</param>
	///<param name = "Exclude">Patterns to exclude in the written directory (e.g. ["node_modules/**", ".gitignore", ".git/"]).</param>
	///<param name = "Include">Patterns to include in the written directory (e.g. ["*.go", "go.mod", "go.sum"]).</param>
	///<param name = "Owner"><para>A user:group to set for the directory and its contents.</para><para>The user and group can either be an ID (1000:1000) or a name (foo:bar).</para><para>If the group is omitted, it defaults to the same as the user.</para></param>
	public Container WithDirectory(string path, Directory directory, IEnumerable<string>? exclude = null, IEnumerable<string>? include = null, string? owner = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		_arguments_ = new OperationArgument("directory", new ReferenceOperationArgumentValue(directory), _arguments_);
		if (exclude != null)
			_arguments_ = new OperationArgument("exclude", ArrayOperationArgumentValue.Create(exclude, element => new StringOperationArgumentValue(element)), _arguments_);
		if (include != null)
			_arguments_ = new OperationArgument("include", ArrayOperationArgumentValue.Create(include, element => new StringOperationArgumentValue(element)), _arguments_);
		if (owner != null)
			_arguments_ = new OperationArgument("owner", new StringOperationArgumentValue(owner), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withDirectory", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container but with a different command entrypoint.</summary>
	///<param name = "Args">Entrypoint to use for future executions (e.g., ["go", "run"]).</param>
	///<param name = "KeepDefaultArgs">Don't remove the default arguments when setting the entrypoint.</param>
	public Container WithEntrypoint(IEnumerable<string> args, bool? keepDefaultArgs = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("args", ArrayOperationArgumentValue.Create(args, element => new StringOperationArgumentValue(element)), _arguments_);
		if (keepDefaultArgs != null)
			_arguments_ = new OperationArgument("keepDefaultArgs", EnumOperationArgumentValue.Create(keepDefaultArgs.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withEntrypoint", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container plus the given environment variable.</summary>
	///<param name = "Name">The name of the environment variable (e.g., "HOST").</param>
	///<param name = "Value">The value of the environment variable. (e.g., "localhost").</param>
	///<param name = "Expand">Replace `${VAR}` or `$VAR` in the value according to the current environment variables defined in the container (e.g., "/opt/bin:$PATH").</param>
	public Container WithEnvVariable(string name, string value, bool? expand = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		_arguments_ = new OperationArgument("value", new StringOperationArgumentValue(value), _arguments_);
		if (expand != null)
			_arguments_ = new OperationArgument("expand", EnumOperationArgumentValue.Create(expand.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withEnvVariable", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container after executing the specified command inside it.</summary>
	///<param name = "Args"><para>Command to run instead of the container's default command (e.g., ["run", "main.go"]).</para><para>If empty, the container's default command is used.</para></param>
	///<param name = "SkipEntrypoint">If the container has an entrypoint, ignore it for args rather than using it to wrap them.</param>
	///<param name = "Stdin">Content to write to the command's standard input before closing (e.g., "Hello world").</param>
	///<param name = "RedirectStdout">Redirect the command's standard output to a file in the container (e.g., "/tmp/stdout").</param>
	///<param name = "RedirectStderr">Redirect the command's standard error to a file in the container (e.g., "/tmp/stderr").</param>
	///<param name = "ExperimentalPrivilegedNesting"><para>Provides dagger access to the executed command.</para><para>Do not use this option unless you trust the command being executed; the command being executed WILL BE GRANTED FULL ACCESS TO YOUR HOST FILESYSTEM.</para></param>
	///<param name = "InsecureRootCapabilities">Execute the command with all root capabilities. This is similar to running a command with "sudo" or executing "docker run" with the "--privileged" flag. Containerization does not provide any security guarantees when using this option. It should only be used when absolutely necessary and only with trusted commands.</param>
	public Container WithExec(IEnumerable<string> args, bool? skipEntrypoint = null, string? stdin = null, string? redirectStdout = null, string? redirectStderr = null, bool? experimentalPrivilegedNesting = null, bool? insecureRootCapabilities = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("args", ArrayOperationArgumentValue.Create(args, element => new StringOperationArgumentValue(element)), _arguments_);
		if (skipEntrypoint != null)
			_arguments_ = new OperationArgument("skipEntrypoint", EnumOperationArgumentValue.Create(skipEntrypoint.Value), _arguments_);
		if (stdin != null)
			_arguments_ = new OperationArgument("stdin", new StringOperationArgumentValue(stdin), _arguments_);
		if (redirectStdout != null)
			_arguments_ = new OperationArgument("redirectStdout", new StringOperationArgumentValue(redirectStdout), _arguments_);
		if (redirectStderr != null)
			_arguments_ = new OperationArgument("redirectStderr", new StringOperationArgumentValue(redirectStderr), _arguments_);
		if (experimentalPrivilegedNesting != null)
			_arguments_ = new OperationArgument("experimentalPrivilegedNesting", EnumOperationArgumentValue.Create(experimentalPrivilegedNesting.Value), _arguments_);
		if (insecureRootCapabilities != null)
			_arguments_ = new OperationArgument("insecureRootCapabilities", EnumOperationArgumentValue.Create(insecureRootCapabilities.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withExec", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary><para>Expose a network port.</para><para>Exposed ports serve two purposes:</para><para>- For health checks and introspection, when running services</para><para>- For setting the EXPOSE OCI field when publishing the container</para></summary>
	///<param name = "Port">Port number to expose</param>
	///<param name = "Protocol">Transport layer network protocol</param>
	///<param name = "Description">Optional port description</param>
	public Container WithExposedPort(int port, NetworkProtocol? protocol = null, string? description = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("port", EnumOperationArgumentValue.Create(port), _arguments_);
		if (protocol != null)
			_arguments_ = new OperationArgument("protocol", EnumOperationArgumentValue.Create(protocol), _arguments_);
		if (description != null)
			_arguments_ = new OperationArgument("description", new StringOperationArgumentValue(description), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withExposedPort", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container plus the contents of the given file copied to the given path.</summary>
	///<param name = "Path">Location of the copied file (e.g., "/tmp/file.txt").</param>
	///<param name = "Source">Identifier of the file to copy.</param>
	///<param name = "Permissions">Permission given to the copied file (e.g., 0600).</param>
	///<param name = "Owner"><para>A user:group to set for the file.</para><para>The user and group can either be an ID (1000:1000) or a name (foo:bar).</para><para>If the group is omitted, it defaults to the same as the user.</para></param>
	public Container WithFile(string path, File source, int? permissions = null, string? owner = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		_arguments_ = new OperationArgument("source", new ReferenceOperationArgumentValue(source), _arguments_);
		if (permissions != null)
			_arguments_ = new OperationArgument("permissions", EnumOperationArgumentValue.Create(permissions.Value), _arguments_);
		if (owner != null)
			_arguments_ = new OperationArgument("owner", new StringOperationArgumentValue(owner), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withFile", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Indicate that subsequent operations should be featured more prominently in the UI.</summary>
	public Container WithFocus()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("withFocus", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container plus the given label.</summary>
	///<param name = "Name">The name of the label (e.g., "org.opencontainers.artifact.created").</param>
	///<param name = "Value">The value of the label (e.g., "2023-01-01T00:00:00Z").</param>
	public Container WithLabel(string name, string value)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		_arguments_ = new OperationArgument("value", new StringOperationArgumentValue(value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withLabel", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container plus a cache volume mounted at the given path.</summary>
	///<param name = "Path">Location of the cache directory (e.g., "/cache/node_modules").</param>
	///<param name = "Cache">Identifier of the cache volume to mount.</param>
	///<param name = "Source">Identifier of the directory to use as the cache volume's root.</param>
	///<param name = "Sharing">Sharing mode of the cache volume.</param>
	///<param name = "Owner"><para>A user:group to set for the mounted cache directory.</para><para>Note that this changes the ownership of the specified mount along with the initial filesystem provided by source (if any). It does not have any effect if/when the cache has already been created.</para><para>The user and group can either be an ID (1000:1000) or a name (foo:bar).</para><para>If the group is omitted, it defaults to the same as the user.</para></param>
	public Container WithMountedCache(string path, CacheVolume cache, Directory? source = null, CacheSharingMode? sharing = null, string? owner = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		_arguments_ = new OperationArgument("cache", new ReferenceOperationArgumentValue(cache), _arguments_);
		if (source != null)
			_arguments_ = new OperationArgument("source", new ReferenceOperationArgumentValue(source), _arguments_);
		if (sharing != null)
			_arguments_ = new OperationArgument("sharing", EnumOperationArgumentValue.Create(sharing), _arguments_);
		if (owner != null)
			_arguments_ = new OperationArgument("owner", new StringOperationArgumentValue(owner), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withMountedCache", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container plus a directory mounted at the given path.</summary>
	///<param name = "Path">Location of the mounted directory (e.g., "/mnt/directory").</param>
	///<param name = "Source">Identifier of the mounted directory.</param>
	///<param name = "Owner"><para>A user:group to set for the mounted directory and its contents.</para><para>The user and group can either be an ID (1000:1000) or a name (foo:bar).</para><para>If the group is omitted, it defaults to the same as the user.</para></param>
	public Container WithMountedDirectory(string path, Directory source, string? owner = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		_arguments_ = new OperationArgument("source", new ReferenceOperationArgumentValue(source), _arguments_);
		if (owner != null)
			_arguments_ = new OperationArgument("owner", new StringOperationArgumentValue(owner), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withMountedDirectory", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container plus a file mounted at the given path.</summary>
	///<param name = "Path">Location of the mounted file (e.g., "/tmp/file.txt").</param>
	///<param name = "Source">Identifier of the mounted file.</param>
	///<param name = "Owner"><para>A user or user:group to set for the mounted file.</para><para>The user and group can either be an ID (1000:1000) or a name (foo:bar).</para><para>If the group is omitted, it defaults to the same as the user.</para></param>
	public Container WithMountedFile(string path, File source, string? owner = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		_arguments_ = new OperationArgument("source", new ReferenceOperationArgumentValue(source), _arguments_);
		if (owner != null)
			_arguments_ = new OperationArgument("owner", new StringOperationArgumentValue(owner), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withMountedFile", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container plus a secret mounted into a file at the given path.</summary>
	///<param name = "Path">Location of the secret file (e.g., "/tmp/secret.txt").</param>
	///<param name = "Source">Identifier of the secret to mount.</param>
	///<param name = "Owner"><para>A user:group to set for the mounted secret.</para><para>The user and group can either be an ID (1000:1000) or a name (foo:bar).</para><para>If the group is omitted, it defaults to the same as the user.</para></param>
	///<param name = "Mode"><para>Permission given to the mounted secret (e.g., 0600).</para><para>This option requires an owner to be set to be active.</para></param>
	public Container WithMountedSecret(string path, Secret source, string? owner = null, int? mode = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		_arguments_ = new OperationArgument("source", new ReferenceOperationArgumentValue(source), _arguments_);
		if (owner != null)
			_arguments_ = new OperationArgument("owner", new StringOperationArgumentValue(owner), _arguments_);
		if (mode != null)
			_arguments_ = new OperationArgument("mode", EnumOperationArgumentValue.Create(mode.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withMountedSecret", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container plus a temporary directory mounted at the given path.</summary>
	///<param name = "Path">Location of the temporary directory (e.g., "/tmp/temp_dir").</param>
	public Container WithMountedTemp(string path)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withMountedTemp", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container plus a new file written at the given path.</summary>
	///<param name = "Path">Location of the written file (e.g., "/tmp/file.txt").</param>
	///<param name = "Contents">Content of the file to write (e.g., "Hello world!").</param>
	///<param name = "Permissions">Permission given to the written file (e.g., 0600).</param>
	///<param name = "Owner"><para>A user:group to set for the file.</para><para>The user and group can either be an ID (1000:1000) or a name (foo:bar).</para><para>If the group is omitted, it defaults to the same as the user.</para></param>
	public Container WithNewFile(string path, string? contents = null, int? permissions = null, string? owner = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		if (contents != null)
			_arguments_ = new OperationArgument("contents", new StringOperationArgumentValue(contents), _arguments_);
		if (permissions != null)
			_arguments_ = new OperationArgument("permissions", EnumOperationArgumentValue.Create(permissions.Value), _arguments_);
		if (owner != null)
			_arguments_ = new OperationArgument("owner", new StringOperationArgumentValue(owner), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withNewFile", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container with a registry authentication for a given address.</summary>
	///<param name = "Address"><para>Registry's address to bind the authentication to.</para><para>Formatted as [host]/[user]/[repo]:[tag] (e.g. docker.io/dagger/dagger:main).</para></param>
	///<param name = "Username">The username of the registry's account (e.g., "Dagger").</param>
	///<param name = "Secret">The API key, password or token to authenticate to this registry.</param>
	public Container WithRegistryAuth(string address, string username, Secret secret)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("address", new StringOperationArgumentValue(address), _arguments_);
		_arguments_ = new OperationArgument("username", new StringOperationArgumentValue(username), _arguments_);
		_arguments_ = new OperationArgument("secret", new ReferenceOperationArgumentValue(secret), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withRegistryAuth", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves the container with the given directory mounted to /.</summary>
	///<param name = "Directory">Directory to mount.</param>
	public Container WithRootfs(Directory directory)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("directory", new ReferenceOperationArgumentValue(directory), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withRootfs", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container plus an env variable containing the given secret.</summary>
	///<param name = "Name">The name of the secret variable (e.g., "API_SECRET").</param>
	///<param name = "Secret">The identifier of the secret value.</param>
	public Container WithSecretVariable(string name, Secret secret)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		_arguments_ = new OperationArgument("secret", new ReferenceOperationArgumentValue(secret), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withSecretVariable", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary><para>Establish a runtime dependency on a service.</para><para>The service will be started automatically when needed and detached when it is no longer needed, executing the default command if none is set.</para><para>The service will be reachable from the container via the provided hostname alias.</para><para>The service dependency will also convey to any files or directories produced by the container.</para></summary>
	///<param name = "Alias">A name that can be used to reach the service from the container</param>
	///<param name = "Service">Identifier of the service container</param>
	public Container WithServiceBinding(string @alias, Service service)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("alias", new StringOperationArgumentValue(@alias), _arguments_);
		_arguments_ = new OperationArgument("service", new ReferenceOperationArgumentValue(service), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withServiceBinding", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container plus a socket forwarded to the given Unix socket path.</summary>
	///<param name = "Path">Location of the forwarded Unix socket (e.g., "/tmp/socket").</param>
	///<param name = "Source">Identifier of the socket to forward.</param>
	///<param name = "Owner"><para>A user:group to set for the mounted socket.</para><para>The user and group can either be an ID (1000:1000) or a name (foo:bar).</para><para>If the group is omitted, it defaults to the same as the user.</para></param>
	public Container WithUnixSocket(string path, Socket source, string? owner = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		_arguments_ = new OperationArgument("source", new ReferenceOperationArgumentValue(source), _arguments_);
		if (owner != null)
			_arguments_ = new OperationArgument("owner", new StringOperationArgumentValue(owner), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withUnixSocket", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container with a different command user.</summary>
	///<param name = "Name">The user to set (e.g., "root").</param>
	public Container WithUser(string name)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withUser", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container with a different working directory.</summary>
	///<param name = "Path">The path to set as the working directory (e.g., "/app").</param>
	public Container WithWorkdir(string path)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withWorkdir", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container with unset default arguments for future commands.</summary>
	public Container WithoutDefaultArgs()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("withoutDefaultArgs", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container with an unset command entrypoint.</summary>
	///<param name = "KeepDefaultArgs">Don't remove the default arguments when unsetting the entrypoint.</param>
	public Container WithoutEntrypoint(bool? keepDefaultArgs = null)
	{
		OperationArgument? _arguments_ = null;
		if (keepDefaultArgs != null)
			_arguments_ = new OperationArgument("keepDefaultArgs", EnumOperationArgumentValue.Create(keepDefaultArgs.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withoutEntrypoint", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container minus the given environment variable.</summary>
	///<param name = "Name">The name of the environment variable (e.g., "HOST").</param>
	public Container WithoutEnvVariable(string name)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withoutEnvVariable", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Unexpose a previously exposed port.</summary>
	///<param name = "Port">Port number to unexpose</param>
	///<param name = "Protocol">Port protocol to unexpose</param>
	public Container WithoutExposedPort(int port, NetworkProtocol? protocol = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("port", EnumOperationArgumentValue.Create(port), _arguments_);
		if (protocol != null)
			_arguments_ = new OperationArgument("protocol", EnumOperationArgumentValue.Create(protocol), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withoutExposedPort", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary><para>Indicate that subsequent operations should not be featured more prominently in the UI.</para><para>This is the initial state of all containers.</para></summary>
	public Container WithoutFocus()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("withoutFocus", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container minus the given environment label.</summary>
	///<param name = "Name">The name of the label to remove (e.g., "org.opencontainers.artifact.created").</param>
	public Container WithoutLabel(string name)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withoutLabel", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container after unmounting everything at the given path.</summary>
	///<param name = "Path">Location of the cache directory (e.g., "/cache/node_modules").</param>
	public Container WithoutMount(string path)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withoutMount", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container without the registry authentication of a given address.</summary>
	///<param name = "Address"><para>Registry's address to remove the authentication from.</para><para>Formatted as [host]/[user]/[repo]:[tag] (e.g. docker.io/dagger/dagger:main).</para></param>
	public Container WithoutRegistryAuth(string address)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("address", new StringOperationArgumentValue(address), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withoutRegistryAuth", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this container with a previously added Unix socket removed.</summary>
	///<param name = "Path">Location of the socket to remove (e.g., "/tmp/socket").</param>
	public Container WithoutUnixSocket(string path)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withoutUnixSocket", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary><para>Retrieves this container with an unset command user.</para><para>Should default to root.</para></summary>
	public Container WithoutUser()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("withoutUser", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary><para>Retrieves this container with an unset working directory.</para><para>Should default to "/".</para></summary>
	public Container WithoutWorkdir()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("withoutWorkdir", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves the working directory for all commands.</summary>
	public async Task<string> Workdir()
	{
		if (CachedWorkdir != null)
			return CachedWorkdir;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("workdir", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}
}

///<summary>A directory.</summary>
public sealed class Directory : BaseClient
{
	internal DirectoryID? CachedId { private get; init; }
	internal bool? CachedExport { private get; init; }
	internal DirectoryID? CachedSync { private get; init; }

	///<summary>A unique identifier for this Directory.</summary>
	public async Task<DirectoryID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary>Load the directory as a Dagger module</summary>
	///<param name = "SourceSubpath"><para>An optional subpath of the directory which contains the module's source code.</para><para>This is needed when the module code is in a subdirectory but requires parent directories to be loaded in order to execute. For example, the module source code may need a go.mod, project.toml, package.json, etc. file from a parent directory.</para><para>If not set, the module source code is loaded from the root of the directory.</para></param>
	public Module AsModule(string? sourceSubpath = null)
	{
		OperationArgument? _arguments_ = null;
		if (sourceSubpath != null)
			_arguments_ = new OperationArgument("sourceSubpath", new StringOperationArgumentValue(sourceSubpath), _arguments_);
		var _newQueryTree_ = QueryTree.Add("asModule", _arguments_);
		return new Module
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Gets the difference between this directory and an another directory.</summary>
	///<param name = "Other">Identifier of the directory to compare.</param>
	public Directory Diff(Directory other)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("other", new ReferenceOperationArgumentValue(other), _arguments_);
		var _newQueryTree_ = QueryTree.Add("diff", _arguments_);
		return new Directory
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves a directory at the given path.</summary>
	///<param name = "Path">Location of the directory to retrieve (e.g., "/src").</param>
	public Directory SubDirectory(string path)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		var _newQueryTree_ = QueryTree.Add("directory", _arguments_);
		return new Directory
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Builds a new Docker container from this directory.</summary>
	///<param name = "Platform">The platform to build.</param>
	///<param name = "Dockerfile">Path to the Dockerfile to use (e.g., "frontend.Dockerfile").</param>
	///<param name = "Target">Target build stage to build.</param>
	///<param name = "BuildArgs">Build arguments to use in the build.</param>
	///<param name = "Secrets"><para>Secrets to pass to the build.</para><para>They will be mounted at /run/secrets/[secret-name].</para></param>
	public Container DockerBuild(Platform? platform = null, string? dockerfile = null, string? target = null, IEnumerable<BuildArg>? buildArgs = null, IEnumerable<Secret>? secrets = null)
	{
		OperationArgument? _arguments_ = null;
		if (platform != null)
			_arguments_ = new OperationArgument("platform", new StringOperationArgumentValue(platform?.Value), _arguments_);
		if (dockerfile != null)
			_arguments_ = new OperationArgument("dockerfile", new StringOperationArgumentValue(dockerfile), _arguments_);
		if (target != null)
			_arguments_ = new OperationArgument("target", new StringOperationArgumentValue(target), _arguments_);
		if (buildArgs != null)
			_arguments_ = new OperationArgument("buildArgs", ArrayOperationArgumentValue.Create(buildArgs, element => new ObjectOperationArgumentValue(element.AsOperationArguments())), _arguments_);
		if (secrets != null)
			_arguments_ = new OperationArgument("secrets", ArrayOperationArgumentValue.Create(secrets, element => new ReferenceOperationArgumentValue(element)), _arguments_);
		var _newQueryTree_ = QueryTree.Add("dockerBuild", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Returns a list of files and directories at the given path.</summary>
	///<param name = "Path">Location of the directory to look at (e.g., "/src").</param>
	public async Task<ImmutableArray<string>> Entries(string? path = null)
	{
		OperationArgument? _arguments_ = null;
		if (path != null)
			_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		var _newQueryTree_ = QueryTree.Add("entries", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary>Writes the contents of the directory to a path on the host.</summary>
	///<param name = "Path">Location of the copied directory (e.g., "logs/").</param>
	public async Task<bool> Export(string path)
	{
		if (CachedExport != null)
			return CachedExport.Value;
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		var _newQueryTree_ = QueryTree.Add("export", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<bool>();
	}

	///<summary>Retrieves a file at the given path.</summary>
	///<param name = "Path">Location of the file to retrieve (e.g., "README.md").</param>
	public File File(string path)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		var _newQueryTree_ = QueryTree.Add("file", _arguments_);
		return new File
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Returns a list of files and directories that matche the given pattern.</summary>
	///<param name = "Pattern">Pattern to match (e.g., "*.md").</param>
	public async Task<ImmutableArray<string>> Glob(string pattern)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("pattern", new StringOperationArgumentValue(pattern), _arguments_);
		var _newQueryTree_ = QueryTree.Add("glob", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary>Creates a named sub-pipeline.</summary>
	///<param name = "Name">Name of the sub-pipeline.</param>
	///<param name = "Description">Description of the sub-pipeline.</param>
	///<param name = "Labels">Labels to apply to the sub-pipeline.</param>
	public Directory Pipeline(string name, string? description = null, IEnumerable<PipelineLabel>? labels = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		if (description != null)
			_arguments_ = new OperationArgument("description", new StringOperationArgumentValue(description), _arguments_);
		if (labels != null)
			_arguments_ = new OperationArgument("labels", ArrayOperationArgumentValue.Create(labels, element => new ObjectOperationArgumentValue(element.AsOperationArguments())), _arguments_);
		var _newQueryTree_ = QueryTree.Add("pipeline", _arguments_);
		return new Directory
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Force evaluation in the engine.</summary>
	public async Task<Directory> Sync()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("sync", _arguments_);
		await ComputeQuery(_newQueryTree_, await Context.Connection());
		return this;
	}

	///<summary>Retrieves this directory plus a directory written at the given path.</summary>
	///<param name = "Path">Location of the written directory (e.g., "/src/").</param>
	///<param name = "Directory">Identifier of the directory to copy.</param>
	///<param name = "Exclude">Exclude artifacts that match the given pattern (e.g., ["node_modules/", ".git*"]).</param>
	///<param name = "Include">Include only artifacts that match the given pattern (e.g., ["app/", "package.*"]).</param>
	public Directory WithDirectory(string path, Directory directory, IEnumerable<string>? exclude = null, IEnumerable<string>? include = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		_arguments_ = new OperationArgument("directory", new ReferenceOperationArgumentValue(directory), _arguments_);
		if (exclude != null)
			_arguments_ = new OperationArgument("exclude", ArrayOperationArgumentValue.Create(exclude, element => new StringOperationArgumentValue(element)), _arguments_);
		if (include != null)
			_arguments_ = new OperationArgument("include", ArrayOperationArgumentValue.Create(include, element => new StringOperationArgumentValue(element)), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withDirectory", _arguments_);
		return new Directory
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this directory plus the contents of the given file copied to the given path.</summary>
	///<param name = "Path">Location of the copied file (e.g., "/file.txt").</param>
	///<param name = "Source">Identifier of the file to copy.</param>
	///<param name = "Permissions">Permission given to the copied file (e.g., 0600).</param>
	public Directory WithFile(string path, File source, int? permissions = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		_arguments_ = new OperationArgument("source", new ReferenceOperationArgumentValue(source), _arguments_);
		if (permissions != null)
			_arguments_ = new OperationArgument("permissions", EnumOperationArgumentValue.Create(permissions.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withFile", _arguments_);
		return new Directory
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this directory plus a new directory created at the given path.</summary>
	///<param name = "Path">Location of the directory created (e.g., "/logs").</param>
	///<param name = "Permissions">Permission granted to the created directory (e.g., 0777).</param>
	public Directory WithNewDirectory(string path, int? permissions = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		if (permissions != null)
			_arguments_ = new OperationArgument("permissions", EnumOperationArgumentValue.Create(permissions.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withNewDirectory", _arguments_);
		return new Directory
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this directory plus a new file written at the given path.</summary>
	///<param name = "Path">Location of the written file (e.g., "/file.txt").</param>
	///<param name = "Contents">Content of the written file (e.g., "Hello world!").</param>
	///<param name = "Permissions">Permission given to the copied file (e.g., 0600).</param>
	public Directory WithNewFile(string path, string contents, int? permissions = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		_arguments_ = new OperationArgument("contents", new StringOperationArgumentValue(contents), _arguments_);
		if (permissions != null)
			_arguments_ = new OperationArgument("permissions", EnumOperationArgumentValue.Create(permissions.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withNewFile", _arguments_);
		return new Directory
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this directory with all file/dir timestamps set to the given time.</summary>
	///<param name = "Timestamp"><para>Timestamp to set dir/files in.</para><para>Formatted in seconds following Unix epoch (e.g., 1672531199).</para></param>
	public Directory WithTimestamps(int timestamp)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("timestamp", EnumOperationArgumentValue.Create(timestamp), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withTimestamps", _arguments_);
		return new Directory
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this directory with the directory at the given path removed.</summary>
	///<param name = "Path">Location of the directory to remove (e.g., ".github/").</param>
	public Directory WithoutDirectory(string path)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withoutDirectory", _arguments_);
		return new Directory
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves this directory with the file at the given path removed.</summary>
	///<param name = "Path">Location of the file to remove (e.g., "/file.txt").</param>
	public Directory WithoutFile(string path)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withoutFile", _arguments_);
		return new Directory
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}
}

///<summary>An environment variable name and value.</summary>
public sealed class EnvVariable : BaseClient
{
	internal EnvVariableID? CachedId { private get; init; }
	internal string? CachedName { private get; init; }
	internal string? CachedValue { private get; init; }

	///<summary>A unique identifier for this EnvVariable.</summary>
	public async Task<EnvVariableID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary></summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("name", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary></summary>
	public async Task<string> Value()
	{
		if (CachedValue != null)
			return CachedValue;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("value", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}
}

///<summary><para>A definition of a field on a custom object defined in a Module.</para><para>A field on an object has a static value, as opposed to a function on an object whose value is computed by invoking code (and can accept arguments).</para></summary>
public sealed class FieldTypeDef : BaseClient
{
	internal FieldTypeDefID? CachedId { private get; init; }
	internal string? CachedDescription { private get; init; }
	internal string? CachedName { private get; init; }

	///<summary>A unique identifier for this FieldTypeDef.</summary>
	public async Task<FieldTypeDefID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary></summary>
	public async Task<string> Description()
	{
		if (CachedDescription != null)
			return CachedDescription;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("description", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary></summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("name", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary></summary>
	public TypeDef GetTypeDef()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("typeDef", _arguments_);
		return new TypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}
}

///<summary>A file.</summary>
public sealed class File : BaseClient
{
	internal FileID? CachedId { private get; init; }
	internal string? CachedContents { private get; init; }
	internal bool? CachedExport { private get; init; }
	internal string? CachedName { private get; init; }
	internal int? CachedSize { private get; init; }
	internal FileID? CachedSync { private get; init; }

	///<summary>A unique identifier for this File.</summary>
	public async Task<FileID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary>Retrieves the contents of the file.</summary>
	public async Task<string> Contents()
	{
		if (CachedContents != null)
			return CachedContents;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("contents", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary>Writes the file to a file path on the host.</summary>
	///<param name = "Path">Location of the written directory (e.g., "output.txt").</param>
	///<param name = "AllowParentDirPath">If allowParentDirPath is true, the path argument can be a directory path, in which case the file will be created in that directory.</param>
	public async Task<bool> Export(string path, bool? allowParentDirPath = null)
	{
		if (CachedExport != null)
			return CachedExport.Value;
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		if (allowParentDirPath != null)
			_arguments_ = new OperationArgument("allowParentDirPath", EnumOperationArgumentValue.Create(allowParentDirPath.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("export", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<bool>();
	}

	///<summary>Retrieves the name of the file.</summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("name", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary>Retrieves the size of the file, in bytes.</summary>
	public async Task<int> Size()
	{
		if (CachedSize != null)
			return CachedSize.Value;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("size", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<int>();
	}

	///<summary>Force evaluation in the engine.</summary>
	public async Task<File> Sync()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("sync", _arguments_);
		await ComputeQuery(_newQueryTree_, await Context.Connection());
		return this;
	}

	///<summary>Retrieves this file with its created/modified timestamps set to the given time.</summary>
	///<param name = "Timestamp"><para>Timestamp to set dir/files in.</para><para>Formatted in seconds following Unix epoch (e.g., 1672531199).</para></param>
	public File WithTimestamps(int timestamp)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("timestamp", EnumOperationArgumentValue.Create(timestamp), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withTimestamps", _arguments_);
		return new File
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}
}

///<summary><para>Function represents a resolver provided by a Module.</para><para>A function always evaluates against a parent object and is given a set of named arguments.</para></summary>
public sealed class Function : BaseClient
{
	internal FunctionID? CachedId { private get; init; }
	internal string? CachedDescription { private get; init; }
	internal string? CachedName { private get; init; }

	///<summary>A unique identifier for this Function.</summary>
	public async Task<FunctionID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary></summary>
	public async Task<ImmutableArray<FunctionArg>> Args()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("args", _arguments_);
		_newQueryTree_ = _newQueryTree_.Add("id");
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).EnumerateArray().Select(json => new FunctionArg { QueryTree = ImmutableList.Create(new Operation("loadFunctionArgFromID", new OperationArgument("id", new StringOperationArgumentValue(json.GetProperty("id").Deserialize<string>())))), Context = Context, CachedId = new FunctionArgID(json.GetProperty("id").Deserialize<string>()) }).ToImmutableArray();
	}

	///<summary></summary>
	public async Task<string> Description()
	{
		if (CachedDescription != null)
			return CachedDescription;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("description", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary></summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("name", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary></summary>
	public TypeDef ReturnType()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("returnType", _arguments_);
		return new TypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Returns the function with the provided argument</summary>
	///<param name = "Name">The name of the argument</param>
	///<param name = "TypeDef">The type of the argument</param>
	///<param name = "Description">A doc string for the argument, if any</param>
	///<param name = "DefaultValue">A default value to use for this argument if not explicitly set by the caller, if any</param>
	public Function WithArg(string name, TypeDef typeDef, string? description = null, JSON? defaultValue = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		_arguments_ = new OperationArgument("typeDef", new ReferenceOperationArgumentValue(typeDef), _arguments_);
		if (description != null)
			_arguments_ = new OperationArgument("description", new StringOperationArgumentValue(description), _arguments_);
		if (defaultValue != null)
			_arguments_ = new OperationArgument("defaultValue", new StringOperationArgumentValue(defaultValue?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withArg", _arguments_);
		return new Function
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Returns the function with the given doc string.</summary>
	///<param name = "Description">The doc string to set.</param>
	public Function WithDescription(string description)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("description", new StringOperationArgumentValue(description), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withDescription", _arguments_);
		return new Function
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}
}

///<summary><para>An argument accepted by a function.</para><para>This is a specification for an argument at function definition time, not an argument passed at function call time.</para></summary>
public sealed class FunctionArg : BaseClient
{
	internal FunctionArgID? CachedId { private get; init; }
	internal JSON? CachedDefaultValue { private get; init; }
	internal string? CachedDescription { private get; init; }
	internal string? CachedName { private get; init; }

	///<summary>A unique identifier for this FunctionArg.</summary>
	public async Task<FunctionArgID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary></summary>
	public async Task<JSON> DefaultValue()
	{
		if (CachedDefaultValue != null)
			return CachedDefaultValue;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("defaultValue", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary></summary>
	public async Task<string> Description()
	{
		if (CachedDescription != null)
			return CachedDescription;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("description", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary></summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("name", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary></summary>
	public TypeDef GetTypeDef()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("typeDef", _arguments_);
		return new TypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}
}

///<summary>An active function call.</summary>
public sealed class FunctionCall : BaseClient
{
	internal FunctionCallID? CachedId { private get; init; }
	internal string? CachedName { private get; init; }
	internal JSON? CachedParent { private get; init; }
	internal string? CachedParentName { private get; init; }
	internal Void? CachedReturnValue { private get; init; }

	///<summary>A unique identifier for this FunctionCall.</summary>
	public async Task<FunctionCallID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary></summary>
	public async Task<ImmutableArray<FunctionCallArgValue>> InputArgs()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("inputArgs", _arguments_);
		_newQueryTree_ = _newQueryTree_.Add("id");
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).EnumerateArray().Select(json => new FunctionCallArgValue { QueryTree = ImmutableList.Create(new Operation("loadFunctionCallArgValueFromID", new OperationArgument("id", new StringOperationArgumentValue(json.GetProperty("id").Deserialize<string>())))), Context = Context, CachedId = new FunctionCallArgValueID(json.GetProperty("id").Deserialize<string>()) }).ToImmutableArray();
	}

	///<summary></summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("name", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary></summary>
	public async Task<JSON> Parent()
	{
		if (CachedParent != null)
			return CachedParent;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("parent", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary></summary>
	public async Task<string> ParentName()
	{
		if (CachedParentName != null)
			return CachedParentName;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("parentName", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary>Set the return value of the function call to the provided value.</summary>
	///<param name = "Value">JSON serialization of the return value.</param>
	public async Task<Void?> ReturnValue(JSON value)
	{
		if (CachedReturnValue != null)
			return CachedReturnValue;
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("value", new StringOperationArgumentValue(value?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("returnValue", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}
}

///<summary>A value passed as a named argument to a function call.</summary>
public sealed class FunctionCallArgValue : BaseClient
{
	internal FunctionCallArgValueID? CachedId { private get; init; }
	internal string? CachedName { private get; init; }
	internal JSON? CachedValue { private get; init; }

	///<summary>A unique identifier for this FunctionCallArgValue.</summary>
	public async Task<FunctionCallArgValueID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary></summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("name", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary></summary>
	public async Task<JSON> Value()
	{
		if (CachedValue != null)
			return CachedValue;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("value", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}
}

///<summary>The result of running an SDK's codegen.</summary>
public sealed class GeneratedCode : BaseClient
{
	internal GeneratedCodeID? CachedId { private get; init; }

	///<summary>A unique identifier for this GeneratedCode.</summary>
	public async Task<GeneratedCodeID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary></summary>
	public Directory Code()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("code", _arguments_);
		return new Directory
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary></summary>
	public async Task<ImmutableArray<string>> VcsGeneratedPaths()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("vcsGeneratedPaths", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary></summary>
	public async Task<ImmutableArray<string>> VcsIgnoredPaths()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("vcsIgnoredPaths", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary>Set the list of paths to mark generated in version control.</summary>
	///<param name = "Paths"></param>
	public GeneratedCode WithVCSGeneratedPaths(IEnumerable<string> paths)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("paths", ArrayOperationArgumentValue.Create(paths, element => new StringOperationArgumentValue(element)), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withVCSGeneratedPaths", _arguments_);
		return new GeneratedCode
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Set the list of paths to ignore in version control.</summary>
	///<param name = "Paths"></param>
	public GeneratedCode WithVCSIgnoredPaths(IEnumerable<string> paths)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("paths", ArrayOperationArgumentValue.Create(paths, element => new StringOperationArgumentValue(element)), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withVCSIgnoredPaths", _arguments_);
		return new GeneratedCode
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}
}

///<summary>A git ref (tag, branch, or commit).</summary>
public sealed class GitRef : BaseClient
{
	internal GitRefID? CachedId { private get; init; }
	internal string? CachedCommit { private get; init; }

	///<summary>A unique identifier for this GitRef.</summary>
	public async Task<GitRefID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary>The resolved commit id at this ref.</summary>
	public async Task<string> Commit()
	{
		if (CachedCommit != null)
			return CachedCommit;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("commit", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary>The filesystem tree at this ref.</summary>
	///<param name = "SshKnownHosts">DEPRECATED: This option should be passed to `git` instead.</param>
	///<param name = "SshAuthSocket">DEPRECATED: This option should be passed to `git` instead.</param>
	public Directory Tree(string? sshKnownHosts = null, Socket? sshAuthSocket = null)
	{
		OperationArgument? _arguments_ = null;
		if (sshKnownHosts != null)
			_arguments_ = new OperationArgument("sshKnownHosts", new StringOperationArgumentValue(sshKnownHosts), _arguments_);
		if (sshAuthSocket != null)
			_arguments_ = new OperationArgument("sshAuthSocket", new ReferenceOperationArgumentValue(sshAuthSocket), _arguments_);
		var _newQueryTree_ = QueryTree.Add("tree", _arguments_);
		return new Directory
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}
}

///<summary>A git repository.</summary>
public sealed class GitRepository : BaseClient
{
	internal GitRepositoryID? CachedId { private get; init; }

	///<summary>A unique identifier for this GitRepository.</summary>
	public async Task<GitRepositoryID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary>Returns details of a branch.</summary>
	///<param name = "Name">Branch's name (e.g., "main").</param>
	public GitRef Branch(string name)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		var _newQueryTree_ = QueryTree.Add("branch", _arguments_);
		return new GitRef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Returns details of a commit.</summary>
	///<param name = "Id">Identifier of the commit (e.g., "b6315d8f2810962c601af73f86831f6866ea798b").</param>
	public GitRef Commit(string id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id), _arguments_);
		var _newQueryTree_ = QueryTree.Add("commit", _arguments_);
		return new GitRef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Returns details of a tag.</summary>
	///<param name = "Name">Tag's name (e.g., "v0.3.9").</param>
	public GitRef Tag(string name)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		var _newQueryTree_ = QueryTree.Add("tag", _arguments_);
		return new GitRef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}
}

///<summary>Information about the host environment.</summary>
public sealed class Host : BaseClient
{
	internal HostID? CachedId { private get; init; }

	///<summary>A unique identifier for this Host.</summary>
	public async Task<HostID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary>Accesses a directory on the host.</summary>
	///<param name = "Path">Location of the directory to access (e.g., ".").</param>
	///<param name = "Exclude">Exclude artifacts that match the given pattern (e.g., ["node_modules/", ".git*"]).</param>
	///<param name = "Include">Include only artifacts that match the given pattern (e.g., ["app/", "package.*"]).</param>
	public Directory Directory(string path, IEnumerable<string>? exclude = null, IEnumerable<string>? include = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		if (exclude != null)
			_arguments_ = new OperationArgument("exclude", ArrayOperationArgumentValue.Create(exclude, element => new StringOperationArgumentValue(element)), _arguments_);
		if (include != null)
			_arguments_ = new OperationArgument("include", ArrayOperationArgumentValue.Create(include, element => new StringOperationArgumentValue(element)), _arguments_);
		var _newQueryTree_ = QueryTree.Add("directory", _arguments_);
		return new Directory
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Accesses a file on the host.</summary>
	///<param name = "Path">Location of the file to retrieve (e.g., "README.md").</param>
	public File File(string path)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		var _newQueryTree_ = QueryTree.Add("file", _arguments_);
		return new File
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Creates a service that forwards traffic to a specified address via the host.</summary>
	///<param name = "Host">Upstream host to forward traffic to.</param>
	///<param name = "Ports"><para>Ports to expose via the service, forwarding through the host network.</para><para>If a port's frontend is unspecified or 0, it defaults to the same as the backend port.</para><para>An empty set of ports is not valid; an error will be returned.</para></param>
	public Service Service(IEnumerable<PortForward> ports, string? host = null)
	{
		OperationArgument? _arguments_ = null;
		if (host != null)
			_arguments_ = new OperationArgument("host", new StringOperationArgumentValue(host), _arguments_);
		_arguments_ = new OperationArgument("ports", ArrayOperationArgumentValue.Create(ports, element => new ObjectOperationArgumentValue(element.AsOperationArguments())), _arguments_);
		var _newQueryTree_ = QueryTree.Add("service", _arguments_);
		return new Service
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary><para>Sets a secret given a user-defined name and the file path on the host, and returns the secret.</para><para>The file is limited to a size of 512000 bytes.</para></summary>
	///<param name = "Name">The user defined name for this secret.</param>
	///<param name = "Path">Location of the file to set as a secret.</param>
	public Secret SetSecretFile(string name, string path)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		var _newQueryTree_ = QueryTree.Add("setSecretFile", _arguments_);
		return new Secret
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Creates a tunnel that forwards traffic from the host to a service.</summary>
	///<param name = "Service">Service to send traffic from the tunnel.</param>
	///<param name = "Ports"><para>Configure explicit port forwarding rules for the tunnel.</para><para>If a port's frontend is unspecified or 0, a random port will be chosen by the host.</para><para>If no ports are given, all of the service's ports are forwarded. If native is true, each port maps to the same port on the host. If native is false, each port maps to a random port chosen by the host.</para><para>If ports are given and native is true, the ports are additive.</para></param>
	///<param name = "Native"><para>Map each service port to the same port on the host, as if the service were running natively.</para><para>Note: enabling may result in port conflicts.</para></param>
	public Service Tunnel(Service service, IEnumerable<PortForward>? ports = null, bool? native = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("service", new ReferenceOperationArgumentValue(service), _arguments_);
		if (ports != null)
			_arguments_ = new OperationArgument("ports", ArrayOperationArgumentValue.Create(ports, element => new ObjectOperationArgumentValue(element.AsOperationArguments())), _arguments_);
		if (native != null)
			_arguments_ = new OperationArgument("native", EnumOperationArgumentValue.Create(native.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("tunnel", _arguments_);
		return new Service
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Accesses a Unix socket on the host.</summary>
	///<param name = "Path">Location of the Unix socket (e.g., "/var/run/docker.sock").</param>
	public Socket UnixSocket(string path)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("path", new StringOperationArgumentValue(path), _arguments_);
		var _newQueryTree_ = QueryTree.Add("unixSocket", _arguments_);
		return new Socket
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}
}

///<summary>A definition of a custom interface defined in a Module.</summary>
public sealed class InterfaceTypeDef : BaseClient
{
	internal InterfaceTypeDefID? CachedId { private get; init; }
	internal string? CachedDescription { private get; init; }
	internal string? CachedName { private get; init; }
	internal string? CachedSourceModuleName { private get; init; }

	///<summary>A unique identifier for this InterfaceTypeDef.</summary>
	public async Task<InterfaceTypeDefID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary></summary>
	public async Task<string> Description()
	{
		if (CachedDescription != null)
			return CachedDescription;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("description", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary></summary>
	public async Task<ImmutableArray<Function>> Functions()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("functions", _arguments_);
		_newQueryTree_ = _newQueryTree_.Add("id");
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).EnumerateArray().Select(json => new Function { QueryTree = ImmutableList.Create(new Operation("loadFunctionFromID", new OperationArgument("id", new StringOperationArgumentValue(json.GetProperty("id").Deserialize<string>())))), Context = Context, CachedId = new FunctionID(json.GetProperty("id").Deserialize<string>()) }).ToImmutableArray();
	}

	///<summary></summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("name", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary></summary>
	public async Task<string> SourceModuleName()
	{
		if (CachedSourceModuleName != null)
			return CachedSourceModuleName;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("sourceModuleName", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}
}

///<summary>A simple key value object that represents a label.</summary>
public sealed class Label : BaseClient
{
	internal LabelID? CachedId { private get; init; }
	internal string? CachedName { private get; init; }
	internal string? CachedValue { private get; init; }

	///<summary>A unique identifier for this Label.</summary>
	public async Task<LabelID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary></summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("name", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary></summary>
	public async Task<string> Value()
	{
		if (CachedValue != null)
			return CachedValue;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("value", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}
}

///<summary>A definition of a list type in a Module.</summary>
public sealed class ListTypeDef : BaseClient
{
	internal ListTypeDefID? CachedId { private get; init; }

	///<summary>A unique identifier for this ListTypeDef.</summary>
	public async Task<ListTypeDefID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary></summary>
	public TypeDef ElementTypeDef()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("elementTypeDef", _arguments_);
		return new TypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}
}

///<summary>A Dagger module.</summary>
public sealed class Module : BaseClient
{
	internal ModuleID? CachedId { private get; init; }
	internal string? CachedDescription { private get; init; }
	internal string? CachedName { private get; init; }
	internal string? CachedSdk { private get; init; }
	internal Void? CachedServe { private get; init; }
	internal string? CachedSourceDirectorySubpath { private get; init; }

	///<summary>A unique identifier for this Module.</summary>
	public async Task<ModuleID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary></summary>
	public async Task<ImmutableArray<Module>> Dependencies()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("dependencies", _arguments_);
		_newQueryTree_ = _newQueryTree_.Add("id");
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).EnumerateArray().Select(json => new Module { QueryTree = ImmutableList.Create(new Operation("loadModuleFromID", new OperationArgument("id", new StringOperationArgumentValue(json.GetProperty("id").Deserialize<string>())))), Context = Context, CachedId = new ModuleID(json.GetProperty("id").Deserialize<string>()) }).ToImmutableArray();
	}

	///<summary></summary>
	public async Task<ImmutableArray<string>> DependencyConfig()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("dependencyConfig", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary></summary>
	public async Task<string?> Description()
	{
		if (CachedDescription != null)
			return CachedDescription;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("description", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string?>();
	}

	///<summary></summary>
	public GeneratedCode GetGeneratedCode()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("generatedCode", _arguments_);
		return new GeneratedCode
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves the module with the objects loaded via its SDK.</summary>
	public Module Initialize()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("initialize", _arguments_);
		return new Module
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary></summary>
	public async Task<ImmutableArray<TypeDef>> Interfaces()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("interfaces", _arguments_);
		_newQueryTree_ = _newQueryTree_.Add("id");
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).EnumerateArray().Select(json => new TypeDef { QueryTree = ImmutableList.Create(new Operation("loadTypeDefFromID", new OperationArgument("id", new StringOperationArgumentValue(json.GetProperty("id").Deserialize<string>())))), Context = Context, CachedId = new TypeDefID(json.GetProperty("id").Deserialize<string>()) }).ToImmutableArray();
	}

	///<summary></summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("name", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary></summary>
	public async Task<ImmutableArray<TypeDef>> Objects()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("objects", _arguments_);
		_newQueryTree_ = _newQueryTree_.Add("id");
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).EnumerateArray().Select(json => new TypeDef { QueryTree = ImmutableList.Create(new Operation("loadTypeDefFromID", new OperationArgument("id", new StringOperationArgumentValue(json.GetProperty("id").Deserialize<string>())))), Context = Context, CachedId = new TypeDefID(json.GetProperty("id").Deserialize<string>()) }).ToImmutableArray();
	}

	///<summary></summary>
	public async Task<string> Sdk()
	{
		if (CachedSdk != null)
			return CachedSdk;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("sdk", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary><para>Serve a module's API in the current session.</para><para>Note: this can only be called once per session. In the future, it could return a stream or service to remove the side effect.</para></summary>
	public async Task<Void?> Serve()
	{
		if (CachedServe != null)
			return CachedServe;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("serve", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary></summary>
	public Directory SourceDirectory()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("sourceDirectory", _arguments_);
		return new Directory
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary></summary>
	public async Task<string> SourceDirectorySubpath()
	{
		if (CachedSourceDirectorySubpath != null)
			return CachedSourceDirectorySubpath;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("sourceDirectorySubpath", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary>This module plus the given Interface type and associated functions</summary>
	///<param name = "Iface"></param>
	public Module WithInterface(TypeDef iface)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("iface", new ReferenceOperationArgumentValue(iface), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withInterface", _arguments_);
		return new Module
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>This module plus the given Object type and associated functions.</summary>
	///<param name = "Object"></param>
	public Module WithObject(TypeDef @object)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("object", new ReferenceOperationArgumentValue(@object), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withObject", _arguments_);
		return new Module
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Retrieves the module with basic configuration loaded, ready for initialization.</summary>
	///<param name = "Directory">The directory containing the module's source code.</param>
	///<param name = "Subpath"><para>An optional subpath of the directory which contains the module's source code.</para><para>This is needed when the module code is in a subdirectory but requires parent directories to be loaded in order to execute. For example, the module source code may need a go.mod, project.toml, package.json, etc. file from a parent directory.</para><para>If not set, the module source code is loaded from the root of the directory.</para></param>
	public Module WithSource(Directory directory, string? subpath = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("directory", new ReferenceOperationArgumentValue(directory), _arguments_);
		if (subpath != null)
			_arguments_ = new OperationArgument("subpath", new StringOperationArgumentValue(subpath), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withSource", _arguments_);
		return new Module
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}
}

///<summary>Static configuration for a module (e.g. parsed contents of dagger.json)</summary>
public sealed class ModuleConfig : BaseClient
{
	internal ModuleConfigID? CachedId { private get; init; }
	internal string? CachedName { private get; init; }
	internal string? CachedRoot { private get; init; }
	internal string? CachedSdk { private get; init; }

	///<summary>A unique identifier for this ModuleConfig.</summary>
	public async Task<ModuleConfigID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary></summary>
	public async Task<ImmutableArray<string>> Dependencies()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("dependencies", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary></summary>
	public async Task<ImmutableArray<string>> Exclude()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("exclude", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary></summary>
	public async Task<ImmutableArray<string>> Include()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("include", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary></summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("name", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary></summary>
	public async Task<string> Root()
	{
		if (CachedRoot != null)
			return CachedRoot;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("root", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary></summary>
	public async Task<string> Sdk()
	{
		if (CachedSdk != null)
			return CachedSdk;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("sdk", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}
}

///<summary>A definition of a custom object defined in a Module.</summary>
public sealed class ObjectTypeDef : BaseClient
{
	internal ObjectTypeDefID? CachedId { private get; init; }
	internal string? CachedDescription { private get; init; }
	internal string? CachedName { private get; init; }
	internal string? CachedSourceModuleName { private get; init; }

	///<summary>A unique identifier for this ObjectTypeDef.</summary>
	public async Task<ObjectTypeDefID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary></summary>
	public Function Constructor()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("constructor", _arguments_);
		return new Function
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary></summary>
	public async Task<string> Description()
	{
		if (CachedDescription != null)
			return CachedDescription;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("description", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary></summary>
	public async Task<ImmutableArray<FieldTypeDef>> Fields()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("fields", _arguments_);
		_newQueryTree_ = _newQueryTree_.Add("id");
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).EnumerateArray().Select(json => new FieldTypeDef { QueryTree = ImmutableList.Create(new Operation("loadFieldTypeDefFromID", new OperationArgument("id", new StringOperationArgumentValue(json.GetProperty("id").Deserialize<string>())))), Context = Context, CachedId = new FieldTypeDefID(json.GetProperty("id").Deserialize<string>()) }).ToImmutableArray();
	}

	///<summary></summary>
	public async Task<ImmutableArray<Function>> Functions()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("functions", _arguments_);
		_newQueryTree_ = _newQueryTree_.Add("id");
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).EnumerateArray().Select(json => new Function { QueryTree = ImmutableList.Create(new Operation("loadFunctionFromID", new OperationArgument("id", new StringOperationArgumentValue(json.GetProperty("id").Deserialize<string>())))), Context = Context, CachedId = new FunctionID(json.GetProperty("id").Deserialize<string>()) }).ToImmutableArray();
	}

	///<summary></summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("name", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary></summary>
	public async Task<string> SourceModuleName()
	{
		if (CachedSourceModuleName != null)
			return CachedSourceModuleName;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("sourceModuleName", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}
}

///<summary>A port exposed by a container.</summary>
public sealed class Port : BaseClient
{
	internal PortID? CachedId { private get; init; }
	internal string? CachedDescription { private get; init; }
	internal int? CachedPort { private get; init; }
	internal NetworkProtocol? CachedProtocol { private get; init; }

	///<summary>A unique identifier for this Port.</summary>
	public async Task<PortID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary></summary>
	public async Task<string?> Description()
	{
		if (CachedDescription != null)
			return CachedDescription;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("description", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string?>();
	}

	///<summary></summary>
	public async Task<int> SubPort()
	{
		if (CachedPort != null)
			return CachedPort.Value;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("port", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<int>();
	}

	///<summary></summary>
	public async Task<NetworkProtocol> Protocol()
	{
		if (CachedProtocol != null)
			return CachedProtocol.Value;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("protocol", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<NetworkProtocol>();
	}
}

///<summary>The root of the DAG.</summary>
public sealed class Client : BaseClient
{
	internal bool? CachedCheckVersionCompatibility { private get; init; }
	internal Platform? CachedDefaultPlatform { private get; init; }

	///<summary>Retrieves a content-addressed blob.</summary>
	///<param name = "Digest">Digest of the blob</param>
	///<param name = "Size">Size of the blob</param>
	///<param name = "MediaType">Media type of the blob</param>
	///<param name = "Uncompressed">Digest of the uncompressed blob</param>
	public Directory Blob(string digest, int size, string mediaType, string uncompressed)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("digest", new StringOperationArgumentValue(digest), _arguments_);
		_arguments_ = new OperationArgument("size", EnumOperationArgumentValue.Create(size), _arguments_);
		_arguments_ = new OperationArgument("mediaType", new StringOperationArgumentValue(mediaType), _arguments_);
		_arguments_ = new OperationArgument("uncompressed", new StringOperationArgumentValue(uncompressed), _arguments_);
		var _newQueryTree_ = QueryTree.Add("blob", _arguments_);
		return new Directory
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Constructs a cache volume for a given cache key.</summary>
	///<param name = "Key">A string identifier to target this cache volume (e.g., "modules-cache").</param>
	public CacheVolume CacheVolume(string key)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("key", new StringOperationArgumentValue(key), _arguments_);
		var _newQueryTree_ = QueryTree.Add("cacheVolume", _arguments_);
		return new CacheVolume
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Checks if the current Dagger Engine is compatible with an SDK's required version.</summary>
	///<param name = "Version">Version required by the SDK.</param>
	public async Task<bool> CheckVersionCompatibility(string version)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("version", new StringOperationArgumentValue(version), _arguments_);
		var _newQueryTree_ = QueryTree.Add("checkVersionCompatibility", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<bool>();
	}

	///<summary><para>Creates a scratch container.</para><para>Optional platform argument initializes new containers to execute and publish as that platform. Platform defaults to that of the builder's host.</para></summary>
	///<param name = "Id">DEPRECATED: Use `loadContainerFromID` instead.</param>
	///<param name = "Platform">Platform to initialize the container with.</param>
	public Container Container(ContainerID? id = null, Platform? platform = null)
	{
		OperationArgument? _arguments_ = null;
		if (id != null)
			_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		if (platform != null)
			_arguments_ = new OperationArgument("platform", new StringOperationArgumentValue(platform?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("container", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary><para>The FunctionCall context that the SDK caller is currently executing in.</para><para>If the caller is not currently executing in a function, this will return an error.</para></summary>
	public FunctionCall CurrentFunctionCall()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("currentFunctionCall", _arguments_);
		return new FunctionCall
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>The module currently being served in the session, if any.</summary>
	public Module CurrentModule()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("currentModule", _arguments_);
		return new Module
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>The TypeDef representations of the objects currently being served in the session.</summary>
	public async Task<ImmutableArray<TypeDef>> CurrentTypeDefs()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("currentTypeDefs", _arguments_);
		_newQueryTree_ = _newQueryTree_.Add("id");
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).EnumerateArray().Select(json => new TypeDef { QueryTree = ImmutableList.Create(new Operation("loadTypeDefFromID", new OperationArgument("id", new StringOperationArgumentValue(json.GetProperty("id").Deserialize<string>())))), Context = Context, CachedId = new TypeDefID(json.GetProperty("id").Deserialize<string>()) }).ToImmutableArray();
	}

	///<summary>The default platform of the engine.</summary>
	public async Task<Platform> DefaultPlatform()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("defaultPlatform", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary>Creates an empty directory.</summary>
	///<param name = "Id">DEPRECATED: Use `loadDirectoryFromID` isntead.</param>
	public Directory Directory(DirectoryID? id = null)
	{
		OperationArgument? _arguments_ = null;
		if (id != null)
			_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("directory", _arguments_);
		return new Directory
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	[Obsolete("Use `loadFileFromID` instead.")]
	///<summary></summary>
	///<param name = "Id"></param>
	public File File(FileID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("file", _arguments_);
		return new File
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Creates a function.</summary>
	///<param name = "Name">Name of the function, in its original format from the implementation language.</param>
	///<param name = "ReturnType">Return type of the function.</param>
	public Function Function(string name, TypeDef returnType)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		_arguments_ = new OperationArgument("returnType", new ReferenceOperationArgumentValue(returnType), _arguments_);
		var _newQueryTree_ = QueryTree.Add("function", _arguments_);
		return new Function
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Create a code generation result, given a directory containing the generated code.</summary>
	///<param name = "Code"></param>
	public GeneratedCode GeneratedCode(Directory code)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("code", new ReferenceOperationArgumentValue(code), _arguments_);
		var _newQueryTree_ = QueryTree.Add("generatedCode", _arguments_);
		return new GeneratedCode
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Queries a Git repository.</summary>
	///<param name = "Url"><para>URL of the git repository.</para><para>Can be formatted as `https://{host}/{owner}/{repo}`, `git@{host}:{owner}/{repo}`.</para><para>Suffix ".git" is optional.</para></param>
	///<param name = "KeepGitDir">Set to true to keep .git directory.</param>
	///<param name = "ExperimentalServiceHost">A service which must be started before the repo is fetched.</param>
	///<param name = "SshKnownHosts">Set SSH known hosts</param>
	///<param name = "SshAuthSocket">Set SSH auth socket</param>
	public GitRepository Git(string url, bool? keepGitDir = null, Service? experimentalServiceHost = null, string? sshKnownHosts = null, Socket? sshAuthSocket = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("url", new StringOperationArgumentValue(url), _arguments_);
		if (keepGitDir != null)
			_arguments_ = new OperationArgument("keepGitDir", EnumOperationArgumentValue.Create(keepGitDir.Value), _arguments_);
		if (experimentalServiceHost != null)
			_arguments_ = new OperationArgument("experimentalServiceHost", new ReferenceOperationArgumentValue(experimentalServiceHost), _arguments_);
		if (sshKnownHosts != null)
			_arguments_ = new OperationArgument("sshKnownHosts", new StringOperationArgumentValue(sshKnownHosts), _arguments_);
		if (sshAuthSocket != null)
			_arguments_ = new OperationArgument("sshAuthSocket", new ReferenceOperationArgumentValue(sshAuthSocket), _arguments_);
		var _newQueryTree_ = QueryTree.Add("git", _arguments_);
		return new GitRepository
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Queries the host environment.</summary>
	public Host GetHost()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("host", _arguments_);
		return new Host
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Returns a file containing an http remote url content.</summary>
	///<param name = "Url">HTTP url to get the content from (e.g., "https://docs.dagger.io").</param>
	///<param name = "ExperimentalServiceHost">A service which must be started before the URL is fetched.</param>
	public File Http(string url, Service? experimentalServiceHost = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("url", new StringOperationArgumentValue(url), _arguments_);
		if (experimentalServiceHost != null)
			_arguments_ = new OperationArgument("experimentalServiceHost", new ReferenceOperationArgumentValue(experimentalServiceHost), _arguments_);
		var _newQueryTree_ = QueryTree.Add("http", _arguments_);
		return new File
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a CacheVolume from its ID.</summary>
	///<param name = "Id"></param>
	public CacheVolume LoadCacheVolumeFromID(CacheVolumeID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadCacheVolumeFromID", _arguments_);
		return new CacheVolume
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a Container from its ID.</summary>
	///<param name = "Id"></param>
	public Container LoadContainerFromID(ContainerID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadContainerFromID", _arguments_);
		return new Container
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a Directory from its ID.</summary>
	///<param name = "Id"></param>
	public Directory LoadDirectoryFromID(DirectoryID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadDirectoryFromID", _arguments_);
		return new Directory
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a EnvVariable from its ID.</summary>
	///<param name = "Id"></param>
	public EnvVariable LoadEnvVariableFromID(EnvVariableID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadEnvVariableFromID", _arguments_);
		return new EnvVariable
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a FieldTypeDef from its ID.</summary>
	///<param name = "Id"></param>
	public FieldTypeDef LoadFieldTypeDefFromID(FieldTypeDefID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadFieldTypeDefFromID", _arguments_);
		return new FieldTypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a File from its ID.</summary>
	///<param name = "Id"></param>
	public File LoadFileFromID(FileID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadFileFromID", _arguments_);
		return new File
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a FunctionArg from its ID.</summary>
	///<param name = "Id"></param>
	public FunctionArg LoadFunctionArgFromID(FunctionArgID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadFunctionArgFromID", _arguments_);
		return new FunctionArg
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a FunctionCallArgValue from its ID.</summary>
	///<param name = "Id"></param>
	public FunctionCallArgValue LoadFunctionCallArgValueFromID(FunctionCallArgValueID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadFunctionCallArgValueFromID", _arguments_);
		return new FunctionCallArgValue
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a FunctionCall from its ID.</summary>
	///<param name = "Id"></param>
	public FunctionCall LoadFunctionCallFromID(FunctionCallID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadFunctionCallFromID", _arguments_);
		return new FunctionCall
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a Function from its ID.</summary>
	///<param name = "Id"></param>
	public Function LoadFunctionFromID(FunctionID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadFunctionFromID", _arguments_);
		return new Function
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a GeneratedCode from its ID.</summary>
	///<param name = "Id"></param>
	public GeneratedCode LoadGeneratedCodeFromID(GeneratedCodeID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadGeneratedCodeFromID", _arguments_);
		return new GeneratedCode
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a GitRef from its ID.</summary>
	///<param name = "Id"></param>
	public GitRef LoadGitRefFromID(GitRefID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadGitRefFromID", _arguments_);
		return new GitRef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a GitRepository from its ID.</summary>
	///<param name = "Id"></param>
	public GitRepository LoadGitRepositoryFromID(GitRepositoryID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadGitRepositoryFromID", _arguments_);
		return new GitRepository
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a Host from its ID.</summary>
	///<param name = "Id"></param>
	public Host LoadHostFromID(HostID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadHostFromID", _arguments_);
		return new Host
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a InterfaceTypeDef from its ID.</summary>
	///<param name = "Id"></param>
	public InterfaceTypeDef LoadInterfaceTypeDefFromID(InterfaceTypeDefID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadInterfaceTypeDefFromID", _arguments_);
		return new InterfaceTypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a Label from its ID.</summary>
	///<param name = "Id"></param>
	public Label LoadLabelFromID(LabelID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadLabelFromID", _arguments_);
		return new Label
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a ListTypeDef from its ID.</summary>
	///<param name = "Id"></param>
	public ListTypeDef LoadListTypeDefFromID(ListTypeDefID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadListTypeDefFromID", _arguments_);
		return new ListTypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a ModuleConfig from its ID.</summary>
	///<param name = "Id"></param>
	public ModuleConfig LoadModuleConfigFromID(ModuleConfigID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadModuleConfigFromID", _arguments_);
		return new ModuleConfig
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a Module from its ID.</summary>
	///<param name = "Id"></param>
	public Module LoadModuleFromID(ModuleID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadModuleFromID", _arguments_);
		return new Module
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a ObjectTypeDef from its ID.</summary>
	///<param name = "Id"></param>
	public ObjectTypeDef LoadObjectTypeDefFromID(ObjectTypeDefID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadObjectTypeDefFromID", _arguments_);
		return new ObjectTypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a Port from its ID.</summary>
	///<param name = "Id"></param>
	public Port LoadPortFromID(PortID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadPortFromID", _arguments_);
		return new Port
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a Secret from its ID.</summary>
	///<param name = "Id"></param>
	public Secret LoadSecretFromID(SecretID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadSecretFromID", _arguments_);
		return new Secret
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a Service from its ID.</summary>
	///<param name = "Id"></param>
	public Service LoadServiceFromID(ServiceID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadServiceFromID", _arguments_);
		return new Service
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a Socket from its ID.</summary>
	///<param name = "Id"></param>
	public Socket LoadSocketFromID(SocketID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadSocketFromID", _arguments_);
		return new Socket
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load a TypeDef from its ID.</summary>
	///<param name = "Id"></param>
	public TypeDef LoadTypeDefFromID(TypeDefID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("loadTypeDefFromID", _arguments_);
		return new TypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Create a new module.</summary>
	public Module GetModule()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("module", _arguments_);
		return new Module
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Load the static configuration for a module from the given source directory and optional subpath.</summary>
	///<param name = "SourceDirectory"></param>
	///<param name = "Subpath"></param>
	public ModuleConfig ModuleConfig(Directory sourceDirectory, string? subpath = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("sourceDirectory", new ReferenceOperationArgumentValue(sourceDirectory), _arguments_);
		if (subpath != null)
			_arguments_ = new OperationArgument("subpath", new StringOperationArgumentValue(subpath), _arguments_);
		var _newQueryTree_ = QueryTree.Add("moduleConfig", _arguments_);
		return new ModuleConfig
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Creates a named sub-pipeline.</summary>
	///<param name = "Name">Name of the sub-pipeline.</param>
	///<param name = "Description">Description of the sub-pipeline.</param>
	///<param name = "Labels">Labels to apply to the sub-pipeline.</param>
	public Client Pipeline(string name, string? description = null, IEnumerable<PipelineLabel>? labels = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		if (description != null)
			_arguments_ = new OperationArgument("description", new StringOperationArgumentValue(description), _arguments_);
		if (labels != null)
			_arguments_ = new OperationArgument("labels", ArrayOperationArgumentValue.Create(labels, element => new ObjectOperationArgumentValue(element.AsOperationArguments())), _arguments_);
		var _newQueryTree_ = QueryTree.Add("pipeline", _arguments_);
		return new Client
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Reference a secret by name.</summary>
	///<param name = "Name"></param>
	public Secret Secret(string name)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		var _newQueryTree_ = QueryTree.Add("secret", _arguments_);
		return new Secret
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary><para>Sets a secret given a user defined name to its plaintext and returns the secret.</para><para>The plaintext value is limited to a size of 128000 bytes.</para></summary>
	///<param name = "Name">The user defined name for this secret</param>
	///<param name = "Plaintext">The plaintext of the secret</param>
	public Secret SetSecret(string name, string plaintext)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		_arguments_ = new OperationArgument("plaintext", new StringOperationArgumentValue(plaintext), _arguments_);
		var _newQueryTree_ = QueryTree.Add("setSecret", _arguments_);
		return new Secret
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	[Obsolete("Use `loadSocketFromID` instead.")]
	///<summary>Loads a socket by its ID.</summary>
	///<param name = "Id"></param>
	public Socket Socket(SocketID id)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("id", new StringOperationArgumentValue(id?.Value), _arguments_);
		var _newQueryTree_ = QueryTree.Add("socket", _arguments_);
		return new Socket
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Create a new TypeDef.</summary>
	public TypeDef GetTypeDef()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("typeDef", _arguments_);
		return new TypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	public static Client Default { get; } = new()
	{
		Context = Context.Default
	};
}

///<summary>A reference to a secret value, which can be handled more safely than the value itself.</summary>
public sealed class Secret : BaseClient
{
	internal SecretID? CachedId { private get; init; }
	internal string? CachedPlaintext { private get; init; }

	///<summary>A unique identifier for this Secret.</summary>
	public async Task<SecretID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary>The value of this secret.</summary>
	public async Task<string> Plaintext()
	{
		if (CachedPlaintext != null)
			return CachedPlaintext;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("plaintext", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}
}

///<summary>A content-addressed service providing TCP connectivity.</summary>
public sealed class Service : BaseClient
{
	internal ServiceID? CachedId { private get; init; }
	internal string? CachedEndpoint { private get; init; }
	internal string? CachedHostname { private get; init; }
	internal ServiceID? CachedStart { private get; init; }
	internal ServiceID? CachedStop { private get; init; }

	///<summary>A unique identifier for this Service.</summary>
	public async Task<ServiceID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary><para>Retrieves an endpoint that clients can use to reach this container.</para><para>If no port is specified, the first exposed port is used. If none exist an error is returned.</para><para>If a scheme is specified, a URL is returned. Otherwise, a host:port pair is returned.</para></summary>
	///<param name = "Port">The exposed port number for the endpoint</param>
	///<param name = "Scheme">Return a URL with the given scheme, eg. http for http://</param>
	public async Task<string> Endpoint(int? port = null, string? scheme = null)
	{
		if (CachedEndpoint != null)
			return CachedEndpoint;
		OperationArgument? _arguments_ = null;
		if (port != null)
			_arguments_ = new OperationArgument("port", EnumOperationArgumentValue.Create(port.Value), _arguments_);
		if (scheme != null)
			_arguments_ = new OperationArgument("scheme", new StringOperationArgumentValue(scheme), _arguments_);
		var _newQueryTree_ = QueryTree.Add("endpoint", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary>Retrieves a hostname which can be used by clients to reach this container.</summary>
	public async Task<string> Hostname()
	{
		if (CachedHostname != null)
			return CachedHostname;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("hostname", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>();
	}

	///<summary>Retrieves the list of ports provided by the service.</summary>
	public async Task<ImmutableArray<Port>> Ports()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("ports", _arguments_);
		_newQueryTree_ = _newQueryTree_.Add("id");
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).EnumerateArray().Select(json => new Port { QueryTree = ImmutableList.Create(new Operation("loadPortFromID", new OperationArgument("id", new StringOperationArgumentValue(json.GetProperty("id").Deserialize<string>())))), Context = Context, CachedId = new PortID(json.GetProperty("id").Deserialize<string>()) }).ToImmutableArray();
	}

	///<summary><para>Start the service and wait for its health checks to succeed.</para><para>Services bound to a Container do not need to be manually started.</para></summary>
	public async Task<Service> Start()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("start", _arguments_);
		await ComputeQuery(_newQueryTree_, await Context.Connection());
		return this;
	}

	///<summary>Stop the service.</summary>
	public async Task<Service> Stop()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("stop", _arguments_);
		await ComputeQuery(_newQueryTree_, await Context.Connection());
		return this;
	}
}

///<summary>A Unix or TCP/IP socket that can be mounted into a container.</summary>
public sealed class Socket : BaseClient
{
	internal SocketID? CachedId { private get; init; }

	///<summary>A unique identifier for this Socket.</summary>
	public async Task<SocketID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}
}

///<summary>A definition of a parameter or return type in a Module.</summary>
public sealed class TypeDef : BaseClient
{
	internal TypeDefID? CachedId { private get; init; }
	internal TypeDefKind? CachedKind { private get; init; }
	internal bool? CachedOptional { private get; init; }

	///<summary>A unique identifier for this TypeDef.</summary>
	public async Task<TypeDefID> Id()
	{
		if (CachedId != null)
			return CachedId;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("id", _arguments_);
		return new((await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<string>());
	}

	///<summary></summary>
	public InterfaceTypeDef AsInterface()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("asInterface", _arguments_);
		return new InterfaceTypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary></summary>
	public ListTypeDef AsList()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("asList", _arguments_);
		return new ListTypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary></summary>
	public ObjectTypeDef AsObject()
	{
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("asObject", _arguments_);
		return new ObjectTypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary></summary>
	public async Task<TypeDefKind> Kind()
	{
		if (CachedKind != null)
			return CachedKind.Value;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("kind", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<TypeDefKind>();
	}

	///<summary></summary>
	public async Task<bool> IsOptional()
	{
		if (CachedOptional != null)
			return CachedOptional.Value;
		OperationArgument? _arguments_ = null;
		var _newQueryTree_ = QueryTree.Add("optional", _arguments_);
		return (await ComputeQuery(_newQueryTree_, await Context.Connection())).Deserialize<bool>();
	}

	///<summary>Adds a function for constructing a new instance of an Object TypeDef, failing if the type is not an object.</summary>
	///<param name = "Function"></param>
	public TypeDef WithConstructor(Function function)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("function", new ReferenceOperationArgumentValue(function), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withConstructor", _arguments_);
		return new TypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Adds a static field for an Object TypeDef, failing if the type is not an object.</summary>
	///<param name = "Name">The name of the field in the object</param>
	///<param name = "TypeDef">The type of the field</param>
	///<param name = "Description">A doc string for the field, if any</param>
	public TypeDef WithField(string name, TypeDef typeDef, string? description = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		_arguments_ = new OperationArgument("typeDef", new ReferenceOperationArgumentValue(typeDef), _arguments_);
		if (description != null)
			_arguments_ = new OperationArgument("description", new StringOperationArgumentValue(description), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withField", _arguments_);
		return new TypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Adds a function for an Object or Interface TypeDef, failing if the type is not one of those kinds.</summary>
	///<param name = "Function"></param>
	public TypeDef WithFunction(Function function)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("function", new ReferenceOperationArgumentValue(function), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withFunction", _arguments_);
		return new TypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Returns a TypeDef of kind Interface with the provided name.</summary>
	///<param name = "Name"></param>
	///<param name = "Description"></param>
	public TypeDef WithInterface(string name, string? description = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		if (description != null)
			_arguments_ = new OperationArgument("description", new StringOperationArgumentValue(description), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withInterface", _arguments_);
		return new TypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Sets the kind of the type.</summary>
	///<param name = "Kind"></param>
	public TypeDef WithKind(TypeDefKind kind)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("kind", EnumOperationArgumentValue.Create(kind), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withKind", _arguments_);
		return new TypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Returns a TypeDef of kind List with the provided type for its elements.</summary>
	///<param name = "ElementType"></param>
	public TypeDef WithListOf(TypeDef elementType)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("elementType", new ReferenceOperationArgumentValue(elementType), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withListOf", _arguments_);
		return new TypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary><para>Returns a TypeDef of kind Object with the provided name.</para><para>Note that an object's fields and functions may be omitted if the intent is only to refer to an object. This is how functions are able to return their own object, or any other circular reference.</para></summary>
	///<param name = "Name"></param>
	///<param name = "Description"></param>
	public TypeDef WithObject(string name, string? description = null)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("name", new StringOperationArgumentValue(name), _arguments_);
		if (description != null)
			_arguments_ = new OperationArgument("description", new StringOperationArgumentValue(description), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withObject", _arguments_);
		return new TypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}

	///<summary>Sets whether this type can be set to null.</summary>
	///<param name = "Optional"></param>
	public TypeDef WithOptional(bool optional)
	{
		OperationArgument? _arguments_ = null;
		_arguments_ = new OperationArgument("optional", EnumOperationArgumentValue.Create(optional), _arguments_);
		var _newQueryTree_ = QueryTree.Add("withOptional", _arguments_);
		return new TypeDef
		{
			QueryTree = _newQueryTree_,
			Context = Context
		};
	}
}// This file was auto-generated by DaggerSDKCodeGen
// Do not make direct changes to this file.
