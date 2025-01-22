package main

import (
	"context"
	"fmt"
	"main/internal/dagger"
	"math/rand"
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

	// Used by bootstrap module.  For development only.  See [/sdk/dotnet/bootstrap/readme.md].
	// +optional
	ClientContainer *dagger.Container
	// +optional
	PrimerContainer *dagger.Container
	// +optional
	CodeGeneratorContainer *dagger.Container
	// +optional
	ThunkContainer *dagger.Container
}

func (sdk *DotnetSdk) DotnetContainer(container *dagger.Container) *dagger.Container {
	var result = container.
		WithEnvVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1").
		WithMountedTemp("/tmp").
		WithMountedCache("/home/app/.local/share/NuGet/http-cache", dag.CacheVolume("nuget-http"),
			dagger.ContainerWithMountedCacheOpts{Owner: uid, Sharing: dagger.CacheSharingModeShared}).
		WithWorkdir("/scratch"). // Match the Dagger convention for running modules workdir name
		WithDirectory(".", dag.Directory(), dagger.ContainerWithDirectoryOpts{Owner: uid})
	// TODO: Figure out if any additional directories should have cache mounted
	return result
}

func (sdk *DotnetSdk) DotnetRuntimeContainer() *dagger.Container {
	return sdk.DotnetContainer(dag.Container().
		//From("mcr.microsoft.com/dotnet/runtime:8.0-noble").WithUser(uid),
		From("mcr.microsoft.com/dotnet/runtime:8.0-noble-chiseled"), // "chiseled" means distroless
	)
}

func (sdk *DotnetSdk) DotnetSdkContainer() *dagger.Container {
	return sdk.DotnetContainer(dag.Container().From("mcr.microsoft.com/dotnet/sdk:8.0-noble")).
		WithMountedCache("/home/app/.dotnet", dag.CacheVolume(fmt.Sprintf(`nuget-home-%d`, rand.Uint64())),
			dagger.ContainerWithMountedCacheOpts{Owner: uid}).
		WithExec([]string{"dotnet", "workload", "update"}). // Prevents warning from appearing in all logs
		WithUser(uid).
		WithDirectory("/Out", dag.Directory(), dagger.ContainerWithDirectoryOpts{Owner: uid})
}

func (sdk *DotnetSdk) InjectCodegenDependencies(
	primer *dagger.Container,
	codeGenerator *dagger.Container,
) *DotnetSdk {
	sdk.PrimerContainer = primer
	sdk.CodeGeneratorContainer = codeGenerator
	return sdk
}

func (sdk *DotnetSdk) InjectModuleRuntimeDependencies(
	client *dagger.Container,
	thunk *dagger.Container,
) *DotnetSdk {
	sdk.ClientContainer = client
	sdk.ThunkContainer = thunk
	return sdk
}

func (sdk *DotnetSdk) MaybeAddClientPackage(container *dagger.Container) *dagger.Container {
	if sdk.ClientContainer != nil {
		return container.WithDirectory("/", sdk.ClientContainer.Directory("/"))
	}
	return container
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

	if sdk.PrimerContainer == nil {
		sdk.PrimerContainer = dag.Container().From(registry + "dagger-dotnet-primer:" + version)
	}
	if sdk.ThunkContainer == nil {
		sdk.ThunkContainer = dag.Container().From(registry + "dagger-dotnet-thunk:" + version)
	}
	var readyToPrimeContainer = sdk.DotnetRuntimeContainer().
		WithDirectory("/", sdk.PrimerContainer.Directory("/")).
		WithDirectory("/", sdk.ThunkContainer.Directory("/")).

		// Download Thunk's NuGet dependencies.  Runs once per Thunk release (or each time nuget cache is GC'd).
		WithEnvVariable("Dagger:Module:SourcePath", "/Thunk").
		WithEnvVariable("Dagger:Module:IsCore", "").
		WithExec([]string{"dotnet", "/Primer/Dagger.Primer.dll"}).
		WithoutEnvVariable("Dagger:Module:IsCore").

		// Discover Module's layout and maybe download its NuGet assemblies.  Runs once per module release.
		WithEnvVariable("Dagger:Module:Name", name).
		WithEnvVariable("Dagger:Module:SourcePath", path.Join("/Module", subPath))

	maybeReadyToInvokeContainer := sdk.MaybeAddClientPackage(readyToPrimeContainer).
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
			entries, err := modSource.ContextDirectory().Entries(ctx, dagger.DirectoryEntriesOpts{Path: subPath})
			if err != nil || len(entries) == 0 {
				// It's init.  TODO: Why TF is ModuleRuntime even called on init?  I shouldn't have to handle this case.  There's obviously nothing to invoke or introspect.
				return maybeReadyToInvokeContainer.WithEntrypoint([]string{""}), nil
			}
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
		buildDirectory := sdk.MaybeAddClientPackage(sdk.DotnetSdkContainer()).
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
		WithUser("0"). // https://github.com/dagger/dagger/issues/9427
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

	buildDirectory := sdk.CodegenImplementation(ctx, introspectionJson)

	// Add csproj if not already present, update client version if outdated
	var hasDll = false
	var csprojPath = ""
	entries, err := modSource.ContextDirectory().Entries(ctx, dagger.DirectoryEntriesOpts{Path: subPath})
	if err == nil {
		for _, entry := range entries {
			if strings.HasSuffix(entry, name+".dll") {
				hasDll = true
				break
			} else if entry == name+".csproj" {
				csprojPath = entry
				break
			} else if entry == name {
				innerEntries, err := modSource.ContextDirectory().
					Entries(ctx, dagger.DirectoryEntriesOpts{Path: path.Join(subPath, entry)})
				if err != nil {
					continue
				}
				for _, innerEntry := range innerEntries {
					if innerEntry == name+".csproj" {
						csprojPath = path.Join(subPath, entry)
						break
					}
				}
				if len(csprojPath) > 0 {
					break
				}
			}
		}
	}
	if len(csprojPath) > 0 {
		csprojFullPath := path.Join(subPath, csprojPath)
		csproj, err := modSource.ContextDirectory().File(csprojFullPath).Contents(ctx)
		if err == nil {
			buildDirectory = buildDirectory.WithNewFile(csprojPath, replaceVersion(csproj, "0.15.2.0")) // TODO: Don't hardcode this version
		}
	} else if !hasDll {
		csproj, err := os.ReadFile("/src/sdk/dotnet/module/Template.csproj")
		if err != nil {
			return nil, fmt.Errorf("failed to read file '/src/sdk/dotnet/module/Template.csproj': %v", err)
		}
		buildDirectory = buildDirectory.
			WithNewFile(name+".csproj", strings.Replace(string(csproj), "$", name, -1)).
			WithFile("Cow.cs", dag.CurrentModule().Source().File("Cow.cs"))
		// TODO: Add obj and bin to dagger.json ignores
	}

	// TODO: Configurable whether documentation & PDB are included
	return dag.GeneratedCode(dag.Directory().WithDirectory(subPath, buildDirectory)).
		WithVCSIgnoredPaths([]string{"**/*.pdb", "bin", "obj"}), nil
}

func (sdk *DotnetSdk) CodegenImplementation(
	ctx context.Context,
	introspectionJson *dagger.File,
) *dagger.Directory {
	version, _ := dag.Version(ctx)

	// buildDirectory composition doesn't use any parameters besides introspectionJson, so if introspectionJson is
	// identical between modules, code generation is not re-run unnecessarily.
	if sdk.PrimerContainer == nil {
		sdk.PrimerContainer = dag.Container().From(registry + "dagger-dotnet-primer:" + version)
	}
	if sdk.CodeGeneratorContainer == nil {
		sdk.CodeGeneratorContainer = dag.Container().From(registry + "dagger-dotnet-codegenerator:" + version)
	}
	return sdk.DotnetRuntimeContainer().
		WithDirectory("/", sdk.PrimerContainer.Directory("/")).
		WithDirectory("/", sdk.CodeGeneratorContainer.Directory("/")).

		// Install CodeGenerator dependencies, including reference assemblies
		WithEnvVariable("Dagger:Module:SourcePath", "/CodeGenerator").
		WithEnvVariable("Dagger:Module:IsCore", "").
		WithExec([]string{"/usr/bin/dotnet", "/Primer/Dagger.Primer.dll"}).
		WithoutEnvVariable("Dagger:Module:IsCore").
		WithoutEnvVariable("Dagger:Module:SourcePath").

		// Set code generation parameters and let it rip.
		WithMountedFile("/mnt/introspection.json", introspectionJson).
		WithExec([]string{"dotnet", "/CodeGenerator/Dagger.CodeGenerator.dll"}).
		Directory(".")
}

func replaceVersion(csproj, version string) string {
	const prefix = `<PackageReference Include="MaxPixel.Dagger.Client" Version="`
	start := strings.Index(csproj, prefix)
	if start == -1 {
		return csproj
	}

	start += len(prefix)
	end := strings.IndexByte(csproj[start:], '"')
	if end == -1 {
		return csproj
	}

	var builder strings.Builder
	builder.Grow(len(csproj) - (end - len(version)))

	builder.WriteString(csproj[:start])
	builder.WriteString(version)
	builder.WriteString(csproj[start+end:])

	return builder.String()
}
