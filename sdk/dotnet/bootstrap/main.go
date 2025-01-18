// Builds and publishes all the component Dotnet assemblies required for the SDK module to function correctly.
// Minimalistic.  Usually you should use the more robust per-project pipelines, but those are built on the dotnet SDK,
// so this is useful if you break/change a lot of things and can't rely on earlier versions of the binaries.

package main

import (
	"context"
	"dagger/bootstrap/internal/dagger"
)

type Bootstrap struct{}

const (
	uid = "1654" // "1654 is 1000 + the ASCII values of each of the characters in dotnet"
)

func SdkContainer() *dagger.Container {
	// `alpine` is slightly smaller than `noble`, but the SDK module uses the distroless variant, which will share
	// more layers with Ubuntu than with Alpine.
	return dag.Container().
		From("mcr.microsoft.com/dotnet/sdk:8.0-noble").
		WithUser(uid).
		WithWorkdir("/home/app").
		WithEnvVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1").
		WithMountedTemp("/tmp").
		WithMountedCache("/home/app/.local/share/NuGet/http-cache", dag.CacheVolume("nuget-http"),
			dagger.ContainerWithMountedCacheOpts{Owner: uid, Sharing: dagger.CacheSharingModeShared}).
		WithDirectory("out", dag.Directory(), dagger.ContainerWithDirectoryOpts{Owner: uid})
}

func Build(source *dagger.Directory, name string) *dagger.Directory {
	return SdkContainer().
		WithDirectory(name, source, dagger.ContainerWithDirectoryOpts{Owner: uid}).
		WithExec([]string{"dotnet", "build", name, "--output=out", "--nologo", "--os=linux", "-property:ContinuousIntegrationBuild=true", "-maxCpuCount"}).
		Directory("out")
}

// TODO: Include Client in this script, instead of relying on it having been published to NuGet already.

func (m *Bootstrap) Primer(
	ctx context.Context,
	// +defaultPath="/sdk/dotnet/Primer"
	// +ignore=["*", "!*.csproj", "!**/*.cs"]
	source *dagger.Directory,
	url string,
	user string,
	secret *dagger.Secret,
) (string, error) {
	version, _ := dag.Version(ctx)
	return dag.Container().
		WithDirectory("/Dependencies", dag.Directory(), dagger.ContainerWithDirectoryOpts{Owner: uid}).
		WithDirectory("/PrimedState", dag.Directory(), dagger.ContainerWithDirectoryOpts{Owner: uid}).
		WithDirectory("/Primer", Build(source, "Primer")).
		WithRegistryAuth(url, user, secret).
		Publish(ctx, url+"/dagger-dotnet-primer:"+version)
}

func (m *Bootstrap) CodeGenerator(
	ctx context.Context,
	// +defaultPath="/sdk/dotnet/CodeGenerator"
	// +ignore=["*", "!*.csproj", "!**/*.cs"]
	source *dagger.Directory,
	url string,
	user string,
	secret *dagger.Secret,
) (string, error) {
	version, _ := dag.Version(ctx)
	return dag.Container().
		WithDirectory("/Reference", dag.Directory(), dagger.ContainerWithDirectoryOpts{Owner: uid}).
		WithDirectory("/CodeGenerator", Build(source, "CodeGenerator"), dagger.ContainerWithDirectoryOpts{Owner: uid}).
		WithRegistryAuth(url, user, secret).
		Publish(ctx, url+"/dagger-dotnet-codegenerator:"+version)
}

func (m *Bootstrap) Thunk(
	ctx context.Context,
	// +defaultPath="/sdk/dotnet/Thunk"
	// +ignore=["*", "!*.csproj", "!**/*.cs", "!Generated/*"]
	source *dagger.Directory,
	url string,
	user string,
	secret *dagger.Secret,
) (string, error) {
	version, _ := dag.Version(ctx)
	return dag.Container().
		WithDirectory("/ThunkDependencies", dag.Directory(), dagger.ContainerWithDirectoryOpts{Owner: uid}).
		WithDirectory("/Thunk", Build(source, "Thunk"), dagger.ContainerWithDirectoryOpts{Owner: uid}).
		WithRegistryAuth(url, user, secret).
		Publish(ctx, url+"/dagger-dotnet-thunk:"+version)
}

func (m *Bootstrap) Debug(ctx context.Context) (string, error) {
	return dag.Container().From("docker.io/busybox").WithDirectory("/mnt", dag.Container().From("ghcr.io/m-pixel/dagger-dotnet-codegenerator:0.0.0").Directory("/")).
		WithExec([]string{"ls", "-la", "/mnt"}).
		Stdout(ctx)
}
