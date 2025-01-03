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

func (sdk *DotnetSdk) ModuleRuntime(
	ctx context.Context,
	modSource *dagger.ModuleSource,
	introspectionJson *dagger.File, // TODO: Can I omit?
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

	return dag.Container().
		From("mcr.microsoft.com/dotnet/runtime:8.0-noble-chiseled").
		WithEnvVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1").
		WithMountedDirectory("/mnt/thunk", thunkBuildDirectory).
		WithMountedDirectory("/mnt/module", modSource.ContextDirectory()).
		WithEntrypoint([]string{"/usr/bin/dotnet", "/mnt/thunk/Dagger.Thunk.dll", name, subPath}), nil
}

func (sdk *DotnetSdk) Codegen(
	ctx context.Context,
	modSource *dagger.ModuleSource,
	introspectionJson *dagger.File,
// +defaultPath="/sdk/dotnet"
// +ignore=["*", "!CodeGenerator/bin/Release/net8.0/linux-x64/*", "!Client/Client.csproj", "!Client/**/*.cs", "CodeGenerator/bin/Release/net8.0/linux-x64/Dagger.CodeGen", "*.pdb"]
	sdkDirectory *dagger.Directory,
) (*dagger.GeneratedCode, error) {
	// TODO: Don't actually generate code if the context is call (as opposed to init or sync), and a generated SDK is
	// already present.  The generated SDK is less than 200 KB, so it SHOULD be included in distributed modules, so that
	// invoking dotnet modules doesn't require an over 600 MB compiler to be downloaded.

	// Must use full dotnet SDK because need msbuild to compile the generated code.  Distroless not available for
	// dotnet/sdk, only dotnet/runtime.  Alpine is smallest at 694 MB... but it's musl so it will have worse runtime
	// performance, and won't match target runtime.  bookworm-slim is 844 MB, noble (Ubuntu) is (surprisingly) less at
	// 831 MB.  Azure Linux is bigger (899 MB).  So, is it worth the extra 147 MB to do noble instead of alpine?  For
	// now, the answer is no, as glibc vs musl doesn't actually make a difference for dotnet as long as AOT isn't used
	// (and even then, it may be handled elegantly by the compiler - I've never tried).
	buildDirectory := dag.Container().
		From("mcr.microsoft.com/dotnet/sdk:8.0-alpine3.20").
		WithEnvVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1").
		WithUser("app").
		WithWorkdir("/home/app").
		// TODO: Figure out if any directories should have cache mounted

		// Run C# generator which will place output at ./Generated.cs
		WithMountedFile("introspection.json", introspectionJson).
		WithMountedDirectory("CodeGenerator", sdkDirectory.Directory("CodeGenerator/bin/Release/net8.0/linux-x64")).
		WithExec(
			[]string{"dotnet", "CodeGenerator/Dagger.CodeGenerator.dll"},
			dagger.ContainerWithExecOpts{ExperimentalPrivilegedNesting: true}).
		WithoutMount("CodeGenerator").

		// Compile SDK which references ../Generated.cs
		WithMountedDirectory("Client", sdkDirectory.Directory("Client")).
		WithMountedCache(
			"Client/obj",
			dag.CacheVolume("dotnet-sdk-obj"),
			dagger.ContainerWithMountedCacheOpts{Owner: "app"}).
		WithWorkdir("Client").
		WithExec([]string{"dotnet", "build", "--configuration=Release", "--os=linux", "--output=../out"}).
		Directory("../out")

	subPath, err := modSource.SourceSubpath(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to retrieve module source subpath for dotnet codegen: %v", err)
	}
	// TODO: Configurable whether documentation & PDB are included
	// TODO: Pass-through (or create) .csproj file, making sure that it contains a ref to the generated SDK
	return dag.GeneratedCode(dag.Directory().WithDirectory(path.Join(subPath, "Libraries"), buildDirectory)).
		WithVCSIgnoredPaths([]string{"Libraries", "bin", "obj"}), nil
}
