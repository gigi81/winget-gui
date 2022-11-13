namespace WingetGUI.Core.Contracts.Services;
public interface IDispatcherService
{
    bool TryEnqueue(Action action);
}
