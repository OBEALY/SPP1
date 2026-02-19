namespace TestedProject;

public sealed class TextService
{
    public string Join(string left, string right) => $"{left} {right}";

    public string? MaybeNull(bool shouldBeNull) => shouldBeNull ? null : "data";

    public IReadOnlyList<int> Range(int start, int count)
    {
        return Enumerable.Range(start, count).ToArray();
    }
}
