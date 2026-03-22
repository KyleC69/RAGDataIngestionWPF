// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatBusyStateScopeFactory.cs
// Author: Kyle L. Crowder
// Build Num: 140815



using DataIngestionLib.Contracts.Services;




namespace DataIngestionLib.Services;





public sealed class ChatBusyStateScopeFactory : IChatBusyStateScopeFactory
{
    public IDisposable Enter(Action<bool> busyStateCallback)
    {
        ArgumentNullException.ThrowIfNull(busyStateCallback);
        return new BusyStateScope(busyStateCallback);
    }








    private sealed class BusyStateScope : IDisposable
    {
        private readonly Action<bool> _busyStateCallback;
        private bool _disposed;








        public BusyStateScope(Action<bool> busyStateCallback)
        {
            _busyStateCallback = busyStateCallback;
            _busyStateCallback(true);
        }








        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _busyStateCallback(false);
        }
    }
}