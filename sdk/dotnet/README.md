# Dagger Dotnet SDK

Dagger SDK for Dotnet.  Not official.  Maintained by https://m-pixel.com.

## Developing This SDK

The **Pipelines** project is a C# Dagger Module that implements CI/CD tasks for this SDK, including the ability to publish the Client library as a package to NuGet.

## Notes on Design Decisions

When I began developing this SDK, I actually began by copy-pasting the TypeScript SDK's source code and changing the syntax as necessary to eliminate compile errors.  After all, there are a lot of similar design choices behind TypeScript and C# (some of the same people worked on both languages).  My goal was to keep it as close as possible, so that upstream changes to the TypeScript SDK could be most easily adapted to the C# one.  To the extent that there were any improvements to be made to the TypeScript SDK, I figured I would incrementally make them to both SDKs at once.

However, after a while, I decided to break from this goal.  The code generators doesn't actually change very often.  And at a certain point, I became familiar enough with Dagger's inner-workings that it was easy enough to go straight from a change in the design spec to knowing how that should change the implementation.  Meanwhile, the cruft inherited from the other SDKs was actually becoming a nuisance.

As of now, the Dotnet SDK tries to strike the best possible balance between Dagger conventions and Dotnet idioms, while naming and structuring everything intuitively, paying little regard for the design choices made in other Dagger SDKs.

### Assemblies

_Why does this SDK support loading pre-built modules?  Doesn't that go agains the Dagger Modules design goal of mimicking Go, in which dependencies are referenced as GitHub repos and compiled as needed?_

Unlike other compiled languages like Go and C++, Dotnet assemblies are much easier to inspect as if they _were_ source code.  Compiling and then decompiling C# code will give you something very close to the original.  Also, dotnet assemblies can be platform-agnostic IL, which is JIT-compiled to the target platform on the fly.  So there are fewer downsides to "binary" distribution when it comes to C# as opposed to other languages.  Dotnet solved trust and compatibility problems in a different way than Go did.

When it comes to Go, the compiler is already a baseline dependency of Dagger itself.  When it comes to TypeScript and Python, the runtime and the SDK are one and the same.  There's not really any choice in the matter, any advantage to be had by pre-compiling in terms of disk size requirements (only in terms of cold start latency).

When it comes to dotnet, the SDK that is required to build the most potentially complex module (one that uses a `.sln` file), is approximately a half gigabyte download; meanwhile, running pre-built modules and even compiling simple ones requires a substantially smaller (less than 100MiB) runtime.  From the perspective of a C# module developer, this probably doesn't matter much, although it might be nice to have the ability to use only their host system's dotnet SDK and not store two of them.  Others, who use your dotnet module as a dependency, or directly as a CLI tool, are likely to appreciate it at least a little bit if your module's cold start is cut down from over 10 seconds to less than two, and requires over 400MiB less disk space.

So, when it comes to dotnet, due to the design of the framework itself, there are opportunities afforded by pre-compilation that are non-negligible and thus worth enabling.  Ultimately, it doesn't make sense to force this Go idiom on the Dotnet SDK.  Likewise, it's easy enough to support both paths, and doesn't make sense to _require_ that modules are pre-compiled (although it is heavily encouraged).

### Naming

Some naming has been done slightly differently in this SDK as compared to the others.

**Client vs Query**:  Most Dagger SDKs rename the "Query" object to "Client" during code generation.  I found this to be incongruent with the choice of naming the base class for all Dagger object representations "Base Client".  Indeed, every object that inherits this class is a client-side representation of a Dagger object, and an API client for Dagger.  Every distinct query is rooted in the Query object, so its _internal_ name of "Query" was actually perfectly appropriate and informative.  So, this SDK passes through the name "Query" unchanged.

**Context vs Session**:  Most Dagger SDKs use "Context" to describe the object that owns the GraphQL connection.  While not necessarily inappropriate, it's vague.  This is the client-side representation of what the server considers to be a "session".  It is bound to a session, and the word "session" is used mostly consistently throughout the Dagger software.  Why should this be called something slightly less descriptive from the client's perspective?  This SDK calls it "Session".
