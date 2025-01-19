package main

import (
	"context"
	"fmt"
	"main/internal/dagger"
	"os"
	"path"
	"strings"
)

const (
	uid                  = "1654"
	registry             = "ghcr.io/m-pixel/"
	primerExitNotFound   = 120
	primerExitSameFolder = 121
	primerExitSubFolder  = 122
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

func DotnetContainer(container *dagger.Container) *dagger.Container {
	return container.
		WithEnvVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1").
		WithMountedTemp("/tmp").
		WithMountedCache("/home/app/.local/share/NuGet/http-cache", dag.CacheVolume("nuget-http"),
			dagger.ContainerWithMountedCacheOpts{Owner: uid, Sharing: dagger.CacheSharingModeShared}).
		WithWorkdir("/scratch"). // Match the Dagger convention for running modules workdir name
		WithDirectory(".", dag.Directory(), dagger.ContainerWithDirectoryOpts{Owner: uid})
	// TODO: Figure out if any additional directories should have cache mounted
}

func DotnetRuntimeContainer() *dagger.Container {
	return DotnetContainer(dag.Container().
		//From("mcr.microsoft.com/dotnet/runtime:8.0-noble").WithUser(uid),
		From("mcr.microsoft.com/dotnet/runtime:8.0-noble-chiseled"), // "chiseled" means distroless
	)
}

func (sdk *DotnetSdk) ModuleRuntime(
	ctx context.Context,
	modSource *dagger.ModuleSource,
) (*dagger.Container, error) {
	subPath, err := modSource.SourceSubpath(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to retrieve module source subpath for dotnet invocation: %v", err)
	}

	name, err := modSource.ModuleName(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to retrieve module name for dotnet invocation: %v", err)
	}

	version, _ := dag.Version(ctx)
	// TODO: Is it beneficial for any of this to be async?
	// TODO: Support compiling the module if it's not pre-compiled

	// TODO: Configurable source container URL (and is it possible to bind a service to an SDK container?)
	readyToPrimeContainer := DotnetRuntimeContainer().
		// TODO: Wrapper SDK that attaches service for self-hosting the containers, builds them.
		WithDirectory("/", dag.Container().From(registry+"dagger-dotnet-primer:"+version).Directory("/")).
		WithDirectory("/", dag.Container().From(registry+"dagger-dotnet-thunk:"+version).Directory("/")).

		// Download Thunk's NuGet dependencies.  Runs once per Thunk release (or each time nuget cache is GC'd).
		WithEnvVariable("Dagger:Module:SourcePath", "/Thunk").
		WithEnvVariable("Dagger:Module:IsCore", "").
		WithExec([]string{"dotnet", "/Primer/Dagger.Primer.dll"}).
		WithoutEnvVariable("Dagger:Module:IsCore").

		// Discover Module's layout and maybe download its NuGet assemblies.  Runs once per module release.
		WithEnvVariable("Dagger:Module:Name", name).
		WithEnvVariable("Dagger:Module:SourcePath", path.Join("/Module", subPath))
	maybeReadyToInvokeContainer := readyToPrimeContainer.
		WithMountedDirectory("/Module", modSource.ContextDirectory()).
		WithExec(
			[]string{"/usr/bin/dotnet", "/Primer/Dagger.Primer.dll"},
			dagger.ContainerWithExecOpts{Expect: dagger.ReturnTypeAny})

	var readyToInvokeContainer *dagger.Container
	primerResponse, err := maybeReadyToInvokeContainer.ExitCode(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to get exit code for dotnet invocation: %v", err)
	}
	if primerResponse >= 120 {
		// Needs to be built.

		// Is csproj in src dir or in subdir?
		var target = "."
		if primerResponse == primerExitNotFound {
			return nil, fmt.Errorf(
				"couldn't find assembly or project - must have %s/%s.csproj, or %s/%s/%s.csproj",
				subPath, name, subPath, name, name)
		} else if primerResponse == primerExitSubFolder {
			target = name
		} else if primerResponse != primerExitSameFolder {
			return nil, fmt.Errorf("unexpected primer response: %d", primerResponse)
		}

		// `alpine` is slightly smaller than `noble`, but the SDK module uses the distroless variant, which will share
		// more layers with Ubuntu than with Alpine.
		buildDirectory := DotnetContainer(dag.Container().From("mcr.microsoft.com/dotnet/sdk:8.0-noble").WithUser(uid)).
			WithDirectory("/Out", dag.Directory(), dagger.ContainerWithDirectoryOpts{Owner: uid}).
			WithDirectory("/scratch", modSource.ContextDirectory(), dagger.ContainerWithDirectoryOpts{Owner: uid}).
			WithWorkdir(subPath).
			WithExec([]string{"dotnet", "build", target, "--nologo", "--os=linux", "-property:ContinuousIntegrationBuild=true", "-maxCpuCount"})
		readyToInvokeContainer = maybeReadyToInvokeContainer.
			WithMountedDirectory("/Module", buildDirectory.Directory("/scratch")).
			WithExec([]string{"/usr/bin/dotnet", "/Primer/Dagger.Primer.dll"})
	} else if primerResponse != 0 {
		_, err := maybeReadyToInvokeContainer.Stdout(ctx)
		return nil, fmt.Errorf("failed to prime module container for dotnet invocation: %v", err)
	} else {
		readyToInvokeContainer = maybeReadyToInvokeContainer
	}

	// Done priming, now invoke.  Runs once per invocation.
	return readyToInvokeContainer.
		WithEntrypoint([]string{"/usr/bin/dotnet", "/Thunk/Dagger.Thunk.dll"}), nil
}

func (sdk *DotnetSdk) Codegen(
	ctx context.Context,
	modSource *dagger.ModuleSource,
	introspectionJson *dagger.File,
) (*dagger.GeneratedCode, error) {
	// TODO: Don't actually generate code if the context is call (as opposed to init or sync), and a generated SDK is
	// already present or the module has no dependencies.  The generated SDK is less than 200 KB, so it can be included
	// in distributed modules, and if there are no deps than it can bind to Thunk's version.

	subPath, err := modSource.SourceSubpath(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to retrieve module source subpath for dotnet code generation: %v", err)
	}

	name, err := modSource.ModuleName(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to retrieve module name for dotnet code generation: %v", err)
	}

	version, _ := dag.Version(ctx)

	// buildDirectory composition doesn't use any parameters besides introspectionJson, so if introspectionJson is
	// identical between modules, code generation is not re-run unnecessarily.
	buildDirectory := DotnetRuntimeContainer().
		WithDirectory("/", dag.Container().From(registry+"dagger-dotnet-primer:"+version).Directory("/")).
		WithDirectory("/", dag.Container().From(registry+"dagger-dotnet-codegenerator:"+version).Directory("/")).

		// Install CodeGenerator dependencies, including reference assemblies
		WithEnvVariable("Dagger:Module:SourcePath", "/CodeGenerator").
		WithEnvVariable("Dagger:Module:IsCore", "").
		WithExec([]string{"/usr/bin/dotnet", "/Primer/Dagger.Primer.dll"}).
		WithoutEnvVariable("Dagger:Module:IsCore").
		WithoutEnvVariable("Dagger:Module:SourcePath").

		// Set code generation parameters and let it rip.
		WithMountedFile("/mnt/introspection.json", introspectionJson).
		WithExec(
			[]string{"dotnet", "/CodeGenerator/Dagger.CodeGenerator.dll"},
			dagger.ContainerWithExecOpts{ExperimentalPrivilegedNesting: true}).
		Directory(".")

	// Add csproj if not already present
	var hasCsproj = false
	var hasFolder = false
	var hasSln = false
	var hasDll = false
	entries, err := modSource.ContextDirectory().Entries(ctx, dagger.DirectoryEntriesOpts{Path: subPath})
	if err == nil {
		for _, entry := range entries {
			if strings.HasSuffix(entry, name+".dll") {
				hasDll = true
				break
			} else if entry == name+".csproj" {
				hasCsproj = true
				break
			} else if strings.HasSuffix(entry, ".sln") {
				hasSln = true
			} else if entry == name {
				hasFolder = true
			}
		}
	}
	if !hasDll && !hasCsproj && (!hasFolder || !hasSln) {
		csproj, err := os.ReadFile("/src/sdk/dotnet/module/Template.csproj")
		if err != nil {
			return nil, fmt.Errorf("failed to read file '/src/sdk/dotnet/module/Template.csproj': %v", err)
		}
		buildDirectory = buildDirectory.
			WithNewFile(name+".csproj", strings.Replace(string(csproj), "$", name, -1)).
			WithFile("Cow.cs", dag.CurrentModule().Source().File("Cow.cs"))
	}

	// TODO: Configurable whether documentation & PDB are included
	return dag.GeneratedCode(dag.Directory().WithDirectory(subPath, buildDirectory)).
		WithVCSIgnoredPaths([]string{"**/*.pdb", "bin", "obj"}), nil
}
