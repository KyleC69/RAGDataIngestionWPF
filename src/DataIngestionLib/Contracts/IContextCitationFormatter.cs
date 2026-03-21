using DataIngestionLib.Models;

namespace DataIngestionLib.Contracts.Services;

public interface IContextCitationFormatter
{
    string FormatSection(string heading, IReadOnlyList<ContextCitation> citations, int maxCharacters);
}