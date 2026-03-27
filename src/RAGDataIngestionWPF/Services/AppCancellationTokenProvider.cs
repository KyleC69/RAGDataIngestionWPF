// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         AppCancellationTokenProvider.cs
// Author: Kyle L. Crowder
// Build Num: 073032



#nullable enable

using Microsoft.Extensions.Logging;

using RAGDataIngestionWPF.Contracts.Services;




namespace RAGDataIngestionWPF.Services;





/// <summary>
///     Manages application-wide cancellation token lifecycle.
///     Supports nested/linked cancellation scopes for page, operation, and sub-task granularity.
/// </summary>
internal sealed class AppCancellationTokenProvider : IAppCancellationTokenProvider, IDisposable
{
    private readonly object _lock = new object();
    private readonly ILogger<AppCancellationTokenProvider> _logger;
    private CancellationTokenSource _cts;
    private bool _disposed;








    public AppCancellationTokenProvider(ILogger<AppCancellationTokenProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cts = new CancellationTokenSource();
        _logger.LogInformation("AppCancellationTokenProvider initialized.");
    }








    public CancellationToken Token
    {
        get
        {
            lock (_lock)
            {
                ThrowIfDisposed();
                return _cts.Token;
            }
        }
    }








    public LinkedCancellationTokenScope CreateLinkedScope()
    {
        lock (_lock)
        {
            ThrowIfDisposed();
            return new LinkedScope(_cts.Token, _logger);
        }
    }








    public void CancelAll()
    {
        lock (_lock)
        {
            if (_disposed)
            {
                _logger.LogWarning("CancelAll called after disposal. Ignoring.");
                return;
            }

            if (!_cts.IsCancellationRequested)
            {
                _logger.LogInformation("Cancelling all app operations via CancelAll().");
                _cts.Cancel();
            }
        }
    }








    public void Reset()
    {
        lock (_lock)
        {
            ThrowIfDisposed();

            if (_cts.IsCancellationRequested)
            {
                _logger.LogInformation("Resetting app cancellation token.");
                _cts.Dispose();
                _cts = new CancellationTokenSource();
            }
        }
    }








    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _logger.LogInformation("Disposing AppCancellationTokenProvider.");

            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }

            _cts.Dispose();
            _disposed = true;
        }
    }








    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(AppCancellationTokenProvider), "AppCancellationTokenProvider has been disposed.");
        }
    }








    /// <summary>
    ///     Internal implementation of LinkedCancellationTokenScope.
    ///     Allows per-operation cancellation while respecting app-level cancellation.
    /// </summary>
    private sealed class LinkedScope : LinkedCancellationTokenScope
    {
        private readonly CancellationToken _appToken;
        private readonly ILogger _logger;
        private bool _disposed;
        private CancellationTokenSource? _linkedCts;








        public LinkedScope(CancellationToken appToken, ILogger logger)
        {
            _appToken = appToken;
            _logger = logger;
            _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(appToken);
        }








        public CancellationToken Token
        {
            get
            {
                ThrowIfDisposed();
                return _linkedCts!.Token;
            }
        }








        public void Cancel()
        {
            if (_linkedCts != null && !_linkedCts.IsCancellationRequested)
            {
                _logger.LogInformation("Cancelling linked scope operation.");
                _linkedCts.Cancel();
            }
        }








        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _logger.LogDebug("Disposing LinkedCancellationTokenScope.");

            if (_linkedCts != null && !_linkedCts.IsCancellationRequested)
            {
                _linkedCts.Cancel();
            }

            _linkedCts?.Dispose();
            _linkedCts = null;
            _disposed = true;
        }








        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(LinkedScope));
            }
        }
    }
}