// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         AIModels.cs
// Author: Kyle L. Crowder
// Build Num: 202403



namespace DataIngestionLib.Models;





public record AIModels
    {
    /// <summary>A locally-hosted GPT-family open-source model served through Ollama.</summary>
    public const string GPTOSS = "gpt-oss:20b-cloud";

    /// <summary>OpenAI GPT-4 cloud model identifier.</summary>
    public const string GPT4 = "gpt-4";

    /// <summary>Meta Llama 3.2 1-billion parameter variant served through Ollama.</summary>
    public const string Llama1B = "llama3.2:1b";

    /// <summary>Meta Llama 3.2 3-billion parameter variant served through Ollama.</summary>
    public const string Llama323B = "llama3.2:3b";

    /// <summary>MixedBread AI large embedding model (<c>mxbai-embed-large</c>) served through Ollama.</summary>
    public const string MXBAI = "mxbai-embed-large:latest";
    }