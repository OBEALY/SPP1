using MiniTestFramework;

var runner = new TestRunner();
var report = await runner.RunAsync(typeof(Program).Assembly);

Console.WriteLine(report.ToConsoleText());
var reportPath = Path.Combine(AppContext.BaseDirectory, "TestResults.txt");
await File.WriteAllTextAsync(reportPath, report.ToFileText());
Console.WriteLine($"Report file: {reportPath}");
