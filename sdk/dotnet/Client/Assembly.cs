using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Dagger.IntegrationTests")]

// A fixed assembly name is used for all generated assemblies, so that it's possible to allow them to access internal
// members of this assembly.  If each module's generated assembly had a unique name, a different approach would be
// necessary for giving the generated assembly access to this assembly's internal members.
//
// That creates a small problem, though: A conflict between the Generated library that the Thunk needs to load for
// itself in order to receive the function parameters from and send the return value to Dagger, and the Generated
// library that the module assembly loaded into the Thunk's runtime uses.  There are a few ways to solve that problem:
//
// 1. Don't actually use a generated client in the Thunk.  Implement the few Dagger calls that it needs to make directly
//    in the Thunk's implementation as raw GraphQL queries.  This is undesirable because maintainability.
// 2. Don't split Client from Generated.  Honestly, this isn't terrible option.  But even if the optimizations that the
//    split enables are minor, prohibiting their possibility and throwing away work I've already done to make it work is
//    sufficiently unappealing.
// 3. Patch Client so that a module-specific Generated assembly name is InternalsVisibleTo'd.  Unfortunately, this
//    requires that patched version of Dagger.Client to be retained, and to have its *own* unique assembly name.  That
//    eliminates a number of the benefits of having the common portion of the Client split out, and patching Client to
//    have a different assembly name sounds like a potential "rabbit-hole".  I tried using an ephemeral patched Client
//    in the Generated compilation process only, hoping that internal visibility adherence is only tested at
//    compile-time, but it turns out that it's checked at load-time, too.
// 4. Use a custom Assembly Load Context that scopes the module's version of Dagger.Generated.  This is extremely simple
//    and works perfectly (Dotnet framework has great support for loading multiple versions of the same assembly in the
//    same process as long as they're in different load contexts).
[assembly: InternalsVisibleTo("Dagger.Generated")]

[assembly: InternalsVisibleTo("Dagger.Thunk")]
