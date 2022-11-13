using Microsoft.UI.Dispatching;
using WingetGUI.Core.Contracts.Services;

namespace WingetGUI.Services;
public class DispatcherService : IDispatcherService
{
    internal static IDispatcherService FromCurrentThread() => new DispatcherService(DispatcherQueue.GetForCurrentThread());

    private readonly DispatcherQueue _queue;

    public DispatcherService(DispatcherQueue queue)
    {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
    }

    public bool TryEnqueue(Action action)
    {
        return _queue.TryEnqueue(() => action?.Invoke());
    }
}
