# Dagger Dotnet Thunk

> Thunks have been widely used to provide interoperability between software modules whose routines cannot call each other directly. This may occur because the routines have different calling conventions, run in different CPU modes or address spaces, or at least one runs in a virtual machine. A compiler (or other tool) can solve this problem by generating a thunk that automates the additional steps needed to call the target routine, whether that is transforming arguments, copying them to another location, or switching the CPU mode. A successful thunk minimizes the extra work the caller must do compared to a normal call.

 \- [Wikipedia authors](https://en.wikipedia.org/wiki/Thunk)

This program is responsible for translating a Dotnet [assembly](https://learn.microsoft.com/en-us/dotnet/standard/assembly/)'s compatible exports into callable Dagger Functions and constructible Dagger Objects, allowing a Dotnet `.dll` file to serve _as_ a Dagger Module.  It is written with the assumption that it will only ever be called via the SDK module (see `../module`), thus it assumes a particular filesystem layout, and relies on the Dagger engine being in a particular state to function correctly.  It either submits a description of the assembly, or invokes a specified function from that assembly (in its own process space), depending on the details of that contextual state.

All functions, fields, and gettable properties that are both public and static are mapped to a no-parameter Dagger object named after the module.  If the assembly also contains a non-static type with the same name as the module, the Dagger object that they are mapped to will have a "Factory" suffix.

All public functions, fields, and property accessors of all public types exported by the Assembly will be mapped to Dagger objects.  If any such exports utilize parameter or field types that are incompatible with Dagger, the program will crash with a relevant error message.  Such issues should be resolved by moving the incompatible function/field/property to a dependency library, or by making them `internal` or `private` instead of `public`.
