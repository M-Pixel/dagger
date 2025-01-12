# Dependency Downloader

Primes a container for thunking.  Runs once per module version, while Thunk runs once per invocation.

## Module Layout Detection

Determines where the module assembly is.

## NuGet Restore

Makes it possible to install runtime dependencies from NuGet based on a `.deps.json` file.  This allows Dagger modules to share dependencies (e.g. if there are 10 modules that all require `Newtonsoft.Json`, there only need be one instance of `Newtonsoft.Json` on disk), and allows modules to be published as pre-compiled without including copies of their dependencies.
