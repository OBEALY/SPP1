namespace MiniTestFramework;

public enum TestStatus
{
    Passed,
    Failed,
    Error
}

public sealed class TestCaseResult
{
    public required string ClassName { get; init; }
    public required string MethodName { get; init; }
    public required string DisplayName { get; init; }
    public required TestStatus Status { get; init; }
    public string? Message { get; init; }
    public TimeSpan Duration { get; init; }
}

public sealed class TestRunReport
{
    public required IReadOnlyList<TestCaseResult> Results { get; init; }

    public int Total => Results.Count;
    public int Passed => Results.Count(r => r.Status == TestStatus.Passed);
    public int Failed => Results.Count(r => r.Status == TestStatus.Failed);
    public int Errored => Results.Count(r => r.Status == TestStatus.Error);

    public string ToConsoleText()
    {
        var lines = new List<string>
        {
            "=== TEST RUN REPORT ===",
            $"Total: {Total}, Passed: {Passed}, Failed: {Failed}, Error: {Errored}"
        };

        foreach (var result in Results)
        {
            lines.Add($"{result.Status,-6} | {result.DisplayName} | {result.Duration.TotalMilliseconds:F1} ms");
            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                lines.Add($"         {result.Message}");
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    public string ToFileText() => ToConsoleText();
}
