# Bootstrap

[The SDK module readme](../module/readme.md) explains that (and why) it relies on pre-built assemblies rather than including the whole source code of this SDK in its context and compiling them on the fly, but that, at the same time, for development purposes, it should _also_ be possible to do this:

1. Make a change to Thunk's source code.  Save it, don't build it.
2. Run a single command in your terminal.
3. Observe the end-result of your change, see if it worked or not.

This module makes that possible.  It takes the form of an SDK module, but delegates its implementation to the primary SDK module, after injecting the dependencies that the primary SDK module needs and would have otherwise downloaded from the internet had they not been injected.

Decisions about what to pull in from the context (and therefore download from GitHub) cannot be made at run-time.  These decisions are effectively baked into the `dagger.json` file and `// +ignore` metadata for `Directory` parameters.  Thus, it isn't possible for the primary SDK module to have dual modes, for retrieving its programs vs building them from source, entirely by itself.  Either the files necessary to compile Thunk are _always_ included in the SDK module even for those who don't need them, or they aren't included.  That's why this module is necessary.

This module:

1. Builds Client, Code Generator, Primer, and Thunk.
2. Injects them into the real SDK module.
3. Delegates calls to the rea real SDK module that has had its dependencies injected.

All dotnet modules in this repo (e.g. the Pipelines for testing and publishing artifacts to the public, Thunk which isn't technically a module but acts like one in order to get a Generated folder) use `../bootstrap` rather than `../module` as their SDK.  Yes, that means there's a circular dependency between this module and Thunk, but it's easy enough to work around because Thunk is only needed by the SDK module for invocation, and needs the SDK module only for generation.

As explained in the SDK module readme, it uses OCI images as the distribution mechanism for its dependencies.  As there isn't any way to have Dagger's `Container.From()` pull from a Dagger service, the containers are passed in as plain Dagger container objects.  Apart from that, the code-paths of using `bootstrap` with on-the-fly assemblies vs using `module` with pulled-from-registry assemblies are identical.  There is very little room for bugs that are specific to the pulled-from-registry flow, that would be missed in the on-the-fly flow.  To that end, **the Pipelines project that is responsible for pushing the publicly available pre-built assemblies defers to bootstrap's implementation of their build process**, using it as a "normal" module (not necessarily an SDK module).

Client is an exception to the rule about distributing as a container.  Frankly, it's a close call, but [as explained in the Client readme](../Client/ReadMe.md), it is distributed through NuGet.  Bootstrap, then, injects Client by uploading it to an ephemeral NuGet server that is bound as a service to containers that invoke Primer.
