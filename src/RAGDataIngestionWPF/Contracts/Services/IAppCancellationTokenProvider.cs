// Build Date: 2026/03/25
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:      IAppCancellationTokenProvider.cs
// Purpose:   Centralized cancellation token provider linked to application lifecycle

#nullable enable

namespace RAGDataIngestionWPF.Contracts.Services;

/// <summary>
/// Provides application-wide cancellation token management linked to app lifecycle.
/// Cancellation is triggered on:
/// - Normal app shutdown (OnExit)
/// - Unhandled exceptions / app crashes (OnDispatcherUnhandledException)
/// - User navigation away from pages
/// - Explicit cancellation button clicks
/// </summary>
public interface IAppCancellationTokenProvider
{
    /// <summary>
    /// Gets the current application-wide cancellation token. 
    /// This token is cancelled when the app shuts down, crashes, or user explicitly cancels operations.
    /// Create linked tokens for scoped cancellation (e.g., per-page or per-operation).
    /// </summary>
    CancellationToken Token { get; }

    /// <summary>
    /// Creates a linked cancellation token scope for page or operation-specific cancellation.
    /// The returned token will be cancelled when either:
    /// - The scope is disposed (normal lifecycle)
    /// - The app-wide token is cancelled (app-level interrupt)
    /// </summary>
    LinkedCancellationTokenScope CreateLinkedScope();

    /// <summary>
    /// Cancels all operations linked to this provider.
    /// Called automatically on app shutdown or crash.
    /// Safe to call multiple times.
    /// </summary>
    void CancelAll();

    /// <summary>
    /// Cancels and restarts the token for fresh operations.
    /// Used when returning to a page or restarting an operation.
    /// </summary>
    void Reset();
}

/// <summary>
/// A linked cancellation scope that allows cancelling sub-operations independently
/// while still respecting app-level cancellation.
/// </summary>
public interface LinkedCancellationTokenScope : IDisposable
{
    /// <summary>
    /// Gets the linked cancellation token scoped to this operation.
    /// </summary>
    CancellationToken Token { get; }

    /// <summary>
    /// Cancels this scope's specific operations.
    /// </summary>
    void Cancel();
}
