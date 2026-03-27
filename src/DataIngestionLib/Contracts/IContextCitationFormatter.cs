// Build Date: 2026/03/27
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IContextCitationFormatter.cs
// Author: Kyle L. Crowder
// Build Num: 072938



using DataIngestionLib.Models;




namespace DataIngestionLib.Contracts.Services;





public interface IContextCitationFormatter
{
    string FormatSection(string heading, IReadOnlyList<ContextCitation> citations, int maxCharacters);
}