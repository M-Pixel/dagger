# Dagger Dotnet SDK Module

"Dagger Dotnet SDK", used casually, might either refer to the solution (the collection of projects that collectively are a "kit of software for developing dagger modules in dotnet"), the library (that you import and use as a dotnet module developer), or the SDK module.  From within Dagger Engine, an "SDK" is a _Dagger module_ that acts as the delivery mechanism and interface for SDK functionality.  This directory represents the SDK module for the Dagger Dotnet SDK.

An SDK module must itself use an SDK in order to be a module.  Only Golang is embedded in Dagger itself (all other SDKs ultimately rely on the Golang SDK), so it is conventional for SDK modules to be golang modules.

This module effectively just wraps the CodeGenerator and Thunk programs as Dagger Functions.

## To-do List

- Move to standalone repository or add Excludes to counteract the Go module's "required includes" which cause the hundreds of Dagger engine/cli/etc source files to be included in the module despite not being needed there (or wait for upstream to fix this issue).  See [discussions/9303](https://github.com/dagger/dagger/discussions/9303).
