package main

import (
	"context"
	"fmt"
	"main/internal/dagger"
	"path"
)

// core/schema/sdk.go defines an implicit interface for SDK modules.  This adheres to that interface.

// The SDKSourceDir *Directory member of the other runtimes is not relevant here; for non-builtin SDK modules, the
// parameter is given an empty folder.  Anyhow, the dotnet SDK (in proper, idiomatic dotnet style) uses its own
// Roslyn-based SDK generator instead of Dagger's go-template one, and has its own means of including those files
// without needing to be handed a SDKSourceDir argument by the engine.

type DotnetSdk struct {
	// RequiredPaths is required by Dagger.  If absent, the module will fail.  It's left empty here because it applies
	// to the entire context directory, and because the subject module's source directory will have `**/*` included
	// forcefully by moduleSourceResolveFromCaller anyway.  This SDK doesn't need to require any paths outside the
	// subject module's csproj (source) folder.
	RequiredPaths []string
}

func DotnetRuntimeContainer() *dagger.Container {
	return dag.Container().
		From("mcr.microsoft.com/dotnet/runtime:8.0-noble-chiseled").
		//From("mcr.microsoft.com/dotnet/runtime:8.0-noble").
		WithEnvVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
}

func (sdk *DotnetSdk) ModuleRuntime(
	ctx context.Context,
	modSource *dagger.ModuleSource,
// +defaultPath="/sdk/dotnet/Thunk/bin/Release/net8.0/linux-x64"
	thunkBuildDirectory *dagger.Directory,
) (*dagger.Container, error) {
	subPath, err := modSource.SourceSubpath(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to retrieve module source subpath for dotnet invocation: %v", err)
	}

	name, err := modSource.ModuleName(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to retrieve module name for dotnet invocation: %v", err)
	}
	// TODO: Support compiling the module if it's not pre-compiled
	// TODO: Can thunk program retrieve the name instead?  Thunk program requires Dagger SDK anyways for submitting introspection.

	return DotnetRuntimeContainer().
		WithMountedDirectory("/mnt/thunk", thunkBuildDirectory).
		WithMountedDirectory("/mnt/module", modSource.ContextDirectory()).
		WithEntrypoint([]string{"/usr/bin/dotnet", "/mnt/thunk/Dagger.Thunk.dll", name, subPath}), nil
}

func (sdk *DotnetSdk) Codegen(
	ctx context.Context,
	modSource *dagger.ModuleSource,
	introspectionJson *dagger.File,
// +defaultPath="/sdk/dotnet"
// +ignore=["*", "!CodeGenerator/bin/Release/net8.0/linux-x64/*", "!Client/bin/Release/net8.0/*", "CodeGenerator/bin/Release/net8.0/linux-x64/Dagger.CodeGen", "*.pdb"]
	sdkDirectory *dagger.Directory,
) (*dagger.GeneratedCode, error) {
	// TODO: Don't actually generate code if the context is call (as opposed to init or sync), and a generated SDK is
	// already present.  The generated SDK is less than 200 KB, so it SHOULD be included in distributed modules, so that
	// invoking dotnet modules doesn't require an over 600 MB compiler to be downloaded.

	subPath, err := modSource.SourceSubpath(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to retrieve module source subpath for dotnet code generation: %v", err)
	}

	name, err := modSource.ModuleName(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to retrieve module name for dotnet invocation: %v", err)
	}

	clientBuildDirectory := sdkDirectory.Directory("Client/bin/Release/net8.0")

	buildDirectory := dag.Container().
		From("mcr.microsoft.com/dotnet/sdk:8.0-alpine3.20").
		WithEnvVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1").
		WithUser("app").
		WithDirectory("scratch", dag.Directory(), dagger.ContainerWithDirectoryOpts{Owner: "app"}).
		WithWorkdir("scratch").
		// TODO: Figure out if any directories should have cache mounted
		WithMountedFile("/mnt/introspection.json", introspectionJson).
		WithMountedDirectory("/mnt/CodeGenerator", sdkDirectory.Directory("CodeGenerator/bin/Release/net8.0/linux-x64")).
		WithMountedDirectory("/mnt/Client", clientBuildDirectory).
		WithEnvVariable("Dagger:Module:Name", name).
		WithExec(
			[]string{"dotnet", "/mnt/CodeGenerator/Dagger.CodeGenerator.dll"},
			dagger.ContainerWithExecOpts{ExperimentalPrivilegedNesting: true}).
		Directory(".")

	// TODO: Configurable whether documentation & PDB are included
	// TODO: Pass-through (or create) .csproj file, making sure that it contains a ref to the generated SDK
	return dag.GeneratedCode(
		dag.Directory().
			WithDirectory(path.Join(subPath, "Libraries"), clientBuildDirectory).
			WithDirectory(subPath, buildDirectory),
	).
		WithVCSIgnoredPaths([]string{"Libraries", "bin", "obj"}), nil
}
