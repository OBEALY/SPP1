namespace MiniTestFramework;

[AttributeUsage(AttributeTargets.Class)]
public sealed class TestClassAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class TestClassInfoAttribute(string owner, string? description = null) : Attribute
{
    public string Owner { get; } = owner;
    public string? Description { get; } = description;
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class UseSharedContextAttribute(Type contextType) : Attribute
{
    public Type ContextType { get; } = contextType;
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class TestAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class TestCaseAttribute(params object?[] args) : Attribute
{
    public object?[] Args { get; } = args;
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class TestInfoAttribute(string title, int priority = 0) : Attribute
{
    public string Title { get; } = title;
    public int Priority { get; } = priority;
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class BeforeAllAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class AfterAllAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class BeforeEachAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class AfterEachAttribute : Attribute
{
}
