namespace MiniTestFramework;

public interface ISharedContext : IDisposable
{
    Task InitializeAsync();
}
