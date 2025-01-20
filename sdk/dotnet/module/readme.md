# Dagger Dotnet SDK Module

"Dagger Dotnet SDK", used casually, might either refer to the solution (the collection of projects that collectively are a "kit of software for developing dagger modules in dotnet"), the library (that you import and use as a dotnet module developer), or the SDK module.  From within Dagger Engine, an "SDK" is a _Dagger module_ that acts as the delivery mechanism and interface for SDK functionality.  This directory represents the SDK module for the Dagger Dotnet SDK.

An SDK module must itself use an SDK in order to be a module.  Only Golang is embedded in Dagger itself (all other SDKs ultimately rely on the Golang SDK), so it is conventional for SDK modules to be golang modules.

This module effectively just wraps the CodeGenerator and Thunk programs as Dagger Functions.

## Pre-Compiled Assembly Distribution

As stated repeatedly throughout this project's ADRs, it is an important goal to be as lightweight as ~~possible~~ practical, especially for users of Dotnet modules who are not themselves Dotnet developers.  Thus, it should be capable of operating without pulling in the relatively large Dotnet SDK.  Without the SDK to compile its constituent components like CodeGenerator, Primer, and Thunk, they must be pre-compiled and distributed.

There are four sufficiently idiomatic options for distributing those builds: **Git**, **OCI**, **NuGet**, and **HTTPS**.

Various popular principles (something about kissing and razors) tell us that HTTPS, being the simplest option, is the best option.  All the other options use HTTPS — do they add anything of value for this scenario, to justify their added complexity?  The problem with HTTPS is that there is an asymmetry, and a lack of standardization, around submitting in particular: GitHub artifact API, vs S3 bucket upload, vs SSH transfer...  This makes it difficult to establish a simple pipeline that can be consistent across official, forks (potentially not on GitHub), and local testing.

Although Git is convenient because Dagger contains excellent Git integration, Git is (in theory) technically ill-suited for binary distribution (although it may be interesting to see how it fares against the other options in a real performance test).

NuGet is attractive as the de-facto standard for Dotnet assembly distribution.  It is already necessary to implement a lightweight NuGet client to download module dependencies.  Although, the NuGet client itself would need to be distributed somehow, so there is inevitably a need for two technologies.  It also uses zip compression, which is not the best choice nowadays, includes metadata unnecessary for this purpose, and conventionally puts all target platforms in a single archive.

At a technical level, OCI is more sophisticated, using better compression methods.  As opposed to implementing HTTPS directly, OCI clients like Dagger have already engineered in HTTPS optimizations like parallelism, and choice of various protocol options.  The only reason to not use OCI as a distribution mechanism here would be if doing `sdkRuntimeContainer.WithDirectory("/", distributionContainer.Directory("/"))` causes the payload to be cached twice (once as `distributionContainer`, once as `sdkRuntimeContainer.WithDirectory(...)`, potentially even once again as `distributionContainer.Directory("/")`).  But as long as the "copy" is exactly that, `/` to `/` with no additional parameters, [it should effectively create hard-links instead of copying the files](https://github.com/marcosnils/dagger/blob/ed2603df3b0ba400a4c8a6f55e128945c5a2c300/core/directory.go?plain=1#L585-L614).  Projects like Bazel and Ko have proven out this idea as a good choice by reputable software engineers.

Relying on pre-distributed binaries may seem counter to Dagger conventions, but the TypeScript SDK (for example) technically downloads Node JS (or Deno or Bun).  Yet there is an inconvenience to relying on pre-built binaries of _previous versions_ of a software to develop later versions (to an extent - most programming languages eventually cross that line, and Dagger itself has, too).  If you make a change to Thunk (for example), how do you verify that change without pushing it up to an artifact server?  Dagger is all about avoiding "push & pray", so a solution for running this module against not-yet-distributed assemblies is required.  That solution exists in the form of the adjacent bootstrap module, along with a small bit of dependency injection logic in this module.  See [bootstrap's readme](../bootstrap/readme.md) for the implementation details of that solution.

## To-do List

- Move to standalone repository or add Excludes to counteract the Go module's "required includes" which cause the hundreds of Dagger engine/cli/etc source files to be included in the module despite not being needed there (or wait for upstream to fix this issue).  See [discussions/9303](https://github.com/dagger/dagger/discussions/9303).
- Warn or error if client library reference version doesn't match engine version.  And figure out how to deal with engine versions generally.  And figure out why codegen is run but not applied in the course of invocation.
