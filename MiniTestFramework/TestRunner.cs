using System.Diagnostics;
using System.Reflection;

namespace MiniTestFramework;

public sealed class TestRunner
{
    public async Task<TestRunReport> RunAsync(Assembly assembly)
    {
        var results = new List<TestCaseResult>();
        var testTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(IsTestClass)
            .ToArray();

        foreach (var testType in testTypes)
        {
            await RunTestClassAsync(testType, results);
        }

        return new TestRunReport
        {
            Results = results
        };
    }

    private static bool IsTestClass(Type type)
    {
        return type.GetCustomAttribute<TestClassAttribute>() is not null
               || type.GetCustomAttribute<TestClassInfoAttribute>() is not null;
    }

    private static async Task RunTestClassAsync(Type testType, List<TestCaseResult> results)
    {
        var sharedContext = await TryCreateSharedContextAsync(testType);

        try
        {
            var instance = Activator.CreateInstance(testType)
                           ?? throw new TestDiscoveryException($"Cannot create an instance of {testType.Name}.");
            InjectSharedContextIfNeeded(instance, sharedContext);

            var beforeAll = FindLifecycleMethods(testType, typeof(BeforeAllAttribute));
            var afterAll = FindLifecycleMethods(testType, typeof(AfterAllAttribute));
            var beforeEach = FindLifecycleMethods(testType, typeof(BeforeEachAttribute));
            var afterEach = FindLifecycleMethods(testType, typeof(AfterEachAttribute));

            try
            {
                await InvokeLifecycleAsync(instance, beforeAll, "BeforeAll");
            }
            catch (Exception ex)
            {
                AddLifecycleError(testType, "BeforeAll", ex, results);
                return;
            }

            var testEntries = FindTests(testType);

            foreach (var entry in testEntries)
            {
                await RunSingleTestAsync(instance, entry, beforeEach, afterEach, results);
            }

            try
            {
                await InvokeLifecycleAsync(instance, afterAll, "AfterAll");
            }
            catch (Exception ex)
            {
                AddLifecycleError(testType, "AfterAll", ex, results);
            }
        }
        finally
        {
            sharedContext?.Dispose();
        }
    }

    private static async Task<ISharedContext?> TryCreateSharedContextAsync(Type testType)
    {
        var useShared = testType.GetCustomAttribute<UseSharedContextAttribute>();
        if (useShared is null)
        {
            return null;
        }

        if (!typeof(ISharedContext).IsAssignableFrom(useShared.ContextType))
        {
            throw new TestDiscoveryException($"{useShared.ContextType.Name} does not implement ISharedContext.");
        }

        var context = Activator.CreateInstance(useShared.ContextType) as ISharedContext
                      ?? throw new TestDiscoveryException($"Cannot create shared context: {useShared.ContextType.Name}.");

        await context.InitializeAsync();
        return context;
    }

    private static void InjectSharedContextIfNeeded(object instance, ISharedContext? sharedContext)
    {
        if (sharedContext is null)
        {
            return;
        }

        var prop = instance.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(p => p.CanWrite && p.PropertyType.IsAssignableFrom(sharedContext.GetType()));
        prop?.SetValue(instance, sharedContext);
    }

    private static List<MethodInfo> FindLifecycleMethods(Type testType, Type attrType)
    {
        return testType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttributes(attrType, false).Any())
            .ToList();
    }

    private static List<(MethodInfo Method, object?[] Args, string DisplayName)> FindTests(Type testType)
    {
        var methods = testType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var entries = new List<(MethodInfo, object?[], string)>();

        foreach (var method in methods)
        {
            var hasTestAttribute = method.GetCustomAttribute<TestAttribute>() is not null;
            var cases = method.GetCustomAttributes<TestCaseAttribute>().ToArray();
            if (!hasTestAttribute && cases.Length == 0)
            {
                continue;
            }

            if (cases.Length == 0)
            {
                entries.Add((method, Array.Empty<object?>(), $"{testType.Name}.{method.Name}"));
                continue;
            }

            for (var i = 0; i < cases.Length; i++)
            {
                entries.Add((method, cases[i].Args, $"{testType.Name}.{method.Name}[{i}]"));
            }
        }

        return entries;
    }

    private static async Task RunSingleTestAsync(
        object instance,
        (MethodInfo Method, object?[] Args, string DisplayName) entry,
        List<MethodInfo> beforeEach,
        List<MethodInfo> afterEach,
        List<TestCaseResult> results)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await InvokeLifecycleAsync(instance, beforeEach, "BeforeEach");
        }
        catch (Exception ex)
        {
            results.Add(new TestCaseResult
            {
                ClassName = instance.GetType().Name,
                MethodName = entry.Method.Name,
                DisplayName = entry.DisplayName,
                Status = TestStatus.Error,
                Message = ex.InnerException?.Message ?? ex.Message,
                Duration = sw.Elapsed
            });
            return;
        }

        try
        {
            await InvokeMethodAsync(instance, entry.Method, entry.Args);
            results.Add(new TestCaseResult
            {
                ClassName = instance.GetType().Name,
                MethodName = entry.Method.Name,
                DisplayName = entry.DisplayName,
                Status = TestStatus.Passed,
                Duration = sw.Elapsed
            });
        }
        catch (AssertionFailedException ex)
        {
            results.Add(new TestCaseResult
            {
                ClassName = instance.GetType().Name,
                MethodName = entry.Method.Name,
                DisplayName = entry.DisplayName,
                Status = TestStatus.Failed,
                Message = ex.Message,
                Duration = sw.Elapsed
            });
        }
        catch (Exception ex)
        {
            results.Add(new TestCaseResult
            {
                ClassName = instance.GetType().Name,
                MethodName = entry.Method.Name,
                DisplayName = entry.DisplayName,
                Status = TestStatus.Error,
                Message = ex.InnerException?.Message ?? ex.Message,
                Duration = sw.Elapsed
            });
        }
        finally
        {
            try
            {
                await InvokeLifecycleAsync(instance, afterEach, "AfterEach");
            }
            catch (Exception ex)
            {
                results.Add(new TestCaseResult
                {
                    ClassName = instance.GetType().Name,
                    MethodName = entry.Method.Name,
                    DisplayName = $"{entry.DisplayName}::AfterEach",
                    Status = TestStatus.Error,
                    Message = ex.InnerException?.Message ?? ex.Message,
                    Duration = sw.Elapsed
                });
            }
        }
    }

    private static async Task InvokeLifecycleAsync(object instance, List<MethodInfo> methods, string stageName)
    {
        foreach (var method in methods)
        {
            try
            {
                await InvokeMethodAsync(instance, method, Array.Empty<object?>());
            }
            catch (Exception ex)
            {
                throw new TestExecutionException($"Error in {stageName}: {method.Name}", ex);
            }
        }
    }

    private static async Task InvokeMethodAsync(object instance, MethodInfo method, object?[] args)
    {
        ValidateMethodParameters(method, args);
        object? result;
        try
        {
            result = method.Invoke(instance, args);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw ex.InnerException;
        }

        switch (result)
        {
            case Task task:
                await task;
                break;
            case ValueTask valueTask:
                await valueTask;
                break;
        }
    }

    private static void ValidateMethodParameters(MethodInfo method, object?[] args)
    {
        var parameters = method.GetParameters();
        if (parameters.Length != args.Length)
        {
            throw new TestDiscoveryException($"Method {method.Name} expects {parameters.Length} args but got {args.Length}.");
        }

        for (var i = 0; i < parameters.Length; i++)
        {
            var arg = args[i];
            if (arg is null)
            {
                continue;
            }

            if (!parameters[i].ParameterType.IsAssignableFrom(arg.GetType()))
            {
                throw new TestDiscoveryException(
                    $"Argument {i} for {method.Name} has type {arg.GetType().Name}, expected {parameters[i].ParameterType.Name}.");
            }
        }
    }

    private static void AddLifecycleError(Type testType, string stage, Exception ex, List<TestCaseResult> results)
    {
        results.Add(new TestCaseResult
        {
            ClassName = testType.Name,
            MethodName = stage,
            DisplayName = $"{testType.Name}::{stage}",
            Status = TestStatus.Error,
            Message = ex.InnerException?.Message ?? ex.Message,
            Duration = TimeSpan.Zero
        });
    }
}
