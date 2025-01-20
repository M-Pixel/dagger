// Wrapper around the SDK module implementation, adding in-memory servers, so that local iteration of the SDK module
//itself is possible without making round-trips through public NuGet or container registry.

package main

import (
	"context"
	"dagger/bootstrap/internal/dagger"
)

type Bootstrap struct {
	RequiredPaths []string
}

const (
	uid = "1654" // "1654 is 1000 + the ASCII values of each of the characters in dotnet"
)

func Build(source *dagger.Directory, name string) *dagger.Directory {
	return dag.DotnetSDK().DotnetSDKContainer().
		WithDirectory(".", source, dagger.ContainerWithDirectoryOpts{Owner: uid}).
		WithExec([]string{
			"dotnet", "build", name, "--output=/Out", "--nologo", "--configuration=Release", "--os=linux",
			"-property:ContinuousIntegrationBuild=true", "-maxCpuCount"}).
		Directory("/Out")
}

// TODO: Include Client in this script, instead of relying on it having been published to NuGet already.

func (sdk *Bootstrap) Primer(source *dagger.Directory) *dagger.Container {
	return dag.Container().
		WithDirectory("/Dependencies", dag.Directory(), dagger.ContainerWithDirectoryOpts{Owner: uid}).
		WithDirectory("/PrimedState", dag.Directory(), dagger.ContainerWithDirectoryOpts{Owner: uid}).
		WithDirectory("/Primer", Build(source, "Primer"),
			dagger.ContainerWithDirectoryOpts{Exclude: []string{"*.pdb"}})

}

func (sdk *Bootstrap) CodeGenerator(source *dagger.Directory) *dagger.Container {
	return dag.Container().
		WithDirectory("/Reference", dag.Directory(), dagger.ContainerWithDirectoryOpts{Owner: uid}).
		WithDirectory("/CodeGenerator", Build(source, "CodeGenerator"),
			// Primer includes its dependencies, so that it can be downloaded and run without needing the large dotnet
			// SDK or NuGet client.  The other programs on the other hand can rely on Primer to download their
			// dependencies, so that Foo.dll isn't redundantly included in multiple layers/images.  For reasons
			// explained in `Primer.cs`, it needs to put those dependencies in the same folder as `Dagger.Program.dll`.
			// So: this directory needs to be writable by the user, and it can exclude the dependency dlls which `dotnet
			// build` unavoidably copies into the build output.
			dagger.ContainerWithDirectoryOpts{
				Owner: uid,
				// While it's possible to rely on Primer to pull Dagger.Client from NuGet, due to the fact that project
				// references don't save the "lib/target" subpath in `.deps.json`, Primer's NuGet client would need some
				// additional complexity to handle this case.  At the moment, that complexity doesn't seem worth adding
				// to save a mere ~37 KiB.
				Include: []string{"Dagger.CodeGenerator.*", "Dagger.Client.dll"},
				Exclude: []string{"Dagger.CodeGenerator.pdb"}})
}

func (sdk *Bootstrap) Thunk(source *dagger.Directory) *dagger.Container {
	return dag.Container().
		WithDirectory("/Thunk", Build(source, "Thunk"),
			dagger.ContainerWithDirectoryOpts{
				Owner:   uid,
				Include: []string{"Dagger.Thunk.*", "Dagger.Client.dll"},
				Exclude: []string{"Dagger.Thunk.pdb"}})
}

func (sdk *Bootstrap) ClientLayer(source *dagger.Directory) *dagger.Container {
	return dag.Container().
		WithFile("/home/app/.nuget/NuGet/NuGet.Config", dag.CurrentModule().Source().File("NuGet.Config"),
			dagger.ContainerWithFileOpts{Owner: uid}).
		WithDirectory("/NuGetRepo", sdk.ClientPackages(source))
}

func (sdk *Bootstrap) ClientPackages(source *dagger.Directory) *dagger.Directory {
	return dag.DotnetSDK().DotnetSDKContainer().
		WithDirectory(".", source, dagger.ContainerWithDirectoryOpts{Owner: uid}).
		WithExec([]string{
			"dotnet", "pack", ".", "--output=/Out", "--nologo", "-property:ContinuousIntegrationBuild=true",
			"-maxCpuCount"}).
		Directory("/Out")
}

func (sdk *Bootstrap) ModuleRuntime(
	ctx context.Context,
	modSource *dagger.ModuleSource,
	introspectionJson *dagger.File,
	// +defaultPath="/sdk/dotnet"
	// +ignore=["*", "!*/*.csproj", "!Thunk/dagger.json", "!*/**/*.cs", "!Client/dagger-icon.png"]
	source *dagger.Directory,
) (*dagger.Container, error) {
	// Thunk needs Dagger.Generated.csproj to compile.
	source = source.WithDirectory("/Thunk", dag.DotnetSDK().CodegenImplementation(introspectionJson))
	return dag.DotnetSDK().
		Inject(
			sdk.ClientLayer(source.Directory("/Client")),
			sdk.Primer(source),
			sdk.CodeGenerator(source),
			dagger.DotnetSDKInjectOpts{Thunk: sdk.Thunk(source)}).
		ModuleRuntime(modSource), nil
}

func (sdk *Bootstrap) Codegen(
	ctx context.Context,
	modSource *dagger.ModuleSource,
	introspectionJson *dagger.File,
	// +defaultPath="/sdk/dotnet"
	// +ignore=["*", "!*/*.csproj", "!Thunk/dagger.json", "!*/**/*.cs", "!Client/dagger-icon.png"]
	source *dagger.Directory,
) (*dagger.GeneratedCode, error) {
	return dag.DotnetSDK().
		Inject(
			sdk.ClientLayer(source.Directory("/Client")),
			sdk.Primer(source),
			sdk.CodeGenerator(source)).
		Codegen(modSource, introspectionJson), nil
}
