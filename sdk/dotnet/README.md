# Dagger Dotnet SDK

Dagger SDK for Dotnet.  Not official.  Maintained by https://m-pixel.com.

## Notes on Design Decisions

When I began developing this SDK, I actually began by copy-pasting the TypeScript SDK's source code and changing the syntax as necessary to eliminate compile errors.  After all, there are a lot of similar design choices behind TypeScript and C# (some of the same people worked on both languages).  My goal was to keep it as close as possible, so that upstream changes to the TypeScript SDK could be most easily adapted to the C# one.  To the extent that there were any improvements to be made to the TypeScript SDK, I figured I would incrementally make them to both SDKs at once.

However, after a while, I decided to break from this goal.  The code generators doesn't actually change very often.  And at a certain point, I became familiar enough with Dagger's inner-workings that it was easy enough to go straight from a change in the design spec to knowing how that should change the implementation.  Meanwhile, the cruft inherited from the other SDKs was actually becoming a nuisance.

As of now, the Dotnet SDK tries to strike the best possible balance between Dagger conventions and Dotnet idioms, while naming and structuring everything intuitively, paying little regard for the design choices made in other Dagger SDKs.

### Naming

Some naming has been done slightly differently in this SDK as compared to the others.

**Client vs Query**:  Most Dagger SDKs rename the "Query" object to "Client" during code generation.  I found this to be incongruent with the choice of naming the base class for all Dagger object representations "Base Client".  Indeed, every object that inherits this class is a client-side representation of a Dagger object, and an API client for Dagger.  Every distinct query is rooted in the Query object, so its _internal_ name of "Query" was actually perfectly appropriate and informative.  So, this SDK passes through the name "Query" unchanged.

**Context vs Session**:  Most Dagger SDKs use "Context" to describe the object that owns the GraphQL connection.  While not necessarily inappropriate, it's vague.  This is the client-side representation of what the server considers to be a "session".  It is bound to a session, and the word "session" is used mostly consistently throughout the Dagger software.  Why should this be called something slightly less descriptive from the client's perspective?  This SDK calls it "Session".
