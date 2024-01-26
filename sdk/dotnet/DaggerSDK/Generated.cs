using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using static DaggerSDK.APIUtils;

namespace DaggerSDK;
///<param name = "Name">The build argument name.</param>
///<param name = "Value">The build argument value.</param>
public sealed record BuildArg(string Name, string Value);
///<summary>Sharing mode of the cache volume.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<CacheSharingMode>))]
public enum CacheSharingMode
{
	LOCKED,
	PRIVATE,
	SHARED
}

///<summary>A global cache volume identifier.</summary>
public sealed record CacheVolumeID(string Value);
///<summary>A unique container identifier. Null designates an empty container (scratch).</summary>
public sealed record ContainerID(string Value);
///<summary>The `DateTime` scalar type represents a DateTime. The DateTime is serialized as an RFC 3339 quoted string</summary>
public sealed record DateTime(string Value);
///<summary>A content-addressed directory identifier.</summary>
public sealed record DirectoryID(string Value);
///<summary>A file identifier.</summary>
public sealed record FileID(string Value);
///<summary>A reference to a FunctionArg.</summary>
public sealed record FunctionArgID(string Value);
///<summary>A reference to a Function.</summary>
public sealed record FunctionID(string Value);
///<summary>A reference to GeneratedCode.</summary>
public sealed record GeneratedCodeID(string Value);
///<summary>A git reference identifier.</summary>
public sealed record GitRefID(string Value);
///<summary>A git repository identifier.</summary>
public sealed record GitRepositoryID(string Value);
///<summary>The `ID` scalar type represents a unique identifier, often used to refetch an object or as key for a cache. The ID type appears in a JSON response as a String; however, it is not intended to be human-readable. When expected as an input type, any string (such as `"4"`) or integer (such as `4`) input value will be accepted as an ID.</summary>
public sealed record ID(string Value);
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

///<summary>An arbitrary JSON-encoded value.</summary>
public sealed record JSON(string Value);
///<summary>A reference to a Module.</summary>
public sealed record ModuleID(string Value);
///<summary>Transport layer network protocol associated to a port.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<NetworkProtocol>))]
public enum NetworkProtocol
{
	TCP,
	UDP
}

///<param name = "Name">Label name.</param>
///<param name = "Value">Label value.</param>
public sealed record PipelineLabel(string Name, string Value);
///<summary><para>The platform config OS and architecture in a Container.</para><para>The format is [os]/[platform]/[version] (e.g., "darwin/arm64/v7", "windows/amd64", "linux/arm64").</para></summary>
public sealed record Platform(string Value);
///<param name = "Backend">Destination port for traffic.</param>
///<param name = "Frontend">Port to expose to clients. If unspecified, a default will be chosen.</param>
///<param name = "Protocol">Protocol to use for traffic.</param>
public sealed record PortForward(int Backend, int? Frontend, NetworkProtocol? Protocol);
///<summary>A unique identifier for a secret.</summary>
public sealed record SecretID(string Value);
///<summary>A unique service identifier.</summary>
public sealed record ServiceID(string Value);
///<summary>A content-addressed socket identifier.</summary>
public sealed record SocketID(string Value);
///<summary>A reference to a TypeDef.</summary>
public sealed record TypeDefID(string Value);
///<summary>Distinguishes the different kinds of TypeDefs.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<TypeDefKind>))]
public enum TypeDefKind
{
	BooleanKind,
	IntegerKind,
	ListKind,
	ObjectKind,
	StringKind,
	VoidKind
}

///<summary><para>The absense of a value.</para><para>A Null Void is used as a placeholder for resolvers that do not return anything.</para></summary>
public sealed record Void(string Value);
///<summary>A directory whose contents persist across runs.</summary>
public sealed class CacheVolume : BaseClient
{
	internal CacheVolumeID? CachedId { private get; init; }

	///<summary></summary>
	public async Task<CacheVolumeID> Id()
	{
		if (CachedId != null)
			return CachedId;
		return new((await ComputeQuery(QueryTree.Add("id"), await Context.Connection())).Deserialize<string>());
	}
}

///<summary>An OCI-compatible container, also known as a docker container.</summary>
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

	///<summary>A unique identifier for this container.</summary>
	public async Task<ContainerID> Id()
	{
		if (CachedId != null)
			return CachedId;
		return new((await ComputeQuery(QueryTree.Add("id"), await Context.Connection())).Deserialize<string>());
	}

	///<summary><para>Turn the container into a Service.</para><para>Be sure to set any exposed ports before this conversion.</para></summary>
	public Service AsService()
	{
		return new Service
		{
			QueryTree = QueryTree.Add("asService"),
			Context = Context
		};
	}

	///<summary>Returns a File representing the container serialized to a tarball.</summary>
	///<param name = "PlatformVariants">Identifiers for other platform specific containers. Used for multi-platform image.</param>
	///<param name = "ForcedCompression">Force each layer of the image to use the specified compression algorithm. If this is unset, then if a layer already has a compressed blob in the engine's cache, that will be used (this can result in a mix of compression algorithms for different layers). If this is unset and a layer has no compressed blob in the engine's cache, then it will be compressed using Gzip.</param>
	///<param name = "MediaTypes">Use the specified media types for the image's layers. Defaults to OCI, which is largely compatible with most recent container runtimes, but Docker may be needed for older runtimes without OCI support.</param>
	public File AsTarball(IReadOnlyList<Container>? platformVariants = default, ImageLayerCompression? forcedCompression = default, ImageMediaTypes? mediaTypes = default)
	{
		return new File
		{
			QueryTree = QueryTree.Add("asTarball", new OperationArgument("platformVariants", platformVariants, ParameterSerialization.Reference, true), new OperationArgument("forcedCompression", forcedCompression, ParameterSerialization.Enum, false), new OperationArgument("mediaTypes", mediaTypes, ParameterSerialization.Enum, false)),
			Context = Context
		};
	}

	///<summary>Initializes this container from a Dockerfile build.</summary>
	///<param name = "Context">Directory context used by the Dockerfile.</param>
	///<param name = "Dockerfile"><para>Path to the Dockerfile to use.</para><para>Default: './Dockerfile'.</para></param>
	///<param name = "BuildArgs">Additional build arguments.</param>
	///<param name = "Target">Target build stage to build.</param>
	///<param name = "Secrets"><para>Secrets to pass to the build.</para><para>They will be mounted at /run/secrets/[secret-name] in the build container</para><para>They can be accessed in the Dockerfile using the "secret" mount type and mount path /run/secrets/[secret-name] e.g. RUN --mount=type=secret,id=my-secret curl url?token=$(cat /run/secrets/my-secret)"</para></param>
	public Container Build(Directory context, string? dockerfile = default, IReadOnlyList<BuildArg>? buildArgs = default, string? target = default, IReadOnlyList<Secret>? secrets = default)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("build", new OperationArgument("context", context, ParameterSerialization.Reference, false), new OperationArgument("dockerfile", dockerfile, ParameterSerialization.String, false), new OperationArgument("buildArgs", buildArgs, ParameterSerialization.Object, true), new OperationArgument("target", target, ParameterSerialization.String, false), new OperationArgument("secrets", secrets, ParameterSerialization.Reference, true)),
			Context = Context
		};
	}

	///<summary>Retrieves default arguments for future commands.</summary>
	public async Task<ImmutableArray<string>> DefaultArgs()
	{
		return (await ComputeQuery(QueryTree.Add("defaultArgs"), await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary><para>Retrieves a directory at the given path.</para><para>Mounts are included.</para></summary>
	///<param name = "Path">The path of the directory to retrieve (e.g., "./src").</param>
	public Directory Directory(string path)
	{
		return new Directory
		{
			QueryTree = QueryTree.Add("directory", new OperationArgument("path", path, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Retrieves entrypoint to be prepended to the arguments of all commands.</summary>
	public async Task<ImmutableArray<string>> Entrypoint()
	{
		return (await ComputeQuery(QueryTree.Add("entrypoint"), await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary>Retrieves the value of the specified environment variable.</summary>
	///<param name = "Name">The name of the environment variable to retrieve (e.g., "PATH").</param>
	public async Task<string?> EnvVariable(string name)
	{
		if (CachedEnvVariable != null)
			return CachedEnvVariable;
		return (await ComputeQuery(QueryTree.Add("envVariable", new OperationArgument("name", name, ParameterSerialization.String, false)), await Context.Connection())).Deserialize<string?>();
	}

	///<summary>Retrieves the list of environment variables passed to commands.</summary>
	public async Task<ImmutableArray<EnvVariable>> EnvVariables()
	{
		return (await ComputeQuery(QueryTree.Add("envVariables").Add("name value"), await Context.Connection())).EnumerateArray().Select(json => new EnvVariable { QueryTree = QueryTree, Context = Context, CachedName = json.GetProperty("name").Deserialize<string>(), CachedValue = json.GetProperty("value").Deserialize<string>() }).ToImmutableArray();
	}

	///<summary><para>EXPERIMENTAL API! Subject to change/removal at any time.</para><para>experimentalWithAllGPUs configures all available GPUs on the host to be accessible to this container. This currently works for Nvidia devices only.</para></summary>
	public Container ExperimentalWithAllGPUs()
	{
		return new Container
		{
			QueryTree = QueryTree.Add("experimentalWithAllGPUs"),
			Context = Context
		};
	}

	///<summary><para>EXPERIMENTAL API! Subject to change/removal at any time.</para><para>experimentalWithGPU configures the provided list of devices to be accesible to this container. This currently works for Nvidia devices only.</para></summary>
	///<param name = "Devices"></param>
	public Container ExperimentalWithGPU(IReadOnlyList<string> devices)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("experimentalWithGPU", new OperationArgument("devices", devices, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary><para>Writes the container as an OCI tarball to the destination file path on the host for the specified platform variants.</para><para>Return true on success. It can also publishes platform variants.</para></summary>
	///<param name = "Path">Host's destination path (e.g., "./tarball"). Path can be relative to the engine's workdir or absolute.</param>
	///<param name = "PlatformVariants">Identifiers for other platform specific containers. Used for multi-platform image.</param>
	///<param name = "ForcedCompression">Force each layer of the exported image to use the specified compression algorithm. If this is unset, then if a layer already has a compressed blob in the engine's cache, that will be used (this can result in a mix of compression algorithms for different layers). If this is unset and a layer has no compressed blob in the engine's cache, then it will be compressed using Gzip.</param>
	///<param name = "MediaTypes">Use the specified media types for the exported image's layers. Defaults to OCI, which is largely compatible with most recent container runtimes, but Docker may be needed for older runtimes without OCI support.</param>
	public async Task<bool> Export(string path, IReadOnlyList<Container>? platformVariants = default, ImageLayerCompression? forcedCompression = default, ImageMediaTypes? mediaTypes = default)
	{
		if (CachedExport != null)
			return CachedExport.Value;
		return (await ComputeQuery(QueryTree.Add("export", new OperationArgument("path", path, ParameterSerialization.String, false), new OperationArgument("platformVariants", platformVariants, ParameterSerialization.Reference, true), new OperationArgument("forcedCompression", forcedCompression, ParameterSerialization.Enum, false), new OperationArgument("mediaTypes", mediaTypes, ParameterSerialization.Enum, false)), await Context.Connection())).Deserialize<bool>();
	}

	///<summary><para>Retrieves the list of exposed ports.</para><para>This includes ports already exposed by the image, even if not explicitly added with dagger.</para></summary>
	public async Task<ImmutableArray<Port>> ExposedPorts()
	{
		return (await ComputeQuery(QueryTree.Add("exposedPorts").Add("description port protocol"), await Context.Connection())).EnumerateArray().Select(json => new Port { QueryTree = QueryTree, Context = Context, CachedDescription = json.GetProperty("description").Deserialize<string?>(), CachedPort = json.GetProperty("port").Deserialize<int>(), CachedProtocol = json.GetProperty("protocol").Deserialize<NetworkProtocol>() }).ToImmutableArray();
	}

	///<summary><para>Retrieves a file at the given path.</para><para>Mounts are included.</para></summary>
	///<param name = "Path">The path of the file to retrieve (e.g., "./README.md").</param>
	public File File(string path)
	{
		return new File
		{
			QueryTree = QueryTree.Add("file", new OperationArgument("path", path, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Initializes this container from a pulled base image.</summary>
	///<param name = "Address"><para>Image's address from its registry.</para><para>Formatted as [host]/[user]/[repo]:[tag] (e.g., "docker.io/dagger/dagger:main").</para></param>
	public Container From(string address)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("from", new OperationArgument("address", address, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>The unique image reference which can only be retrieved immediately after the 'Container.From' call.</summary>
	public async Task<string?> ImageRef()
	{
		if (CachedImageRef != null)
			return CachedImageRef;
		return (await ComputeQuery(QueryTree.Add("imageRef"), await Context.Connection())).Deserialize<string?>();
	}

	///<summary><para>Reads the container from an OCI tarball.</para><para>NOTE: this involves unpacking the tarball to an OCI store on the host at $XDG_CACHE_DIR/dagger/oci. This directory can be removed whenever you like.</para></summary>
	///<param name = "Source">File to read the container from.</param>
	///<param name = "Tag">Identifies the tag to import from the archive, if the archive bundles multiple tags.</param>
	public Container Import(File source, string? tag = default)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("import", new OperationArgument("source", source, ParameterSerialization.Reference, false), new OperationArgument("tag", tag, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Retrieves the value of the specified label.</summary>
	///<param name = "Name"></param>
	public async Task<string?> Label(string name)
	{
		if (CachedLabel != null)
			return CachedLabel;
		return (await ComputeQuery(QueryTree.Add("label", new OperationArgument("name", name, ParameterSerialization.String, false)), await Context.Connection())).Deserialize<string?>();
	}

	///<summary>Retrieves the list of labels passed to container.</summary>
	public async Task<ImmutableArray<Label>> Labels()
	{
		return (await ComputeQuery(QueryTree.Add("labels").Add("name value"), await Context.Connection())).EnumerateArray().Select(json => new Label { QueryTree = QueryTree, Context = Context, CachedName = json.GetProperty("name").Deserialize<string>(), CachedValue = json.GetProperty("value").Deserialize<string>() }).ToImmutableArray();
	}

	///<summary>Retrieves the list of paths where a directory is mounted.</summary>
	public async Task<ImmutableArray<string>> Mounts()
	{
		return (await ComputeQuery(QueryTree.Add("mounts"), await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary>Creates a named sub-pipeline</summary>
	///<param name = "Name">Pipeline name.</param>
	///<param name = "Description">Pipeline description.</param>
	///<param name = "Labels">Pipeline labels.</param>
	public Container Pipeline(string name, string? description = default, IReadOnlyList<PipelineLabel>? labels = default)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("pipeline", new OperationArgument("name", name, ParameterSerialization.String, false), new OperationArgument("description", description, ParameterSerialization.String, false), new OperationArgument("labels", labels, ParameterSerialization.Object, true)),
			Context = Context
		};
	}

	///<summary>The platform this container executes and publishes as.</summary>
	public async Task<Platform> GetPlatform()
	{
		if (CachedPlatform != null)
			return CachedPlatform;
		return new((await ComputeQuery(QueryTree.Add("platform"), await Context.Connection())).Deserialize<string>());
	}

	///<summary><para>Publishes this container as a new image to the specified address.</para><para>Publish returns a fully qualified ref. It can also publish platform variants.</para></summary>
	///<param name = "Address"><para>Registry's address to publish the image to.</para><para>Formatted as [host]/[user]/[repo]:[tag] (e.g. "docker.io/dagger/dagger:main").</para></param>
	///<param name = "PlatformVariants">Identifiers for other platform specific containers. Used for multi-platform image.</param>
	///<param name = "ForcedCompression">Force each layer of the published image to use the specified compression algorithm. If this is unset, then if a layer already has a compressed blob in the engine's cache, that will be used (this can result in a mix of compression algorithms for different layers). If this is unset and a layer has no compressed blob in the engine's cache, then it will be compressed using Gzip.</param>
	///<param name = "MediaTypes">Use the specified media types for the published image's layers. Defaults to OCI, which is largely compatible with most recent registries, but Docker may be needed for older registries without OCI support.</param>
	public async Task<string> Publish(string address, IReadOnlyList<Container>? platformVariants = default, ImageLayerCompression? forcedCompression = default, ImageMediaTypes? mediaTypes = default)
	{
		if (CachedPublish != null)
			return CachedPublish;
		return (await ComputeQuery(QueryTree.Add("publish", new OperationArgument("address", address, ParameterSerialization.String, false), new OperationArgument("platformVariants", platformVariants, ParameterSerialization.Reference, true), new OperationArgument("forcedCompression", forcedCompression, ParameterSerialization.Enum, false), new OperationArgument("mediaTypes", mediaTypes, ParameterSerialization.Enum, false)), await Context.Connection())).Deserialize<string>();
	}

	///<summary>Retrieves this container's root filesystem. Mounts are not included.</summary>
	public Directory Rootfs()
	{
		return new Directory
		{
			QueryTree = QueryTree.Add("rootfs"),
			Context = Context
		};
	}

	///<summary><para>Return a websocket endpoint that, if connected to, will start the container with a TTY streamed over the websocket.</para><para>Primarily intended for internal use with the dagger CLI.</para></summary>
	public async Task<string> ShellEndpoint()
	{
		if (CachedShellEndpoint != null)
			return CachedShellEndpoint;
		return (await ComputeQuery(QueryTree.Add("shellEndpoint"), await Context.Connection())).Deserialize<string>();
	}

	///<summary><para>The error stream of the last executed command.</para><para>Will execute default command if none is set, or error if there's no default.</para></summary>
	public async Task<string> Stderr()
	{
		if (CachedStderr != null)
			return CachedStderr;
		return (await ComputeQuery(QueryTree.Add("stderr"), await Context.Connection())).Deserialize<string>();
	}

	///<summary><para>The output stream of the last executed command.</para><para>Will execute default command if none is set, or error if there's no default.</para></summary>
	public async Task<string> Stdout()
	{
		if (CachedStdout != null)
			return CachedStdout;
		return (await ComputeQuery(QueryTree.Add("stdout"), await Context.Connection())).Deserialize<string>();
	}

	///<summary><para>Forces evaluation of the pipeline in the engine.</para><para>It doesn't run the default command if no exec has been set.</para></summary>
	public async Task<Container> Sync()
	{
		await ComputeQuery(QueryTree.Add("sync"), await Context.Connection());
		return this;
	}

	///<summary>Retrieves the user to be set for all commands.</summary>
	public async Task<string?> User()
	{
		if (CachedUser != null)
			return CachedUser;
		return (await ComputeQuery(QueryTree.Add("user"), await Context.Connection())).Deserialize<string?>();
	}

	///<summary>Configures default arguments for future commands.</summary>
	///<param name = "Args">Arguments to prepend to future executions (e.g., ["-v", "--no-cache"]).</param>
	public Container WithDefaultArgs(IReadOnlyList<string>? args = default)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withDefaultArgs", new OperationArgument("args", args, ParameterSerialization.String, true)),
			Context = Context
		};
	}

	///<summary>Retrieves this container plus a directory written at the given path.</summary>
	///<param name = "Path">Location of the written directory (e.g., "/tmp/directory").</param>
	///<param name = "Directory">Identifier of the directory to write</param>
	///<param name = "Exclude">Patterns to exclude in the written directory (e.g., ["node_modules/**", ".gitignore", ".git/"]).</param>
	///<param name = "Include">Patterns to include in the written directory (e.g., ["*.go", "go.mod", "go.sum"]).</param>
	///<param name = "Owner"><para>A user:group to set for the directory and its contents.</para><para>The user and group can either be an ID (1000:1000) or a name (foo:bar).</para><para>If the group is omitted, it defaults to the same as the user.</para></param>
	public Container WithDirectory(string path, Directory directory, IReadOnlyList<string>? exclude = default, IReadOnlyList<string>? include = default, string? owner = default)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withDirectory", new OperationArgument("path", path, ParameterSerialization.String, false), new OperationArgument("directory", directory, ParameterSerialization.Reference, false), new OperationArgument("exclude", exclude, ParameterSerialization.String, true), new OperationArgument("include", include, ParameterSerialization.String, true), new OperationArgument("owner", owner, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this container but with a different command entrypoint.</summary>
	///<param name = "Args">Entrypoint to use for future executions (e.g., ["go", "run"]).</param>
	///<param name = "KeepDefaultArgs">Don't remove the default arguments when setting the entrypoint.</param>
	public Container WithEntrypoint(IReadOnlyList<string> args, bool? keepDefaultArgs = default)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withEntrypoint", new OperationArgument("args", args, ParameterSerialization.String, false), new OperationArgument("keepDefaultArgs", keepDefaultArgs, ParameterSerialization.Enum, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this container plus the given environment variable.</summary>
	///<param name = "Name">The name of the environment variable (e.g., "HOST").</param>
	///<param name = "Value">The value of the environment variable. (e.g., "localhost").</param>
	///<param name = "Expand">Replace `${VAR}` or $VAR in the value according to the current environment variables defined in the container (e.g., "/opt/bin:$PATH").</param>
	public Container WithEnvVariable(string name, string value, bool? expand = default)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withEnvVariable", new OperationArgument("name", name, ParameterSerialization.String, false), new OperationArgument("value", value, ParameterSerialization.String, false), new OperationArgument("expand", expand, ParameterSerialization.Enum, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this container after executing the specified command inside it.</summary>
	///<param name = "Args"><para>Command to run instead of the container's default command (e.g., ["run", "main.go"]).</para><para>If empty, the container's default command is used.</para></param>
	///<param name = "SkipEntrypoint">If the container has an entrypoint, ignore it for args rather than using it to wrap them.</param>
	///<param name = "Stdin">Content to write to the command's standard input before closing (e.g., "Hello world").</param>
	///<param name = "RedirectStdout">Redirect the command's standard output to a file in the container (e.g., "/tmp/stdout").</param>
	///<param name = "RedirectStderr">Redirect the command's standard error to a file in the container (e.g., "/tmp/stderr").</param>
	///<param name = "ExperimentalPrivilegedNesting"><para>Provides dagger access to the executed command.</para><para>Do not use this option unless you trust the command being executed. The command being executed WILL BE GRANTED FULL ACCESS TO YOUR HOST FILESYSTEM.</para></param>
	///<param name = "InsecureRootCapabilities">Execute the command with all root capabilities. This is similar to running a command with "sudo" or executing `docker run` with the `--privileged` flag. Containerization does not provide any security guarantees when using this option. It should only be used when absolutely necessary and only with trusted commands.</param>
	public Container WithExec(IReadOnlyList<string> args, bool? skipEntrypoint = default, string? stdin = default, string? redirectStdout = default, string? redirectStderr = default, bool? experimentalPrivilegedNesting = default, bool? insecureRootCapabilities = default)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withExec", new OperationArgument("args", args, ParameterSerialization.String, false), new OperationArgument("skipEntrypoint", skipEntrypoint, ParameterSerialization.Enum, false), new OperationArgument("stdin", stdin, ParameterSerialization.String, false), new OperationArgument("redirectStdout", redirectStdout, ParameterSerialization.String, false), new OperationArgument("redirectStderr", redirectStderr, ParameterSerialization.String, false), new OperationArgument("experimentalPrivilegedNesting", experimentalPrivilegedNesting, ParameterSerialization.Enum, false), new OperationArgument("insecureRootCapabilities", insecureRootCapabilities, ParameterSerialization.Enum, false)),
			Context = Context
		};
	}

	///<summary><para>Expose a network port.</para><para>Exposed ports serve two purposes:</para><para>- For health checks and introspection, when running services - For setting the EXPOSE OCI field when publishing the container</para></summary>
	///<param name = "Port">Port number to expose</param>
	///<param name = "Protocol">Transport layer network protocol</param>
	///<param name = "Description">Optional port description</param>
	public Container WithExposedPort(int port, NetworkProtocol? protocol = default, string? description = default)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withExposedPort", new OperationArgument("port", port, ParameterSerialization.Enum, false), new OperationArgument("protocol", protocol, ParameterSerialization.Enum, false), new OperationArgument("description", description, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this container plus the contents of the given file copied to the given path.</summary>
	///<param name = "Path">Location of the copied file (e.g., "/tmp/file.txt").</param>
	///<param name = "Source">Identifier of the file to copy.</param>
	///<param name = "Permissions"><para>Permission given to the copied file (e.g., 0600).</para><para>Default: 0644.</para></param>
	///<param name = "Owner"><para>A user:group to set for the file.</para><para>The user and group can either be an ID (1000:1000) or a name (foo:bar).</para><para>If the group is omitted, it defaults to the same as the user.</para></param>
	public Container WithFile(string path, File source, int? permissions = default, string? owner = default)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withFile", new OperationArgument("path", path, ParameterSerialization.String, false), new OperationArgument("source", source, ParameterSerialization.Reference, false), new OperationArgument("permissions", permissions, ParameterSerialization.Enum, false), new OperationArgument("owner", owner, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Indicate that subsequent operations should be featured more prominently in the UI.</summary>
	public Container WithFocus()
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withFocus"),
			Context = Context
		};
	}

	///<summary>Retrieves this container plus the given label.</summary>
	///<param name = "Name">The name of the label (e.g., "org.opencontainers.artifact.created").</param>
	///<param name = "Value">The value of the label (e.g., "2023-01-01T00:00:00Z").</param>
	public Container WithLabel(string name, string value)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withLabel", new OperationArgument("name", name, ParameterSerialization.String, false), new OperationArgument("value", value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this container plus a cache volume mounted at the given path.</summary>
	///<param name = "Path">Location of the cache directory (e.g., "/cache/node_modules").</param>
	///<param name = "Cache">Identifier of the cache volume to mount.</param>
	///<param name = "Source">Identifier of the directory to use as the cache volume's root.</param>
	///<param name = "Sharing">Sharing mode of the cache volume.</param>
	///<param name = "Owner"><para>A user:group to set for the mounted cache directory.</para><para>Note that this changes the ownership of the specified mount along with the initial filesystem provided by source (if any). It does not have any effect if/when the cache has already been created.</para><para>The user and group can either be an ID (1000:1000) or a name (foo:bar).</para><para>If the group is omitted, it defaults to the same as the user.</para></param>
	public Container WithMountedCache(string path, CacheVolume cache, Directory? source = default, CacheSharingMode? sharing = default, string? owner = default)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withMountedCache", new OperationArgument("path", path, ParameterSerialization.String, false), new OperationArgument("cache", cache, ParameterSerialization.Reference, false), new OperationArgument("source", source, ParameterSerialization.Reference, false), new OperationArgument("sharing", sharing, ParameterSerialization.Enum, false), new OperationArgument("owner", owner, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this container plus a directory mounted at the given path.</summary>
	///<param name = "Path">Location of the mounted directory (e.g., "/mnt/directory").</param>
	///<param name = "Source">Identifier of the mounted directory.</param>
	///<param name = "Owner"><para>A user:group to set for the mounted directory and its contents.</para><para>The user and group can either be an ID (1000:1000) or a name (foo:bar).</para><para>If the group is omitted, it defaults to the same as the user.</para></param>
	public Container WithMountedDirectory(string path, Directory source, string? owner = default)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withMountedDirectory", new OperationArgument("path", path, ParameterSerialization.String, false), new OperationArgument("source", source, ParameterSerialization.Reference, false), new OperationArgument("owner", owner, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this container plus a file mounted at the given path.</summary>
	///<param name = "Path">Location of the mounted file (e.g., "/tmp/file.txt").</param>
	///<param name = "Source">Identifier of the mounted file.</param>
	///<param name = "Owner"><para>A user or user:group to set for the mounted file.</para><para>The user and group can either be an ID (1000:1000) or a name (foo:bar).</para><para>If the group is omitted, it defaults to the same as the user.</para></param>
	public Container WithMountedFile(string path, File source, string? owner = default)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withMountedFile", new OperationArgument("path", path, ParameterSerialization.String, false), new OperationArgument("source", source, ParameterSerialization.Reference, false), new OperationArgument("owner", owner, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this container plus a secret mounted into a file at the given path.</summary>
	///<param name = "Path">Location of the secret file (e.g., "/tmp/secret.txt").</param>
	///<param name = "Source">Identifier of the secret to mount.</param>
	///<param name = "Owner"><para>A user:group to set for the mounted secret.</para><para>The user and group can either be an ID (1000:1000) or a name (foo:bar).</para><para>If the group is omitted, it defaults to the same as the user.</para></param>
	///<param name = "Mode"><para>Permission given to the mounted secret (e.g., 0600). This option requires an owner to be set to be active.</para><para>Default: 0400.</para></param>
	public Container WithMountedSecret(string path, Secret source, string? owner = default, int? mode = default)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withMountedSecret", new OperationArgument("path", path, ParameterSerialization.String, false), new OperationArgument("source", source, ParameterSerialization.Reference, false), new OperationArgument("owner", owner, ParameterSerialization.String, false), new OperationArgument("mode", mode, ParameterSerialization.Enum, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this container plus a temporary directory mounted at the given path.</summary>
	///<param name = "Path">Location of the temporary directory (e.g., "/tmp/temp_dir").</param>
	public Container WithMountedTemp(string path)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withMountedTemp", new OperationArgument("path", path, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this container plus a new file written at the given path.</summary>
	///<param name = "Path">Location of the written file (e.g., "/tmp/file.txt").</param>
	///<param name = "Contents">Content of the file to write (e.g., "Hello world!").</param>
	///<param name = "Permissions"><para>Permission given to the written file (e.g., 0600).</para><para>Default: 0644.</para></param>
	///<param name = "Owner"><para>A user:group to set for the file.</para><para>The user and group can either be an ID (1000:1000) or a name (foo:bar).</para><para>If the group is omitted, it defaults to the same as the user.</para></param>
	public Container WithNewFile(string path, string? contents = default, int? permissions = default, string? owner = default)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withNewFile", new OperationArgument("path", path, ParameterSerialization.String, false), new OperationArgument("contents", contents, ParameterSerialization.String, false), new OperationArgument("permissions", permissions, ParameterSerialization.Enum, false), new OperationArgument("owner", owner, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this container with a registry authentication for a given address.</summary>
	///<param name = "Address">Registry's address to bind the authentication to. Formatted as [host]/[user]/[repo]:[tag] (e.g. docker.io/dagger/dagger:main).</param>
	///<param name = "Username">The username of the registry's account (e.g., "Dagger").</param>
	///<param name = "Secret">The API key, password or token to authenticate to this registry.</param>
	public Container WithRegistryAuth(string address, string username, Secret secret)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withRegistryAuth", new OperationArgument("address", address, ParameterSerialization.String, false), new OperationArgument("username", username, ParameterSerialization.String, false), new OperationArgument("secret", secret, ParameterSerialization.Reference, false)),
			Context = Context
		};
	}

	///<summary>Initializes this container from this DirectoryID.</summary>
	///<param name = "Directory"></param>
	public Container WithRootfs(Directory directory)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withRootfs", new OperationArgument("directory", directory, ParameterSerialization.Reference, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this container plus an env variable containing the given secret.</summary>
	///<param name = "Name">The name of the secret variable (e.g., "API_SECRET").</param>
	///<param name = "Secret">The identifier of the secret value.</param>
	public Container WithSecretVariable(string name, Secret secret)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withSecretVariable", new OperationArgument("name", name, ParameterSerialization.String, false), new OperationArgument("secret", secret, ParameterSerialization.Reference, false)),
			Context = Context
		};
	}

	///<summary><para>Establish a runtime dependency on a service.</para><para>The service will be started automatically when needed and detached when it is no longer needed, executing the default command if none is set.</para><para>The service will be reachable from the container via the provided hostname alias.</para><para>The service dependency will also convey to any files or directories produced by the container.</para></summary>
	///<param name = "Alias">A name that can be used to reach the service from the container</param>
	///<param name = "Service">Identifier of the service container</param>
	public Container WithServiceBinding(string @alias, Service service)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withServiceBinding", new OperationArgument("alias", @alias, ParameterSerialization.String, false), new OperationArgument("service", service, ParameterSerialization.Reference, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this container plus a socket forwarded to the given Unix socket path.</summary>
	///<param name = "Path">Location of the forwarded Unix socket (e.g., "/tmp/socket").</param>
	///<param name = "Source">Identifier of the socket to forward.</param>
	///<param name = "Owner"><para>A user:group to set for the mounted socket.</para><para>The user and group can either be an ID (1000:1000) or a name (foo:bar).</para><para>If the group is omitted, it defaults to the same as the user.</para></param>
	public Container WithUnixSocket(string path, Socket source, string? owner = default)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withUnixSocket", new OperationArgument("path", path, ParameterSerialization.String, false), new OperationArgument("source", source, ParameterSerialization.Reference, false), new OperationArgument("owner", owner, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this container with a different command user.</summary>
	///<param name = "Name">The user to set (e.g., "root").</param>
	public Container WithUser(string name)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withUser", new OperationArgument("name", name, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this container with a different working directory.</summary>
	///<param name = "Path">The path to set as the working directory (e.g., "/app").</param>
	public Container WithWorkdir(string path)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withWorkdir", new OperationArgument("path", path, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this container with unset default arguments for future commands.</summary>
	public Container WithoutDefaultArgs()
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withoutDefaultArgs"),
			Context = Context
		};
	}

	///<summary>Retrieves this container with an unset command entrypoint.</summary>
	///<param name = "KeepDefaultArgs">Don't remove the default arguments when unsetting the entrypoint.</param>
	public Container WithoutEntrypoint(bool? keepDefaultArgs = default)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withoutEntrypoint", new OperationArgument("keepDefaultArgs", keepDefaultArgs, ParameterSerialization.Enum, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this container minus the given environment variable.</summary>
	///<param name = "Name">The name of the environment variable (e.g., "HOST").</param>
	public Container WithoutEnvVariable(string name)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withoutEnvVariable", new OperationArgument("name", name, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Unexpose a previously exposed port.</summary>
	///<param name = "Port">Port number to unexpose</param>
	///<param name = "Protocol">Port protocol to unexpose</param>
	public Container WithoutExposedPort(int port, NetworkProtocol? protocol = default)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withoutExposedPort", new OperationArgument("port", port, ParameterSerialization.Enum, false), new OperationArgument("protocol", protocol, ParameterSerialization.Enum, false)),
			Context = Context
		};
	}

	///<summary><para>Indicate that subsequent operations should not be featured more prominently in the UI.</para><para>This is the initial state of all containers.</para></summary>
	public Container WithoutFocus()
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withoutFocus"),
			Context = Context
		};
	}

	///<summary>Retrieves this container minus the given environment label.</summary>
	///<param name = "Name">The name of the label to remove (e.g., "org.opencontainers.artifact.created").</param>
	public Container WithoutLabel(string name)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withoutLabel", new OperationArgument("name", name, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this container after unmounting everything at the given path.</summary>
	///<param name = "Path">Location of the cache directory (e.g., "/cache/node_modules").</param>
	public Container WithoutMount(string path)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withoutMount", new OperationArgument("path", path, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this container without the registry authentication of a given address.</summary>
	///<param name = "Address">Registry's address to remove the authentication from. Formatted as [host]/[user]/[repo]:[tag] (e.g. docker.io/dagger/dagger:main).</param>
	public Container WithoutRegistryAuth(string address)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withoutRegistryAuth", new OperationArgument("address", address, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this container with a previously added Unix socket removed.</summary>
	///<param name = "Path">Location of the socket to remove (e.g., "/tmp/socket").</param>
	public Container WithoutUnixSocket(string path)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withoutUnixSocket", new OperationArgument("path", path, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary><para>Retrieves this container with an unset command user.</para><para>Should default to root.</para></summary>
	public Container WithoutUser()
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withoutUser"),
			Context = Context
		};
	}

	///<summary><para>Retrieves this container with an unset working directory.</para><para>Should default to "/".</para></summary>
	public Container WithoutWorkdir()
	{
		return new Container
		{
			QueryTree = QueryTree.Add("withoutWorkdir"),
			Context = Context
		};
	}

	///<summary>Retrieves the working directory for all commands.</summary>
	public async Task<string?> Workdir()
	{
		if (CachedWorkdir != null)
			return CachedWorkdir;
		return (await ComputeQuery(QueryTree.Add("workdir"), await Context.Connection())).Deserialize<string?>();
	}
}

///<summary>A directory.</summary>
public sealed class Directory : BaseClient
{
	internal DirectoryID? CachedId { private get; init; }
	internal bool? CachedExport { private get; init; }
	internal DirectoryID? CachedSync { private get; init; }

	///<summary>The content-addressed identifier of the directory.</summary>
	public async Task<DirectoryID> Id()
	{
		if (CachedId != null)
			return CachedId;
		return new((await ComputeQuery(QueryTree.Add("id"), await Context.Connection())).Deserialize<string>());
	}

	///<summary>Load the directory as a Dagger module</summary>
	///<param name = "SourceSubpath"><para>An optional subpath of the directory which contains the module's source code.</para><para>This is needed when the module code is in a subdirectory but requires parent directories to be loaded in order to execute. For example, the module source code may need a go.mod, project.toml, package.json, etc. file from a parent directory.</para><para>If not set, the module source code is loaded from the root of the directory.</para></param>
	public Module AsModule(string? sourceSubpath = default)
	{
		return new Module
		{
			QueryTree = QueryTree.Add("asModule", new OperationArgument("sourceSubpath", sourceSubpath, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Gets the difference between this directory and an another directory.</summary>
	///<param name = "Other">Identifier of the directory to compare.</param>
	public Directory Diff(Directory other)
	{
		return new Directory
		{
			QueryTree = QueryTree.Add("diff", new OperationArgument("other", other, ParameterSerialization.Reference, false)),
			Context = Context
		};
	}

	///<summary>Retrieves a directory at the given path.</summary>
	///<param name = "Path">Location of the directory to retrieve (e.g., "/src").</param>
	public Directory SubDirectory(string path)
	{
		return new Directory
		{
			QueryTree = QueryTree.Add("directory", new OperationArgument("path", path, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Builds a new Docker container from this directory.</summary>
	///<param name = "Dockerfile"><para>Path to the Dockerfile to use (e.g., "frontend.Dockerfile").</para><para>Defaults: './Dockerfile'.</para></param>
	///<param name = "Platform">The platform to build.</param>
	///<param name = "BuildArgs">Build arguments to use in the build.</param>
	///<param name = "Target">Target build stage to build.</param>
	///<param name = "Secrets"><para>Secrets to pass to the build.</para><para>They will be mounted at /run/secrets/[secret-name].</para></param>
	public Container DockerBuild(string? dockerfile = default, Platform? platform = default, IReadOnlyList<BuildArg>? buildArgs = default, string? target = default, IReadOnlyList<Secret>? secrets = default)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("dockerBuild", new OperationArgument("dockerfile", dockerfile, ParameterSerialization.String, false), new OperationArgument("platform", platform?.Value, ParameterSerialization.String, false), new OperationArgument("buildArgs", buildArgs, ParameterSerialization.Object, true), new OperationArgument("target", target, ParameterSerialization.String, false), new OperationArgument("secrets", secrets, ParameterSerialization.Reference, true)),
			Context = Context
		};
	}

	///<summary>Returns a list of files and directories at the given path.</summary>
	///<param name = "Path">Location of the directory to look at (e.g., "/src").</param>
	public async Task<ImmutableArray<string>> Entries(string? path = default)
	{
		return (await ComputeQuery(QueryTree.Add("entries", new OperationArgument("path", path, ParameterSerialization.String, false)), await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary>Writes the contents of the directory to a path on the host.</summary>
	///<param name = "Path">Location of the copied directory (e.g., "logs/").</param>
	public async Task<bool> Export(string path)
	{
		if (CachedExport != null)
			return CachedExport.Value;
		return (await ComputeQuery(QueryTree.Add("export", new OperationArgument("path", path, ParameterSerialization.String, false)), await Context.Connection())).Deserialize<bool>();
	}

	///<summary>Retrieves a file at the given path.</summary>
	///<param name = "Path">Location of the file to retrieve (e.g., "README.md").</param>
	public File File(string path)
	{
		return new File
		{
			QueryTree = QueryTree.Add("file", new OperationArgument("path", path, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Returns a list of files and directories that matche the given pattern.</summary>
	///<param name = "Pattern">Pattern to match (e.g., "*.md").</param>
	public async Task<ImmutableArray<string>> Glob(string pattern)
	{
		return (await ComputeQuery(QueryTree.Add("glob", new OperationArgument("pattern", pattern, ParameterSerialization.String, false)), await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary>Creates a named sub-pipeline</summary>
	///<param name = "Name">Pipeline name.</param>
	///<param name = "Description">Pipeline description.</param>
	///<param name = "Labels">Pipeline labels.</param>
	public Directory Pipeline(string name, string? description = default, IReadOnlyList<PipelineLabel>? labels = default)
	{
		return new Directory
		{
			QueryTree = QueryTree.Add("pipeline", new OperationArgument("name", name, ParameterSerialization.String, false), new OperationArgument("description", description, ParameterSerialization.String, false), new OperationArgument("labels", labels, ParameterSerialization.Object, true)),
			Context = Context
		};
	}

	///<summary>Force evaluation in the engine.</summary>
	public async Task<Directory> Sync()
	{
		await ComputeQuery(QueryTree.Add("sync"), await Context.Connection());
		return this;
	}

	///<summary>Retrieves this directory plus a directory written at the given path.</summary>
	///<param name = "Path">Location of the written directory (e.g., "/src/").</param>
	///<param name = "Directory">Identifier of the directory to copy.</param>
	///<param name = "Exclude">Exclude artifacts that match the given pattern (e.g., ["node_modules/", ".git*"]).</param>
	///<param name = "Include">Include only artifacts that match the given pattern (e.g., ["app/", "package.*"]).</param>
	public Directory WithDirectory(string path, Directory directory, IReadOnlyList<string>? exclude = default, IReadOnlyList<string>? include = default)
	{
		return new Directory
		{
			QueryTree = QueryTree.Add("withDirectory", new OperationArgument("path", path, ParameterSerialization.String, false), new OperationArgument("directory", directory, ParameterSerialization.Reference, false), new OperationArgument("exclude", exclude, ParameterSerialization.String, true), new OperationArgument("include", include, ParameterSerialization.String, true)),
			Context = Context
		};
	}

	///<summary>Retrieves this directory plus the contents of the given file copied to the given path.</summary>
	///<param name = "Path">Location of the copied file (e.g., "/file.txt").</param>
	///<param name = "Source">Identifier of the file to copy.</param>
	///<param name = "Permissions"><para>Permission given to the copied file (e.g., 0600).</para><para>Default: 0644.</para></param>
	public Directory WithFile(string path, File source, int? permissions = default)
	{
		return new Directory
		{
			QueryTree = QueryTree.Add("withFile", new OperationArgument("path", path, ParameterSerialization.String, false), new OperationArgument("source", source, ParameterSerialization.Reference, false), new OperationArgument("permissions", permissions, ParameterSerialization.Enum, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this directory plus a new directory created at the given path.</summary>
	///<param name = "Path">Location of the directory created (e.g., "/logs").</param>
	///<param name = "Permissions"><para>Permission granted to the created directory (e.g., 0777).</para><para>Default: 0755.</para></param>
	public Directory WithNewDirectory(string path, int? permissions = default)
	{
		return new Directory
		{
			QueryTree = QueryTree.Add("withNewDirectory", new OperationArgument("path", path, ParameterSerialization.String, false), new OperationArgument("permissions", permissions, ParameterSerialization.Enum, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this directory plus a new file written at the given path.</summary>
	///<param name = "Path">Location of the written file (e.g., "/file.txt").</param>
	///<param name = "Contents">Content of the written file (e.g., "Hello world!").</param>
	///<param name = "Permissions"><para>Permission given to the copied file (e.g., 0600).</para><para>Default: 0644.</para></param>
	public Directory WithNewFile(string path, string contents, int? permissions = default)
	{
		return new Directory
		{
			QueryTree = QueryTree.Add("withNewFile", new OperationArgument("path", path, ParameterSerialization.String, false), new OperationArgument("contents", contents, ParameterSerialization.String, false), new OperationArgument("permissions", permissions, ParameterSerialization.Enum, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this directory with all file/dir timestamps set to the given time.</summary>
	///<param name = "Timestamp"><para>Timestamp to set dir/files in.</para><para>Formatted in seconds following Unix epoch (e.g., 1672531199).</para></param>
	public Directory WithTimestamps(int timestamp)
	{
		return new Directory
		{
			QueryTree = QueryTree.Add("withTimestamps", new OperationArgument("timestamp", timestamp, ParameterSerialization.Enum, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this directory with the directory at the given path removed.</summary>
	///<param name = "Path">Location of the directory to remove (e.g., ".github/").</param>
	public Directory WithoutDirectory(string path)
	{
		return new Directory
		{
			QueryTree = QueryTree.Add("withoutDirectory", new OperationArgument("path", path, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Retrieves this directory with the file at the given path removed.</summary>
	///<param name = "Path">Location of the file to remove (e.g., "/file.txt").</param>
	public Directory WithoutFile(string path)
	{
		return new Directory
		{
			QueryTree = QueryTree.Add("withoutFile", new OperationArgument("path", path, ParameterSerialization.String, false)),
			Context = Context
		};
	}
}

///<summary>A simple key value object that represents an environment variable.</summary>
public sealed class EnvVariable : BaseClient
{
	internal string? CachedName { private get; init; }
	internal string? CachedValue { private get; init; }

	///<summary>The environment variable name.</summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		return (await ComputeQuery(QueryTree.Add("name"), await Context.Connection())).Deserialize<string>();
	}

	///<summary>The environment variable value.</summary>
	public async Task<string> Value()
	{
		if (CachedValue != null)
			return CachedValue;
		return (await ComputeQuery(QueryTree.Add("value"), await Context.Connection())).Deserialize<string>();
	}
}

///<summary>A definition of a field on a custom object defined in a Module. A field on an object has a static value, as opposed to a function on an object whose value is computed by invoking code (and can accept arguments).</summary>
public sealed class FieldTypeDef : BaseClient
{
	internal string? CachedDescription { private get; init; }
	internal string? CachedName { private get; init; }

	///<summary>A doc string for the field, if any</summary>
	public async Task<string?> Description()
	{
		if (CachedDescription != null)
			return CachedDescription;
		return (await ComputeQuery(QueryTree.Add("description"), await Context.Connection())).Deserialize<string?>();
	}

	///<summary>The name of the field in the object</summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		return (await ComputeQuery(QueryTree.Add("name"), await Context.Connection())).Deserialize<string>();
	}

	///<summary>The type of the field</summary>
	public TypeDef GetTypeDef()
	{
		return new TypeDef
		{
			QueryTree = QueryTree.Add("typeDef"),
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
	internal int? CachedSize { private get; init; }
	internal FileID? CachedSync { private get; init; }

	///<summary>Retrieves the content-addressed identifier of the file.</summary>
	public async Task<FileID> Id()
	{
		if (CachedId != null)
			return CachedId;
		return new((await ComputeQuery(QueryTree.Add("id"), await Context.Connection())).Deserialize<string>());
	}

	///<summary>Retrieves the contents of the file.</summary>
	public async Task<string> Contents()
	{
		if (CachedContents != null)
			return CachedContents;
		return (await ComputeQuery(QueryTree.Add("contents"), await Context.Connection())).Deserialize<string>();
	}

	///<summary>Writes the file to a file path on the host.</summary>
	///<param name = "Path">Location of the written directory (e.g., "output.txt").</param>
	///<param name = "AllowParentDirPath">If allowParentDirPath is true, the path argument can be a directory path, in which case the file will be created in that directory.</param>
	public async Task<bool> Export(string path, bool? allowParentDirPath = default)
	{
		if (CachedExport != null)
			return CachedExport.Value;
		return (await ComputeQuery(QueryTree.Add("export", new OperationArgument("path", path, ParameterSerialization.String, false), new OperationArgument("allowParentDirPath", allowParentDirPath, ParameterSerialization.Enum, false)), await Context.Connection())).Deserialize<bool>();
	}

	///<summary>Gets the size of the file, in bytes.</summary>
	public async Task<int> Size()
	{
		if (CachedSize != null)
			return CachedSize.Value;
		return (await ComputeQuery(QueryTree.Add("size"), await Context.Connection())).Deserialize<int>();
	}

	///<summary>Force evaluation in the engine.</summary>
	public async Task<File> Sync()
	{
		await ComputeQuery(QueryTree.Add("sync"), await Context.Connection());
		return this;
	}

	///<summary>Retrieves this file with its created/modified timestamps set to the given time.</summary>
	///<param name = "Timestamp"><para>Timestamp to set dir/files in.</para><para>Formatted in seconds following Unix epoch (e.g., 1672531199).</para></param>
	public File WithTimestamps(int timestamp)
	{
		return new File
		{
			QueryTree = QueryTree.Add("withTimestamps", new OperationArgument("timestamp", timestamp, ParameterSerialization.Enum, false)),
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

	///<summary>The ID of the function</summary>
	public async Task<FunctionID> Id()
	{
		if (CachedId != null)
			return CachedId;
		return new((await ComputeQuery(QueryTree.Add("id"), await Context.Connection())).Deserialize<string>());
	}

	///<summary>Arguments accepted by this function, if any</summary>
	public async Task<ImmutableArray<FunctionArg>> Args()
	{
		return (await ComputeQuery(QueryTree.Add("args").Add("id"), await Context.Connection())).EnumerateArray().Select(json => new FunctionArg { QueryTree = QueryTree, Context = Context, CachedId = json.GetProperty("id").Deserialize<FunctionArgID>() }).ToImmutableArray();
	}

	///<summary>A doc string for the function, if any</summary>
	public async Task<string?> Description()
	{
		if (CachedDescription != null)
			return CachedDescription;
		return (await ComputeQuery(QueryTree.Add("description"), await Context.Connection())).Deserialize<string?>();
	}

	///<summary>The name of the function</summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		return (await ComputeQuery(QueryTree.Add("name"), await Context.Connection())).Deserialize<string>();
	}

	///<summary>The type returned by this function</summary>
	public TypeDef ReturnType()
	{
		return new TypeDef
		{
			QueryTree = QueryTree.Add("returnType"),
			Context = Context
		};
	}

	///<summary>Returns the function with the provided argument</summary>
	///<param name = "Name">The name of the argument</param>
	///<param name = "TypeDef">The type of the argument</param>
	///<param name = "Description">A doc string for the argument, if any</param>
	///<param name = "DefaultValue">A default value to use for this argument if not explicitly set by the caller, if any</param>
	public Function WithArg(string name, TypeDef typeDef, string? description = default, JSON? defaultValue = default)
	{
		return new Function
		{
			QueryTree = QueryTree.Add("withArg", new OperationArgument("name", name, ParameterSerialization.String, false), new OperationArgument("typeDef", typeDef, ParameterSerialization.Reference, false), new OperationArgument("description", description, ParameterSerialization.String, false), new OperationArgument("defaultValue", defaultValue?.Value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Returns the function with the doc string</summary>
	///<param name = "Description"></param>
	public Function WithDescription(string description)
	{
		return new Function
		{
			QueryTree = QueryTree.Add("withDescription", new OperationArgument("description", description, ParameterSerialization.String, false)),
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

	///<summary>The ID of the argument</summary>
	public async Task<FunctionArgID> Id()
	{
		if (CachedId != null)
			return CachedId;
		return new((await ComputeQuery(QueryTree.Add("id"), await Context.Connection())).Deserialize<string>());
	}

	///<summary>A default value to use for this argument when not explicitly set by the caller, if any</summary>
	public async Task<JSON?> DefaultValue()
	{
		if (CachedDefaultValue != null)
			return CachedDefaultValue;
		return new((await ComputeQuery(QueryTree.Add("defaultValue"), await Context.Connection())).Deserialize<string>());
	}

	///<summary>A doc string for the argument, if any</summary>
	public async Task<string?> Description()
	{
		if (CachedDescription != null)
			return CachedDescription;
		return (await ComputeQuery(QueryTree.Add("description"), await Context.Connection())).Deserialize<string?>();
	}

	///<summary>The name of the argument</summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		return (await ComputeQuery(QueryTree.Add("name"), await Context.Connection())).Deserialize<string>();
	}

	///<summary>The type of the argument</summary>
	public TypeDef GetTypeDef()
	{
		return new TypeDef
		{
			QueryTree = QueryTree.Add("typeDef"),
			Context = Context
		};
	}
}

///<summary></summary>
public sealed class FunctionCall : BaseClient
{
	internal string? CachedName { private get; init; }
	internal JSON? CachedParent { private get; init; }
	internal string? CachedParentName { private get; init; }
	internal Void? CachedReturnValue { private get; init; }

	///<summary>The argument values the function is being invoked with.</summary>
	public async Task<ImmutableArray<FunctionCallArgValue>> InputArgs()
	{
		return (await ComputeQuery(QueryTree.Add("inputArgs").Add("name value"), await Context.Connection())).EnumerateArray().Select(json => new FunctionCallArgValue { QueryTree = QueryTree, Context = Context, CachedName = json.GetProperty("name").Deserialize<string>(), CachedValue = json.GetProperty("value").Deserialize<JSON>() }).ToImmutableArray();
	}

	///<summary>The name of the function being called.</summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		return (await ComputeQuery(QueryTree.Add("name"), await Context.Connection())).Deserialize<string>();
	}

	///<summary>The value of the parent object of the function being called. If the function is "top-level" to the module, this is always an empty object.</summary>
	public async Task<JSON> Parent()
	{
		if (CachedParent != null)
			return CachedParent;
		return new((await ComputeQuery(QueryTree.Add("parent"), await Context.Connection())).Deserialize<string>());
	}

	///<summary>The name of the parent object of the function being called. If the function is "top-level" to the module, this is the name of the module.</summary>
	public async Task<string> ParentName()
	{
		if (CachedParentName != null)
			return CachedParentName;
		return (await ComputeQuery(QueryTree.Add("parentName"), await Context.Connection())).Deserialize<string>();
	}

	///<summary>Set the return value of the function call to the provided value. The value should be a string of the JSON serialization of the return value.</summary>
	///<param name = "Value"></param>
	public async Task<Void?> ReturnValue(JSON value)
	{
		if (CachedReturnValue != null)
			return CachedReturnValue;
		return new((await ComputeQuery(QueryTree.Add("returnValue", new OperationArgument("value", value.Value, ParameterSerialization.String, false)), await Context.Connection())).Deserialize<string>());
	}
}

///<summary></summary>
public sealed class FunctionCallArgValue : BaseClient
{
	internal string? CachedName { private get; init; }
	internal JSON? CachedValue { private get; init; }

	///<summary>The name of the argument.</summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		return (await ComputeQuery(QueryTree.Add("name"), await Context.Connection())).Deserialize<string>();
	}

	///<summary>The value of the argument represented as a string of the JSON serialization.</summary>
	public async Task<JSON> Value()
	{
		if (CachedValue != null)
			return CachedValue;
		return new((await ComputeQuery(QueryTree.Add("value"), await Context.Connection())).Deserialize<string>());
	}
}

///<summary></summary>
public sealed class GeneratedCode : BaseClient
{
	internal GeneratedCodeID? CachedId { private get; init; }

	///<summary></summary>
	public async Task<GeneratedCodeID> Id()
	{
		if (CachedId != null)
			return CachedId;
		return new((await ComputeQuery(QueryTree.Add("id"), await Context.Connection())).Deserialize<string>());
	}

	///<summary>The directory containing the generated code</summary>
	public Directory Code()
	{
		return new Directory
		{
			QueryTree = QueryTree.Add("code"),
			Context = Context
		};
	}

	///<summary>List of paths to mark generated in version control (i.e. .gitattributes)</summary>
	public async Task<ImmutableArray<string>> VcsGeneratedPaths()
	{
		return (await ComputeQuery(QueryTree.Add("vcsGeneratedPaths"), await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary>List of paths to ignore in version control (i.e. .gitignore)</summary>
	public async Task<ImmutableArray<string>> VcsIgnoredPaths()
	{
		return (await ComputeQuery(QueryTree.Add("vcsIgnoredPaths"), await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary>Set the list of paths to mark generated in version control</summary>
	///<param name = "Paths"></param>
	public GeneratedCode WithVCSGeneratedPaths(IReadOnlyList<string> paths)
	{
		return new GeneratedCode
		{
			QueryTree = QueryTree.Add("withVCSGeneratedPaths", new OperationArgument("paths", paths, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Set the list of paths to ignore in version control</summary>
	///<param name = "Paths"></param>
	public GeneratedCode WithVCSIgnoredPaths(IReadOnlyList<string> paths)
	{
		return new GeneratedCode
		{
			QueryTree = QueryTree.Add("withVCSIgnoredPaths", new OperationArgument("paths", paths, ParameterSerialization.String, false)),
			Context = Context
		};
	}
}

///<summary>A git ref (tag, branch or commit).</summary>
public sealed class GitRef : BaseClient
{
	internal GitRefID? CachedId { private get; init; }
	internal string? CachedCommit { private get; init; }

	///<summary>Retrieves the content-addressed identifier of the git ref.</summary>
	public async Task<GitRefID> Id()
	{
		if (CachedId != null)
			return CachedId;
		return new((await ComputeQuery(QueryTree.Add("id"), await Context.Connection())).Deserialize<string>());
	}

	///<summary>The resolved commit id at this ref.</summary>
	public async Task<string> Commit()
	{
		if (CachedCommit != null)
			return CachedCommit;
		return (await ComputeQuery(QueryTree.Add("commit"), await Context.Connection())).Deserialize<string>();
	}

	///<summary>The filesystem tree at this ref.</summary>
	///<param name = "SshKnownHosts"></param>
	///<param name = "SshAuthSocket"></param>
	public Directory Tree(string? sshKnownHosts = default, Socket? sshAuthSocket = default)
	{
		return new Directory
		{
			QueryTree = QueryTree.Add("tree", new OperationArgument("sshKnownHosts", sshKnownHosts, ParameterSerialization.String, false), new OperationArgument("sshAuthSocket", sshAuthSocket, ParameterSerialization.Reference, false)),
			Context = Context
		};
	}
}

///<summary>A git repository.</summary>
public sealed class GitRepository : BaseClient
{
	internal GitRepositoryID? CachedId { private get; init; }

	///<summary>Retrieves the content-addressed identifier of the git repository.</summary>
	public async Task<GitRepositoryID> Id()
	{
		if (CachedId != null)
			return CachedId;
		return new((await ComputeQuery(QueryTree.Add("id"), await Context.Connection())).Deserialize<string>());
	}

	///<summary>Returns details on one branch.</summary>
	///<param name = "Name">Branch's name (e.g., "main").</param>
	public GitRef Branch(string name)
	{
		return new GitRef
		{
			QueryTree = QueryTree.Add("branch", new OperationArgument("name", name, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Returns details on one commit.</summary>
	///<param name = "Id">Identifier of the commit (e.g., "b6315d8f2810962c601af73f86831f6866ea798b").</param>
	public GitRef Commit(string id)
	{
		return new GitRef
		{
			QueryTree = QueryTree.Add("commit", new OperationArgument("id", id, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Returns details on one tag.</summary>
	///<param name = "Name">Tag's name (e.g., "v0.3.9").</param>
	public GitRef Tag(string name)
	{
		return new GitRef
		{
			QueryTree = QueryTree.Add("tag", new OperationArgument("name", name, ParameterSerialization.String, false)),
			Context = Context
		};
	}
}

///<summary>Information about the host execution environment.</summary>
public sealed class Host : BaseClient
{
	///<summary>Accesses a directory on the host.</summary>
	///<param name = "Path">Location of the directory to access (e.g., ".").</param>
	///<param name = "Exclude">Exclude artifacts that match the given pattern (e.g., ["node_modules/", ".git*"]).</param>
	///<param name = "Include">Include only artifacts that match the given pattern (e.g., ["app/", "package.*"]).</param>
	public Directory Directory(string path, IReadOnlyList<string>? exclude = default, IReadOnlyList<string>? include = default)
	{
		return new Directory
		{
			QueryTree = QueryTree.Add("directory", new OperationArgument("path", path, ParameterSerialization.String, false), new OperationArgument("exclude", exclude, ParameterSerialization.String, true), new OperationArgument("include", include, ParameterSerialization.String, true)),
			Context = Context
		};
	}

	///<summary>Accesses a file on the host.</summary>
	///<param name = "Path">Location of the file to retrieve (e.g., "README.md").</param>
	public File File(string path)
	{
		return new File
		{
			QueryTree = QueryTree.Add("file", new OperationArgument("path", path, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Creates a service that forwards traffic to a specified address via the host.</summary>
	///<param name = "Ports"><para>Ports to expose via the service, forwarding through the host network.</para><para>If a port's frontend is unspecified or 0, it defaults to the same as the backend port.</para><para>An empty set of ports is not valid; an error will be returned.</para></param>
	///<param name = "Host">Upstream host to forward traffic to.</param>
	public Service Service(IReadOnlyList<PortForward> ports, string? host = default)
	{
		return new Service
		{
			QueryTree = QueryTree.Add("service", new OperationArgument("ports", ports, ParameterSerialization.Object, false), new OperationArgument("host", host, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Sets a secret given a user-defined name and the file path on the host, and returns the secret. The file is limited to a size of 512000 bytes.</summary>
	///<param name = "Name">The user defined name for this secret.</param>
	///<param name = "Path">Location of the file to set as a secret.</param>
	public Secret SetSecretFile(string name, string path)
	{
		return new Secret
		{
			QueryTree = QueryTree.Add("setSecretFile", new OperationArgument("name", name, ParameterSerialization.String, false), new OperationArgument("path", path, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Creates a tunnel that forwards traffic from the host to a service.</summary>
	///<param name = "Service">Service to send traffic from the tunnel.</param>
	///<param name = "Native"><para>Map each service port to the same port on the host, as if the service were running natively.</para><para>Note: enabling may result in port conflicts.</para></param>
	///<param name = "Ports"><para>Configure explicit port forwarding rules for the tunnel.</para><para>If a port's frontend is unspecified or 0, a random port will be chosen by the host.</para><para>If no ports are given, all of the service's ports are forwarded. If native is true, each port maps to the same port on the host. If native is false, each port maps to a random port chosen by the host.</para><para>If ports are given and native is true, the ports are additive.</para></param>
	public Service Tunnel(Service service, bool? native = default, IReadOnlyList<PortForward>? ports = default)
	{
		return new Service
		{
			QueryTree = QueryTree.Add("tunnel", new OperationArgument("service", service, ParameterSerialization.Reference, false), new OperationArgument("native", native, ParameterSerialization.Enum, false), new OperationArgument("ports", ports, ParameterSerialization.Object, true)),
			Context = Context
		};
	}

	///<summary>Accesses a Unix socket on the host.</summary>
	///<param name = "Path">Location of the Unix socket (e.g., "/var/run/docker.sock").</param>
	public Socket UnixSocket(string path)
	{
		return new Socket
		{
			QueryTree = QueryTree.Add("unixSocket", new OperationArgument("path", path, ParameterSerialization.String, false)),
			Context = Context
		};
	}
}

///<summary>A simple key value object that represents a label.</summary>
public sealed class Label : BaseClient
{
	internal string? CachedName { private get; init; }
	internal string? CachedValue { private get; init; }

	///<summary>The label name.</summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		return (await ComputeQuery(QueryTree.Add("name"), await Context.Connection())).Deserialize<string>();
	}

	///<summary>The label value.</summary>
	public async Task<string> Value()
	{
		if (CachedValue != null)
			return CachedValue;
		return (await ComputeQuery(QueryTree.Add("value"), await Context.Connection())).Deserialize<string>();
	}
}

///<summary>A definition of a list type in a Module.</summary>
public sealed class ListTypeDef : BaseClient
{
	///<summary>The type of the elements in the list</summary>
	public TypeDef ElementTypeDef()
	{
		return new TypeDef
		{
			QueryTree = QueryTree.Add("elementTypeDef"),
			Context = Context
		};
	}
}

///<summary></summary>
public sealed class Module : BaseClient
{
	internal ModuleID? CachedId { private get; init; }
	internal string? CachedDescription { private get; init; }
	internal string? CachedName { private get; init; }
	internal string? CachedSdk { private get; init; }
	internal Void? CachedServe { private get; init; }
	internal string? CachedSourceDirectorySubPath { private get; init; }

	///<summary>The ID of the module</summary>
	public async Task<ModuleID> Id()
	{
		if (CachedId != null)
			return CachedId;
		return new((await ComputeQuery(QueryTree.Add("id"), await Context.Connection())).Deserialize<string>());
	}

	///<summary>Modules used by this module</summary>
	public async Task<ImmutableArray<Module>> Dependencies()
	{
		return (await ComputeQuery(QueryTree.Add("dependencies").Add("id"), await Context.Connection())).EnumerateArray().Select(json => new Module { QueryTree = QueryTree, Context = Context, CachedId = json.GetProperty("id").Deserialize<ModuleID>() }).ToImmutableArray();
	}

	///<summary>The dependencies as configured by the module</summary>
	public async Task<ImmutableArray<string>> DependencyConfig()
	{
		return (await ComputeQuery(QueryTree.Add("dependencyConfig"), await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary>The doc string of the module, if any</summary>
	public async Task<string?> Description()
	{
		if (CachedDescription != null)
			return CachedDescription;
		return (await ComputeQuery(QueryTree.Add("description"), await Context.Connection())).Deserialize<string?>();
	}

	///<summary>The code generated by the SDK's runtime</summary>
	public GeneratedCode GetGeneratedCode()
	{
		return new GeneratedCode
		{
			QueryTree = QueryTree.Add("generatedCode"),
			Context = Context
		};
	}

	///<summary>The name of the module</summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		return (await ComputeQuery(QueryTree.Add("name"), await Context.Connection())).Deserialize<string>();
	}

	///<summary>Objects served by this module</summary>
	public async Task<ImmutableArray<TypeDef>> Objects()
	{
		return (await ComputeQuery(QueryTree.Add("objects").Add("id"), await Context.Connection())).EnumerateArray().Select(json => new TypeDef { QueryTree = QueryTree, Context = Context, CachedId = json.GetProperty("id").Deserialize<TypeDefID>() }).ToImmutableArray();
	}

	///<summary>The SDK used by this module. Either a name of a builtin SDK or a module ref pointing to the SDK's implementation.</summary>
	public async Task<string> Sdk()
	{
		if (CachedSdk != null)
			return CachedSdk;
		return (await ComputeQuery(QueryTree.Add("sdk"), await Context.Connection())).Deserialize<string>();
	}

	///<summary>Serve a module's API in the current session.     Note: this can only be called once per session.     In the future, it could return a stream or service to remove the side effect.</summary>
	public async Task<Void?> Serve()
	{
		if (CachedServe != null)
			return CachedServe;
		return new((await ComputeQuery(QueryTree.Add("serve"), await Context.Connection())).Deserialize<string>());
	}

	///<summary>The directory containing the module's source code</summary>
	public Directory SourceDirectory()
	{
		return new Directory
		{
			QueryTree = QueryTree.Add("sourceDirectory"),
			Context = Context
		};
	}

	///<summary>The module's subpath within the source directory</summary>
	public async Task<string> SourceDirectorySubPath()
	{
		if (CachedSourceDirectorySubPath != null)
			return CachedSourceDirectorySubPath;
		return (await ComputeQuery(QueryTree.Add("sourceDirectorySubPath"), await Context.Connection())).Deserialize<string>();
	}

	///<summary>This module plus the given Object type and associated functions</summary>
	///<param name = "Object"></param>
	public Module WithObject(TypeDef @object)
	{
		return new Module
		{
			QueryTree = QueryTree.Add("withObject", new OperationArgument("object", @object, ParameterSerialization.Reference, false)),
			Context = Context
		};
	}
}

///<summary>Static configuration for a module (e.g. parsed contents of dagger.json)</summary>
public sealed class ModuleConfig : BaseClient
{
	internal string? CachedName { private get; init; }
	internal string? CachedRoot { private get; init; }
	internal string? CachedSdk { private get; init; }

	///<summary>Modules that this module depends on.</summary>
	public async Task<ImmutableArray<string>> Dependencies()
	{
		return (await ComputeQuery(QueryTree.Add("dependencies"), await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary>Exclude these file globs when loading the module root.</summary>
	public async Task<ImmutableArray<string>> Exclude()
	{
		return (await ComputeQuery(QueryTree.Add("exclude"), await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary>Include only these file globs when loading the module root.</summary>
	public async Task<ImmutableArray<string>> Include()
	{
		return (await ComputeQuery(QueryTree.Add("include"), await Context.Connection())).Deserialize<ImmutableArray<string>>();
	}

	///<summary>The name of the module.</summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		return (await ComputeQuery(QueryTree.Add("name"), await Context.Connection())).Deserialize<string>();
	}

	///<summary>The root directory of the module's project, which may be above the module source code.</summary>
	public async Task<string?> Root()
	{
		if (CachedRoot != null)
			return CachedRoot;
		return (await ComputeQuery(QueryTree.Add("root"), await Context.Connection())).Deserialize<string?>();
	}

	///<summary>Either the name of a built-in SDK ('go', 'python', etc.) OR a module reference pointing to the SDK's module implementation.</summary>
	public async Task<string> Sdk()
	{
		if (CachedSdk != null)
			return CachedSdk;
		return (await ComputeQuery(QueryTree.Add("sdk"), await Context.Connection())).Deserialize<string>();
	}
}

///<summary>A definition of a custom object defined in a Module.</summary>
public sealed class ObjectTypeDef : BaseClient
{
	internal string? CachedDescription { private get; init; }
	internal string? CachedName { private get; init; }

	///<summary>The function used to construct new instances of this object, if any</summary>
	public Function Constructor()
	{
		return new Function
		{
			QueryTree = QueryTree.Add("constructor"),
			Context = Context
		};
	}

	///<summary>The doc string for the object, if any</summary>
	public async Task<string?> Description()
	{
		if (CachedDescription != null)
			return CachedDescription;
		return (await ComputeQuery(QueryTree.Add("description"), await Context.Connection())).Deserialize<string?>();
	}

	///<summary>Static fields defined on this object, if any</summary>
	public async Task<ImmutableArray<FieldTypeDef>> Fields()
	{
		return (await ComputeQuery(QueryTree.Add("fields").Add("description name"), await Context.Connection())).EnumerateArray().Select(json => new FieldTypeDef { QueryTree = QueryTree, Context = Context, CachedDescription = json.GetProperty("description").Deserialize<string?>(), CachedName = json.GetProperty("name").Deserialize<string>() }).ToImmutableArray();
	}

	///<summary>Functions defined on this object, if any</summary>
	public async Task<ImmutableArray<Function>> Functions()
	{
		return (await ComputeQuery(QueryTree.Add("functions").Add("id"), await Context.Connection())).EnumerateArray().Select(json => new Function { QueryTree = QueryTree, Context = Context, CachedId = json.GetProperty("id").Deserialize<FunctionID>() }).ToImmutableArray();
	}

	///<summary>The name of the object</summary>
	public async Task<string> Name()
	{
		if (CachedName != null)
			return CachedName;
		return (await ComputeQuery(QueryTree.Add("name"), await Context.Connection())).Deserialize<string>();
	}
}

///<summary>A port exposed by a container.</summary>
public sealed class Port : BaseClient
{
	internal string? CachedDescription { private get; init; }
	internal int? CachedPort { private get; init; }
	internal NetworkProtocol? CachedProtocol { private get; init; }

	///<summary>The port description.</summary>
	public async Task<string?> Description()
	{
		if (CachedDescription != null)
			return CachedDescription;
		return (await ComputeQuery(QueryTree.Add("description"), await Context.Connection())).Deserialize<string?>();
	}

	///<summary>The port number.</summary>
	public async Task<int> SubPort()
	{
		if (CachedPort != null)
			return CachedPort.Value;
		return (await ComputeQuery(QueryTree.Add("port"), await Context.Connection())).Deserialize<int>();
	}

	///<summary>The transport layer network protocol.</summary>
	public async Task<NetworkProtocol> Protocol()
	{
		if (CachedProtocol != null)
			return CachedProtocol.Value;
		return (await ComputeQuery(QueryTree.Add("protocol"), await Context.Connection())).Deserialize<NetworkProtocol>();
	}
}

///<summary></summary>
public sealed class Client : BaseClient
{
	internal bool? CachedCheckVersionCompatibility { private get; init; }
	internal Platform? CachedDefaultPlatform { private get; init; }

	///<summary>Constructs a cache volume for a given cache key.</summary>
	///<param name = "Key">A string identifier to target this cache volume (e.g., "modules-cache").</param>
	public CacheVolume CacheVolume(string key)
	{
		return new CacheVolume
		{
			QueryTree = QueryTree.Add("cacheVolume", new OperationArgument("key", key, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Checks if the current Dagger Engine is compatible with an SDK's required version.</summary>
	///<param name = "Version">The SDK's required version.</param>
	public async Task<bool> CheckVersionCompatibility(string version)
	{
		return (await ComputeQuery(QueryTree.Add("checkVersionCompatibility", new OperationArgument("version", version, ParameterSerialization.String, false)), await Context.Connection())).Deserialize<bool>();
	}

	///<summary><para>Creates a scratch container or loads one by ID.</para><para>Optional platform argument initializes new containers to execute and publish as that platform. Platform defaults to that of the builder's host.</para></summary>
	///<param name = "Id"></param>
	///<param name = "Platform"></param>
	public Container Container(ContainerID? id = default, Platform? platform = default)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("container", new OperationArgument("id", id?.Value, ParameterSerialization.String, false), new OperationArgument("platform", platform?.Value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>The FunctionCall context that the SDK caller is currently executing in. If the caller is not currently executing in a function, this will return an error.</summary>
	public FunctionCall CurrentFunctionCall()
	{
		return new FunctionCall
		{
			QueryTree = QueryTree.Add("currentFunctionCall"),
			Context = Context
		};
	}

	///<summary>The module currently being served in the session, if any.</summary>
	public Module CurrentModule()
	{
		return new Module
		{
			QueryTree = QueryTree.Add("currentModule"),
			Context = Context
		};
	}

	///<summary>The default platform of the builder.</summary>
	public async Task<Platform> DefaultPlatform()
	{
		return new((await ComputeQuery(QueryTree.Add("defaultPlatform"), await Context.Connection())).Deserialize<string>());
	}

	///<summary>Creates an empty directory or loads one by ID.</summary>
	///<param name = "Id"></param>
	public Directory Directory(DirectoryID? id = default)
	{
		return new Directory
		{
			QueryTree = QueryTree.Add("directory", new OperationArgument("id", id?.Value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	[Obsolete("Use `loadFileFromID` instead.")]
	///<summary>Loads a file by ID.</summary>
	///<param name = "Id"></param>
	public File File(FileID id)
	{
		return new File
		{
			QueryTree = QueryTree.Add("file", new OperationArgument("id", id.Value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Create a function.</summary>
	///<param name = "Name"></param>
	///<param name = "ReturnType"></param>
	public Function Function(string name, TypeDef returnType)
	{
		return new Function
		{
			QueryTree = QueryTree.Add("function", new OperationArgument("name", name, ParameterSerialization.String, false), new OperationArgument("returnType", returnType, ParameterSerialization.Reference, false)),
			Context = Context
		};
	}

	///<summary>Create a code generation result, given a directory containing the generated code.</summary>
	///<param name = "Code"></param>
	public GeneratedCode GeneratedCode(Directory code)
	{
		return new GeneratedCode
		{
			QueryTree = QueryTree.Add("generatedCode", new OperationArgument("code", code, ParameterSerialization.Reference, false)),
			Context = Context
		};
	}

	///<summary>Queries a git repository.</summary>
	///<param name = "Url">Url of the git repository. Can be formatted as `https://{host}/{owner}/{repo}`, `git@{host}:{owner}/{repo}` Suffix ".git" is optional.</param>
	///<param name = "KeepGitDir">Set to true to keep .git directory.</param>
	///<param name = "SshKnownHosts">Set SSH known hosts</param>
	///<param name = "SshAuthSocket">Set SSH auth socket</param>
	///<param name = "ExperimentalServiceHost">A service which must be started before the repo is fetched.</param>
	public GitRepository Git(string url, bool? keepGitDir = default, string? sshKnownHosts = default, Socket? sshAuthSocket = default, Service? experimentalServiceHost = default)
	{
		return new GitRepository
		{
			QueryTree = QueryTree.Add("git", new OperationArgument("url", url, ParameterSerialization.String, false), new OperationArgument("keepGitDir", keepGitDir, ParameterSerialization.Enum, false), new OperationArgument("sshKnownHosts", sshKnownHosts, ParameterSerialization.String, false), new OperationArgument("sshAuthSocket", sshAuthSocket, ParameterSerialization.Reference, false), new OperationArgument("experimentalServiceHost", experimentalServiceHost, ParameterSerialization.Reference, false)),
			Context = Context
		};
	}

	///<summary>Queries the host environment.</summary>
	public Host GetHost()
	{
		return new Host
		{
			QueryTree = QueryTree.Add("host"),
			Context = Context
		};
	}

	///<summary>Returns a file containing an http remote url content.</summary>
	///<param name = "Url">HTTP url to get the content from (e.g., "https://docs.dagger.io").</param>
	///<param name = "ExperimentalServiceHost">A service which must be started before the URL is fetched.</param>
	public File Http(string url, Service? experimentalServiceHost = default)
	{
		return new File
		{
			QueryTree = QueryTree.Add("http", new OperationArgument("url", url, ParameterSerialization.String, false), new OperationArgument("experimentalServiceHost", experimentalServiceHost, ParameterSerialization.Reference, false)),
			Context = Context
		};
	}

	///<summary>Load a CacheVolume from its ID.</summary>
	///<param name = "Id"></param>
	public CacheVolume LoadCacheVolumeFromID(CacheVolumeID id)
	{
		return new CacheVolume
		{
			QueryTree = QueryTree.Add("loadCacheVolumeFromID", new OperationArgument("id", id.Value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Loads a container from an ID.</summary>
	///<param name = "Id"></param>
	public Container LoadContainerFromID(ContainerID id)
	{
		return new Container
		{
			QueryTree = QueryTree.Add("loadContainerFromID", new OperationArgument("id", id.Value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Load a Directory from its ID.</summary>
	///<param name = "Id"></param>
	public Directory LoadDirectoryFromID(DirectoryID id)
	{
		return new Directory
		{
			QueryTree = QueryTree.Add("loadDirectoryFromID", new OperationArgument("id", id.Value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Load a File from its ID.</summary>
	///<param name = "Id"></param>
	public File LoadFileFromID(FileID id)
	{
		return new File
		{
			QueryTree = QueryTree.Add("loadFileFromID", new OperationArgument("id", id.Value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Load a function argument by ID.</summary>
	///<param name = "Id"></param>
	public FunctionArg LoadFunctionArgFromID(FunctionArgID id)
	{
		return new FunctionArg
		{
			QueryTree = QueryTree.Add("loadFunctionArgFromID", new OperationArgument("id", id.Value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Load a function by ID.</summary>
	///<param name = "Id"></param>
	public Function LoadFunctionFromID(FunctionID id)
	{
		return new Function
		{
			QueryTree = QueryTree.Add("loadFunctionFromID", new OperationArgument("id", id.Value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Load a GeneratedCode by ID.</summary>
	///<param name = "Id"></param>
	public GeneratedCode LoadGeneratedCodeFromID(GeneratedCodeID id)
	{
		return new GeneratedCode
		{
			QueryTree = QueryTree.Add("loadGeneratedCodeFromID", new OperationArgument("id", id.Value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Load a git ref from its ID.</summary>
	///<param name = "Id"></param>
	public GitRef LoadGitRefFromID(GitRefID id)
	{
		return new GitRef
		{
			QueryTree = QueryTree.Add("loadGitRefFromID", new OperationArgument("id", id.Value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Load a git repository from its ID.</summary>
	///<param name = "Id"></param>
	public GitRepository LoadGitRepositoryFromID(GitRepositoryID id)
	{
		return new GitRepository
		{
			QueryTree = QueryTree.Add("loadGitRepositoryFromID", new OperationArgument("id", id.Value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Load a module by ID.</summary>
	///<param name = "Id"></param>
	public Module LoadModuleFromID(ModuleID id)
	{
		return new Module
		{
			QueryTree = QueryTree.Add("loadModuleFromID", new OperationArgument("id", id.Value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Load a Secret from its ID.</summary>
	///<param name = "Id"></param>
	public Secret LoadSecretFromID(SecretID id)
	{
		return new Secret
		{
			QueryTree = QueryTree.Add("loadSecretFromID", new OperationArgument("id", id.Value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Loads a service from ID.</summary>
	///<param name = "Id"></param>
	public Service LoadServiceFromID(ServiceID id)
	{
		return new Service
		{
			QueryTree = QueryTree.Add("loadServiceFromID", new OperationArgument("id", id.Value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Load a Socket from its ID.</summary>
	///<param name = "Id"></param>
	public Socket LoadSocketFromID(SocketID id)
	{
		return new Socket
		{
			QueryTree = QueryTree.Add("loadSocketFromID", new OperationArgument("id", id.Value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Load a TypeDef by ID.</summary>
	///<param name = "Id"></param>
	public TypeDef LoadTypeDefFromID(TypeDefID id)
	{
		return new TypeDef
		{
			QueryTree = QueryTree.Add("loadTypeDefFromID", new OperationArgument("id", id.Value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Create a new module.</summary>
	public Module GetModule()
	{
		return new Module
		{
			QueryTree = QueryTree.Add("module"),
			Context = Context
		};
	}

	///<summary>Load the static configuration for a module from the given source directory and optional subpath.</summary>
	///<param name = "SourceDirectory"></param>
	///<param name = "Subpath"></param>
	public ModuleConfig ModuleConfig(Directory sourceDirectory, string? subpath = default)
	{
		return new ModuleConfig
		{
			QueryTree = QueryTree.Add("moduleConfig", new OperationArgument("sourceDirectory", sourceDirectory, ParameterSerialization.Reference, false), new OperationArgument("subpath", subpath, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Creates a named sub-pipeline.</summary>
	///<param name = "Name">Pipeline name.</param>
	///<param name = "Description">Pipeline description.</param>
	///<param name = "Labels">Pipeline labels.</param>
	public Client Pipeline(string name, string? description = default, IReadOnlyList<PipelineLabel>? labels = default)
	{
		return new Client
		{
			QueryTree = QueryTree.Add("pipeline", new OperationArgument("name", name, ParameterSerialization.String, false), new OperationArgument("description", description, ParameterSerialization.String, false), new OperationArgument("labels", labels, ParameterSerialization.Object, true)),
			Context = Context
		};
	}

	[Obsolete("Use `loadSecretFromID` instead")]
	///<summary>Loads a secret from its ID.</summary>
	///<param name = "Id"></param>
	public Secret Secret(SecretID id)
	{
		return new Secret
		{
			QueryTree = QueryTree.Add("secret", new OperationArgument("id", id.Value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Sets a secret given a user defined name to its plaintext and returns the secret. The plaintext value is limited to a size of 128000 bytes.</summary>
	///<param name = "Name">The user defined name for this secret</param>
	///<param name = "Plaintext">The plaintext of the secret</param>
	public Secret SetSecret(string name, string plaintext)
	{
		return new Secret
		{
			QueryTree = QueryTree.Add("setSecret", new OperationArgument("name", name, ParameterSerialization.String, false), new OperationArgument("plaintext", plaintext, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	[Obsolete("Use `loadSocketFromID` instead.")]
	///<summary>Loads a socket by its ID.</summary>
	///<param name = "Id"></param>
	public Socket Socket(SocketID? id = default)
	{
		return new Socket
		{
			QueryTree = QueryTree.Add("socket", new OperationArgument("id", id?.Value, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Create a new TypeDef.</summary>
	public TypeDef GetTypeDef()
	{
		return new TypeDef
		{
			QueryTree = QueryTree.Add("typeDef"),
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

	///<summary>The identifier for this secret.</summary>
	public async Task<SecretID> Id()
	{
		if (CachedId != null)
			return CachedId;
		return new((await ComputeQuery(QueryTree.Add("id"), await Context.Connection())).Deserialize<string>());
	}

	///<summary>The value of this secret.</summary>
	public async Task<string> Plaintext()
	{
		if (CachedPlaintext != null)
			return CachedPlaintext;
		return (await ComputeQuery(QueryTree.Add("plaintext"), await Context.Connection())).Deserialize<string>();
	}
}

///<summary></summary>
public sealed class Service : BaseClient
{
	internal ServiceID? CachedId { private get; init; }
	internal string? CachedEndpoint { private get; init; }
	internal string? CachedHostname { private get; init; }
	internal ServiceID? CachedStart { private get; init; }
	internal ServiceID? CachedStop { private get; init; }

	///<summary>A unique identifier for this service.</summary>
	public async Task<ServiceID> Id()
	{
		if (CachedId != null)
			return CachedId;
		return new((await ComputeQuery(QueryTree.Add("id"), await Context.Connection())).Deserialize<string>());
	}

	///<summary><para>Retrieves an endpoint that clients can use to reach this container.</para><para>If no port is specified, the first exposed port is used. If none exist an error is returned.</para><para>If a scheme is specified, a URL is returned. Otherwise, a host:port pair is returned.</para></summary>
	///<param name = "Port">The exposed port number for the endpoint</param>
	///<param name = "Scheme">Return a URL with the given scheme, eg. http for http://</param>
	public async Task<string> Endpoint(int? port = default, string? scheme = default)
	{
		if (CachedEndpoint != null)
			return CachedEndpoint;
		return (await ComputeQuery(QueryTree.Add("endpoint", new OperationArgument("port", port, ParameterSerialization.Enum, false), new OperationArgument("scheme", scheme, ParameterSerialization.String, false)), await Context.Connection())).Deserialize<string>();
	}

	///<summary>Retrieves a hostname which can be used by clients to reach this container.</summary>
	public async Task<string> Hostname()
	{
		if (CachedHostname != null)
			return CachedHostname;
		return (await ComputeQuery(QueryTree.Add("hostname"), await Context.Connection())).Deserialize<string>();
	}

	///<summary>Retrieves the list of ports provided by the service.</summary>
	public async Task<ImmutableArray<Port>> Ports()
	{
		return (await ComputeQuery(QueryTree.Add("ports").Add("description port protocol"), await Context.Connection())).EnumerateArray().Select(json => new Port { QueryTree = QueryTree, Context = Context, CachedDescription = json.GetProperty("description").Deserialize<string?>(), CachedPort = json.GetProperty("port").Deserialize<int>(), CachedProtocol = json.GetProperty("protocol").Deserialize<NetworkProtocol>() }).ToImmutableArray();
	}

	///<summary><para>Start the service and wait for its health checks to succeed.</para><para>Services bound to a Container do not need to be manually started.</para></summary>
	public async Task<Service> Start()
	{
		await ComputeQuery(QueryTree.Add("start"), await Context.Connection());
		return this;
	}

	///<summary>Stop the service.</summary>
	public async Task<Service> Stop()
	{
		await ComputeQuery(QueryTree.Add("stop"), await Context.Connection());
		return this;
	}
}

///<summary></summary>
public sealed class Socket : BaseClient
{
	internal SocketID? CachedId { private get; init; }

	///<summary>The content-addressed identifier of the socket.</summary>
	public async Task<SocketID> Id()
	{
		if (CachedId != null)
			return CachedId;
		return new((await ComputeQuery(QueryTree.Add("id"), await Context.Connection())).Deserialize<string>());
	}
}

///<summary>A definition of a parameter or return type in a Module.</summary>
public sealed class TypeDef : BaseClient
{
	internal TypeDefID? CachedId { private get; init; }
	internal TypeDefKind? CachedKind { private get; init; }
	internal bool? CachedOptional { private get; init; }

	///<summary></summary>
	public async Task<TypeDefID> Id()
	{
		if (CachedId != null)
			return CachedId;
		return new((await ComputeQuery(QueryTree.Add("id"), await Context.Connection())).Deserialize<string>());
	}

	///<summary>If kind is LIST, the list-specific type definition. If kind is not LIST, this will be null.</summary>
	public ListTypeDef AsList()
	{
		return new ListTypeDef
		{
			QueryTree = QueryTree.Add("asList"),
			Context = Context
		};
	}

	///<summary>If kind is OBJECT, the object-specific type definition. If kind is not OBJECT, this will be null.</summary>
	public ObjectTypeDef AsObject()
	{
		return new ObjectTypeDef
		{
			QueryTree = QueryTree.Add("asObject"),
			Context = Context
		};
	}

	///<summary>The kind of type this is (e.g. primitive, list, object)</summary>
	public async Task<TypeDefKind?> Kind()
	{
		if (CachedKind != null)
			return CachedKind.Value;
		return (await ComputeQuery(QueryTree.Add("kind"), await Context.Connection())).Deserialize<TypeDefKind?>();
	}

	///<summary>Whether this type can be set to null. Defaults to false.</summary>
	public async Task<bool> IsOptional()
	{
		if (CachedOptional != null)
			return CachedOptional.Value;
		return (await ComputeQuery(QueryTree.Add("optional"), await Context.Connection())).Deserialize<bool>();
	}

	///<summary>Adds a function for constructing a new instance of an Object TypeDef, failing if the type is not an object.</summary>
	///<param name = "Function"></param>
	public TypeDef WithConstructor(Function function)
	{
		return new TypeDef
		{
			QueryTree = QueryTree.Add("withConstructor", new OperationArgument("function", function, ParameterSerialization.Reference, false)),
			Context = Context
		};
	}

	///<summary>Adds a static field for an Object TypeDef, failing if the type is not an object.</summary>
	///<param name = "Name">The name of the field in the object</param>
	///<param name = "TypeDef">The type of the field</param>
	///<param name = "Description">A doc string for the field, if any</param>
	public TypeDef WithField(string name, TypeDef typeDef, string? description = default)
	{
		return new TypeDef
		{
			QueryTree = QueryTree.Add("withField", new OperationArgument("name", name, ParameterSerialization.String, false), new OperationArgument("typeDef", typeDef, ParameterSerialization.Reference, false), new OperationArgument("description", description, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Adds a function for an Object TypeDef, failing if the type is not an object.</summary>
	///<param name = "Function"></param>
	public TypeDef WithFunction(Function function)
	{
		return new TypeDef
		{
			QueryTree = QueryTree.Add("withFunction", new OperationArgument("function", function, ParameterSerialization.Reference, false)),
			Context = Context
		};
	}

	///<summary>Sets the kind of the type.</summary>
	///<param name = "Kind"></param>
	public TypeDef WithKind(TypeDefKind kind)
	{
		return new TypeDef
		{
			QueryTree = QueryTree.Add("withKind", new OperationArgument("kind", kind, ParameterSerialization.Enum, false)),
			Context = Context
		};
	}

	///<summary>Returns a TypeDef of kind List with the provided type for its elements.</summary>
	///<param name = "ElementType"></param>
	public TypeDef WithListOf(TypeDef elementType)
	{
		return new TypeDef
		{
			QueryTree = QueryTree.Add("withListOf", new OperationArgument("elementType", elementType, ParameterSerialization.Reference, false)),
			Context = Context
		};
	}

	///<summary><para>Returns a TypeDef of kind Object with the provided name.</para><para>Note that an object's fields and functions may be omitted if the intent is only to refer to an object. This is how functions are able to return their own object, or any other circular reference.</para></summary>
	///<param name = "Name"></param>
	///<param name = "Description"></param>
	public TypeDef WithObject(string name, string? description = default)
	{
		return new TypeDef
		{
			QueryTree = QueryTree.Add("withObject", new OperationArgument("name", name, ParameterSerialization.String, false), new OperationArgument("description", description, ParameterSerialization.String, false)),
			Context = Context
		};
	}

	///<summary>Sets whether this type can be set to null.</summary>
	///<param name = "Optional"></param>
	public TypeDef WithOptional(bool optional)
	{
		return new TypeDef
		{
			QueryTree = QueryTree.Add("withOptional", new OperationArgument("optional", optional, ParameterSerialization.Enum, false)),
			Context = Context
		};
	}
}// This file was auto-generated by DaggerSDKCodeGen
// Do not make direct changes to this file.
