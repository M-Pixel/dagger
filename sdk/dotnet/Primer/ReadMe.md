# Dependency Downloader

Primes a container for thunking.  Runs once per module version, while Thunk runs once per invocation.

## Features

### Module Layout Detection

Determines where the module assembly is (e.g. `./Module.dll` vs `bin/Debug/netstandard2.0/Module.dll` vs `Module/bin/Release/net8.0/linux-x64/Module.dll`).

### NuGet Restore

Makes it possible to install runtime dependencies from NuGet based on a `.deps.json` file.  This allows Dagger modules to share dependencies (e.g. if there are 10 modules that all require `Newtonsoft.Json`, there only need be one instance of `Newtonsoft.Json` on disk), and allows modules to be published as pre-compiled without including copies of their dependencies.

### Module Installation

Provides the ability to install modules directly from NuGet packages, instead of being mounted in from context.  Allows distributing modules in a way that is idiomatic to Dotnet and far more efficient than pulling in a Git repository, or using an official NuGet client that writes the compressed package to disk in addition to the extracted files.

## Bootstrapping

...
