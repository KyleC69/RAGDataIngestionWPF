// Build Date: 2026/03/24
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IContextCitationFormatter.cs
// Author: Kyle L. Crowder
// Build Num: 133536



using DataIngestionLib.Models;




namespace DataIngestionLib.Contracts.Services;





public interface IContextCitationFormatter
{
    string FormatSection(string heading, IReadOnlyList<ContextCitation> citations, int maxCharacters);
}