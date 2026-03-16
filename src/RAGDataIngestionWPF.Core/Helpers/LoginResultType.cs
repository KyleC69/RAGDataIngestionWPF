// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Core
// File:         LoginResultType.cs
// Author: Kyle L. Crowder
// Build Num: 182436



namespace RAGDataIngestionWPF.Core.Helpers;





public enum LoginResultType
{
    Success,
    Unauthorized,
    CancelledByUser,
    NoNetworkAvailable,
    UnknownError
}