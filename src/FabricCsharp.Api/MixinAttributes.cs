namespace FabricCsharp.Api;

// Marks a class as a Mixin targeting a specific Minecraft class
[AttributeUsage(AttributeTargets.Class)]
public class MixinAttribute : Attribute
{
    public required Type Target { get; init; }
    public int Priority { get; init; } = 1000;
    public bool Remap { get; init; } = true;
}

// Injection point type — where to inject code
public enum AtType
{
    Head,           // @At("HEAD") — beginning of method
    Tail,           // @At("TAIL") — end of method (before return)
    Return,         // @At("RETURN") — before each return
    Invoke,         // @At("INVOKE") — at a method call
    Field,          // @At("FIELD") — at a field access
    New,            // @At("NEW") — at object creation
    Constant,       // @At("CONSTANT") — at a constant load
    Load,           // @At("LOAD") — at a local variable load
    Store           // @At("STORE") — at a local variable store
}

// Marks a method as an injection point
[AttributeUsage(AttributeTargets.Method)]
public class InjectAttribute : Attribute
{
    public required string Method { get; init; }       // Target method name
    public required AtType At { get; init; }            // Where to inject
    public string[]? Args { get; init; }                // Method descriptor args
    public int Ordinal { get; init; } = -1;             // Which occurrence
    public bool Cancellable { get; init; }              // Whether to make cancellable
}

// Overwrites/replaces the target method entirely
[AttributeUsage(AttributeTargets.Method)]
public class OverwriteAttribute : Attribute
{
    public string? Method { get; init; }  // null = use C# method name
}

// Creates an accessor for a private field
[AttributeUsage(AttributeTargets.Method)]
public class AccessorAttribute : Attribute
{
    public required string Field { get; init; }   // Field name to access
}

// Creates an invoker for a private method
[AttributeUsage(AttributeTargets.Method)]
public class InvokerAttribute : Attribute
{
    public required string Method { get; init; }  // Method to invoke
}

// Modifies a method argument value
[AttributeUsage(AttributeTargets.Method)]
public class ModifyArgAttribute : Attribute
{
    public required string Method { get; init; }
    public required int Index { get; init; }      // Which argument to modify
}

// Modifies a variable value
[AttributeUsage(AttributeTargets.Method)]
public class ModifyVariableAttribute : Attribute
{
    public required string Method { get; init; }
    public required AtType At { get; init; }
    public int Ordinal { get; init; } = -1;
}

// Redirects a method call to another method
[AttributeUsage(AttributeTargets.Method)]
public class RedirectAttribute : Attribute
{
    public required string Method { get; init; }
    public required AtType At { get; init; }
    public string[]? Args { get; init; }
}

// Stub types for Mixin callback types
public abstract class CallbackInfo { }
public abstract class CallbackInfoReturnable<T> : CallbackInfo { }
