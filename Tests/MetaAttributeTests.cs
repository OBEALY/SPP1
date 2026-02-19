using MiniTestFramework;

namespace Tests;

[TestClassInfo("bymer", "Class marker with properties demo")]
public sealed class MetaAttributeTests
{
    [Test]
    public void ClassAttribute_ShouldBeReadable()
    {
        var attr = typeof(MetaAttributeTests).GetCustomAttributes(typeof(TestClassInfoAttribute), false)
            .Cast<TestClassInfoAttribute>()
            .Single();

        AssertEx.Equal("bymer", attr.Owner);
        AssertEx.Contains("properties", attr.Description ?? string.Empty);
    }

    [TestCase("alpha", 1)]
    [TestCase("beta", 2)]
    [TestInfo("Method marker with properties demo", priority: 2)]
    public void MethodAttributes_WithParameters_ShouldExecute(string value, int expectedLenMin)
    {
        AssertEx.Greater(value.Length, expectedLenMin - 1);
    }
}
