using MiniTestFramework;

namespace Tests;

public sealed class AppSharedContext : ISharedContext
{
    public DateTime CreatedAtUtc { get; private set; }
    public int Seed { get; private set; }
    public bool Disposed { get; private set; }

    public async Task InitializeAsync()
    {
        await Task.Delay(20);
        CreatedAtUtc = DateTime.UtcNow;
        Seed = 42;
    }

    public void Dispose()
    {
        Disposed = true;
    }
}
