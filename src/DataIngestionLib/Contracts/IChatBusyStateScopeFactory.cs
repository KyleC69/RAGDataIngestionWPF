namespace DataIngestionLib.Contracts.Services;




public interface IChatBusyStateScopeFactory
{
    IDisposable Enter(Action<bool> busyStateCallback);
}