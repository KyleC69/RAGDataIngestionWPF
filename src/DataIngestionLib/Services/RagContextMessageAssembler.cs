using DataIngestionLib.Contracts;
using DataIngestionLib.Contracts.Services;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Services;




public sealed class RagContextMessageAssembler : IRagContextMessageAssembler
{
    private readonly IAppSettings _appSettings;




    public RagContextMessageAssembler(IAppSettings appSettings)
    {
        ArgumentNullException.ThrowIfNull(appSettings);
        _appSettings = appSettings;
    }




    public IReadOnlyList<ChatMessage> Assemble(IReadOnlyList<ChatMessage> requestMessages, IReadOnlyList<ChatMessage> candidateMessages)
    {
        ArgumentNullException.ThrowIfNull(requestMessages);
        ArgumentNullException.ThrowIfNull(candidateMessages);

        HashSet<string> seenTexts = new(StringComparer.OrdinalIgnoreCase);
        foreach (ChatMessage requestMessage in requestMessages)
        {
            string requestText = NormalizeText(requestMessage.Text);
            if (!string.IsNullOrWhiteSpace(requestText))
            {
                _ = seenTexts.Add(requestText);
            }
        }

        int maxCharacters = Math.Max(500, _appSettings.RAGBudget * 4);
        int currentCharacters = 0;
        List<ChatMessage> assembled = [];

        foreach (ChatMessage candidateMessage in candidateMessages)
        {
            string candidateText = candidateMessage.Text?.Trim() ?? string.Empty;
            string normalizedCandidateText = NormalizeText(candidateText);
            if (string.IsNullOrWhiteSpace(candidateText))
            {
                continue;
            }

            if (!seenTexts.Add(normalizedCandidateText))
            {
                continue;
            }

            if (assembled.Count > 0 && currentCharacters + candidateText.Length > maxCharacters)
            {
                break;
            }

            assembled.Add(new ChatMessage(candidateMessage.Role, candidateText));
            currentCharacters += candidateText.Length;
        }

        return assembled;
    }

    private static string NormalizeText(string? text)
    {
        return string.Join(
            " ",
            (text ?? string.Empty)
                .Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }
}