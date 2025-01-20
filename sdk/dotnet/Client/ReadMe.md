# Client

Implements parts of the Dagger C# Client that aren't generated.

## Remarks

Structuring this as a separate Assembly from the generated code, and distributing it as a NuGet package, makes a few things easier to maintain.  Developing this code directly is easier than embedding it in CodeGenerator.  Distributing it as a NuGet package means that client projects only need a single package reference inserted into their csproj; if it were injected into the user module in the same way as the generated assembly, then the user csproj would need to have all of _this_ csproj's package-references injected into it.  That gets ugly if this library ever drops dependencies - it's simpler to change the version of a known package reference. 
