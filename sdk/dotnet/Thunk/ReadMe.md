# Dagger Dotnet Thunk

> Thunks have been widely used to provide interoperability between software modules whose routines cannot call each other directly. This may occur because the routines have different calling conventions, run in different CPU modes or address spaces, or at least one runs in a virtual machine. A compiler (or other tool) can solve this problem by generating a thunk that automates the additional steps needed to call the target routine, whether that is transforming arguments, copying them to another location, or switching the CPU mode. A successful thunk minimizes the extra work the caller must do compared to a normal call.

 \- [Wikipedia authors](https://en.wikipedia.org/wiki/Thunk)

This program is responsible for translating a Dotnet [assembly](https://learn.microsoft.com/en-us/dotnet/standard/assembly/)'s compatible exports into callable Dagger Functions and constructible Dagger Objects, allowing a Dotnet `.dll` file to serve _as_ a Dagger Module.  It is written with the assumption that it will only ever be called via the SDK module (see `../module`), thus it assumes a particular filesystem layout, and relies on the Dagger engine being in a particular state to function correctly.

As per the Dagger Module specification, it either submits a description of the assembly, or invokes a specified function from that assembly (in its own process space), depending on the details of that contextual state.

## Regarding `dagger.json`

Thunk is not a Dagger module.  If it was, it would be a chicken-egg paradox.  The reason that there is a `dagger.json` file here is so that SDK developers can easily populate the project with a `Generated` client library, using `dagger develop -m Thunk`.

## Introspection

All functions, fields, and gettable properties that are both public and static are mapped to a no-parameter Dagger object named after the module.  If the assembly also contains a non-static type with the same name as the module, the Dagger object that they are mapped to will have a "Static" suffix.

All public functions, fields, and property accessors of all public types (class, struct, and record) exported by the Assembly will be mapped to Dagger objects.  Fields and gettable properties become Dagger Fields.  Methods and settable properties become Dagger Functions.  Nullable parameters, fields, and return values are recognized as optional in Dagger.

## Restrictions

- Dagger doesn't support overloads, because some of its supported languages (e.g. TypeScript) don't support overloads.  You cannot have more than two public methods or constructors on the same object with the same name but different parameters.
- Generic types are not supported, although this rule excludes special cases from the system library that have special meaning like Nullable and Task, as well as all collection types that are supported by `System.Text.Json`.
- Events are not supported, but will be ignored.
- Number types other than `int` and `float` are not supported.
- Multiple members of the same object, and multiple objects of the same assembly, cannot have names that become identical when converted to camelCase, snake_case, or any other case that is used by other module language SDKs.
- `ref`, `in`, and `out` parameters are not supported.

If any such exports utilize parameter or field types that are incompatible with Dagger, the program will crash with a relevant error message.  Such issues should be resolved by making the incompatible function/field/property `internal` or `private` instead of `public` (or by moving them to a dependency assembly).

## Object Serialization

Thunk uses `System.Text.Json` to (de)serialize values that are passed in and out of your module as parameters and return values.  This means that you can use attributes like `[JsonConverter]` to exert fine-grained control over how they are (de)serialized, and add support for (de)serializing objects that are not natively supported by `System.Text.Json` or Thunk.

Thunk adds support for `Task<>` as well.  Feel free to use `Task<>` parameters, fields, and return values.

Object constructors are used when creating a new instance of that object from another module.  However, if the object is returned, and then one of its methods are called by a dependent module in a subsequent DAG query, the method will be invoked on a recreation of the object that is deserialized directly, without using the constructor (unless you decorate the type with attributes that cause `JsonSerializer` to do otherwise).  Additionally, **even private fields will be (de)serialized** in this process.  Effectively, the state of the object is conveniently snapshotted and restored.  However, the sameness of the snapshot is dependent on the round-trip serializability of the members.  Thankfully, because private members are not _reflected_ by Dagger, even though they are retained by it, they are not subject to all of the same restrictions.  For example, you can have a `decimal` type member, or even a custom generic type, as long as they are `System.Text.Json` round-trip compatible.

If there are members that you don't want to include in the snapshot, you must use the `[JsonIgnore]` attribute.  There is an expectation that `DAG.MyObj().Foo()` has the same effect as `(await DAG.MyObj().Sync()).Foo()`, so this should only be done for fields that represent cached values, or system resources that are derivable from other fields that _are_ snapshottable.  For example, if there is an `HttpClient` member, it should be ignored, and either lazily reconstructed or restored upon deserialization.  The `IJsonOnDeserialized` and `IJsonOnDeserializing` interfaces can be useful for this purpose.  However, you might find it easier to stick with fully serializable `record` type objects for your `public` API, and construct `internal` objects as needed to employ ephemeral state.

Currently, there is no support for encoding references (TODO: add this - it's totally possible!).  If two public fields of `Foo` reference the same instance of `Bar`, if `Foo` is restored in a subsequent module invocation, its two `Bar` fields will reference identical but not-same instances.

> TODO: Check JSON attributes to see if fields need to be named differently.
