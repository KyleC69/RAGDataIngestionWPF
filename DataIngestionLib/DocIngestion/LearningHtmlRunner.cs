// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         LearningHtmlRunner.cs
// Author: Kyle L. Crowder
// Build Num: 175052



using System.Net.Http;

using DataIngestionLib.Models;
using DataIngestionLib.Providers;

using HtmlAgilityPack;

using Microsoft.Agents.AI;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlTypes;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using OllamaSharp;
using OllamaSharp.Models;




namespace DataIngestionLib.DocIngestion;





public sealed class LearningHtmlRunner
{
    private readonly IDocRepository _docRepository;
    private readonly IHeadlessBrowser _headlessBrowser;
    private readonly ILogger<LearningHtmlRunner> _logger;
    private readonly OllamaApiClient _ollamaApiClient;
    //private readonly IngestionSettings _settings;








    /// <summary>
    ///     Initializes a new instance of <see cref="LearningHtmlRunner" /> with all required dependencies.
    /// </summary>
    /// <param name="headlessBrowser">Headless browser used for remote page retrieval.</param>
    /// <param name="docRepository">Repository for persisting ingested documentation.</param>
    /// <param name="logger">Structured logger provided by the DI container.</param>
    /// <param name="settings">Ingestion configuration options.</param>
    /// <param name="ollamaApiClient">Ollama API client used for LLM-assisted extraction.</param>
    public LearningHtmlRunner(IHeadlessBrowser headlessBrowser, IDocRepository docRepository, ILogger<LearningHtmlRunner> logger, OllamaApiClient ollamaApiClient)
    {
        _headlessBrowser = headlessBrowser ?? throw new ArgumentNullException(nameof(headlessBrowser));
        _docRepository = docRepository ?? throw new ArgumentNullException(nameof(docRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        //    _settings = (settings ?? throw new ArgumentNullException(nameof(settings))).Value ?? throw new ArgumentNullException(nameof(settings));
        _ollamaApiClient = ollamaApiClient ?? throw new ArgumentNullException(nameof(ollamaApiClient));
    }








    private static List<(HtmlNode Heading, HtmlNode Paragraph)> CombineSuccessiveParagraphs(HtmlNodeCollection content)
    {
        if (content == null || content.Count == 0)
        {
            return [];
        }

        List<(HtmlNode Heading, HtmlNode Paragraph)> results = [];
        HtmlNode? currentHeading = null;
        List<string> paragraphFragments = [];

        foreach (HtmlNode node in content)
        {
            if (node == null)
            {
                continue;
            }

            if (IsHeading(node))
            {
                AddCurrentPair();
                currentHeading = node;
                paragraphFragments.Clear();
                continue;
            }

            if (node.Name == "p" && currentHeading != null)
            {
                var fragment = node.InnerHtml?.Trim();
                if (!string.IsNullOrEmpty(fragment))
                {
                    paragraphFragments.Add(fragment);
                }
            }
        }

        AddCurrentPair();
        return results;



        bool IsHeading(HtmlNode node)
        {
            return node.Name.StartsWith("h", StringComparison.OrdinalIgnoreCase) && node.Name.Length == 2 && char.IsDigit(node.Name[1]);
        }



        void AddCurrentPair()
        {
            if (currentHeading == null || paragraphFragments.Count == 0)
            {
                return;
            }

            HtmlDocument owner = currentHeading.OwnerDocument ?? new HtmlDocument();
            HtmlNode paragraphNode = owner.CreateElement("p");
            paragraphNode.InnerHtml = string.Join(" ", paragraphFragments);
            results.Add((currentHeading, paragraphNode));
        }
    }








    public async Task EmbedRemoteKnowledgeSource(ISettingsProvider settings, CancellationToken cancellationToken = default)
    {

        settings.AddUpdateAppSettings("LearnBaseUrl", "https://learn.microsoft.com/en-us/agent-framework/");
        var startingUrl = settings.ReadSetting("LearnBaseUrl");
        var crawlList = await TocHrefExtractor.GetTocList().ConfigureAwait(false);

        SqlConnection conn = new(Environment.GetEnvironmentVariable("CONN_STRING"));
        //add rag to database
        var select = @"SELECT og_url from RemoteRAG ";
        List<string> existingUrls = [];
        using (SqlCommand cmd = new(select, conn))
        {
            conn.Open();
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    existingUrls.Add(reader["og_url"]?.ToString() ?? string.Empty);
                }
            }
        }

        // Filter out URLs that already exist in the database
        crawlList = crawlList.Where(url => !existingUrls.Contains(startingUrl + url)).ToList();


        foreach (var pg in crawlList)
        {
            var page = await _headlessBrowser.GetPageSourceAsync(startingUrl + pg, cancellationToken).ConfigureAwait(false);
            var normalized = page.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("\t", "");

            HtmlDocument htmlDoc = new();
            htmlDoc.LoadHtml(normalized);

            HtmlNode mainContentNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@data-main-column]");
            var title = htmlDoc.DocumentNode.SelectSingleNode("//title")?.InnerText.Trim();
            var description = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='description']")?.GetAttributeValue("content", "") ?? throw new InvalidOperationException("Description meta tag not found");
            var document_id = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='document_id']")?.GetAttributeValue("content", "") ?? throw new InvalidOperationException("Document ID meta tag not found");
            var updated_at = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='updated_at']")?.GetAttributeValue("content", "") ?? throw new InvalidOperationException("Updated At meta tag not found");
            var ms_date = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='ms.date']")?.GetAttributeValue("content", "") ?? throw new InvalidOperationException("MS Date meta tag not found");
            var og_url = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:url']")?.GetAttributeValue("content", "") ?? throw new InvalidOperationException("OG URL meta tag not found");


            AgentResponse? summary = await SummarizeContent(mainContentNode.InnerText);
            AgentResponse? keywords = await GenerateKeywords(mainContentNode.InnerText);
            // EmbedResponse vectory = await GenerateEmbedding(summary.Text);
            //   SqlVector<float> embedding = new(vectory.Embeddings[0]); // 
            RemoteRAG rag = new()
            {
                // Identity is autogenerated
                Title = title,
                Description = description,
                OgUrl = og_url,
                DocumentId = document_id,
                UpdatedAt = DateTime.TryParse(updated_at, out DateTime ut) ? ut : DateTime.MinValue,
                MSDate = DateTime.TryParse(ms_date, out DateTime md) ? md : DateTime.MinValue,
                Summary = summary?.Text ?? "ModelErr", // Assuming summary has a Content property
                Keywords = keywords?.Text ?? "ModelErr", // Assuming keywords has a Content property
                                                        //      embedding = embedding, // Placeholder, needs to be generated
                                                        //      token_count = vectory.Embeddings.Count, // Placeholder, needs to be calculated
                Version = 1 // Placeholder, will be managed
            };


            SaveSqlRemoteKnowledgeSource(rag);
            // For now, just log the gathered metadata.
            _logger.LogInformation("Page metaTitle='{Title}', Description='{Description}', DocumentId='{DocumentId}', UpdatedAt='{UpdatedAt}', MsDate='{MsDate}', OgUrl='{OgUrl}', Summary='{Summary}'", rag.Title, rag.Description, rag.DocumentId, rag.UpdatedAt, rag.MSDate, rag.OgUrl, rag.Summary);

        }

    }








    /*


    private Task<EmbedResponse> GenerateEmbedding(string summary)
    {
        OllamaApiClient embed = new("http://localhost:11434", AIModels.MXBAI);
        EmbedRequest req = new()
        {
            Model = AIModels.MXBAI,
            Input = [summary],
            Options = new RequestOptions
            {

                NumCtx = 125000,
                NumThread = 4,
                Temperature = 0.6f


            }
        };
        var response = embed.EmbedAsync(req);
        return response;

    }



    */








    private async Task<AgentResponse?> GenerateKeywords(string page)
    {
        var ins = """
                                      You are a keyword extraction assistant. Your task is to read the provided content from a documentation page and extract a comma-separated list of relevant keywords.
                  Focus on technical terms, concepts, and entities mentioned in the text. The keywords should be concise and accurately represent the main topics discussed.
                  Respond ONLY with the comma-separated keywords, with no additional text or formatting. They should hold great semantic relevance to the content and be useful for indexing and search purposes.
                  You must respond with no less than 5 keywords and no more than 8. 
                  """;


        try
        {
            ChatClientAgent api = _ollamaApiClient.AsAIAgent(ins, "", "");
            var r = await api.RunAsync<string>(page).ConfigureAwait(false);
            return r;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract keywords for remote knowledge source.");
            return null; // Return null on failure
        }
    }








    /*


        public async Task<string> RunAsync(string url, CancellationToken cancellationToken = default)
        {
            var startingUrl = string.IsNullOrWhiteSpace(_settings.LearnBaseUrl) ? "https://learn.microsoft.com/en-us/agent-framework/" : _settings.LearnBaseUrl;
            var crawlList = await TocHrefExtractor.GetTocList().ConfigureAwait(false);


            string address;


            //  var ingestionRunResult = await _docRepository.InitiateDataRunAsync(url, cancellationToken).ConfigureAwait(false);
            //   Guid ingestionRunId = ingestionRunResult.IngestionRunId.Value;
            //   Guid snapshotId = ingestionRunResult.SnapshotId.Value;

            foreach (var pg in crawlList)
            {
                cancellationToken.ThrowIfCancellationRequested();
                address = startingUrl + pg;

                _logger.LogInformation("Ingesting page {Page}", address);
                try
                {
                    var page = await _headlessBrowser.GetPageSourceAsync(address, cancellationToken).ConfigureAwait(false);
                    var normalized = page.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("\t", "");

                    HtmlDocument htmlDoc = new();
                    htmlDoc.LoadHtml(normalized);
                    var title = htmlDoc.DocumentNode.SelectSingleNode("//title")?.InnerText.Trim();

                    HtmlNodeCollection metatags = htmlDoc.DocumentNode.SelectNodes("//meta");



                    HtmlNodeCollection content = htmlDoc.DocumentNode.SelectNodes("//div[@data-main-column]//h1 | //div[@data-main-column]//h2 | //div[@data-main-column]//h3 | //div[@data-main-column]//h4 | //div[@data-main-column]//h5 | //div[@data-main-column]//h6 | //div[@data-main-column]//p");
                    var combined = CombineSuccessiveParagraphs(content);

                    Guid pageId = Guid.NewGuid();
                    DateTime? metaDate = null;
                    var updatedDate = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='updated_at']")?.GetAttributeValue("content", null);

                    if (!string.IsNullOrWhiteSpace(updatedDate) && DateTime.TryParse(updatedDate, out DateTime parsedDate))
                    {
                        metaDate = parsedDate;
                    }

                    DocPage dp = new()
                    {
                        Id = pageId,
                        //       SemanticUid = semantic,
                        //           SemanticUidHash = HashUtils.ComputeSha256(semantic),
                        //           SourceSnapshotId = snapshotId,
                        SourcePath = address,
                        Url = address,
                        VersionNumber = 0,
                        //           CreatedIngestionRunId = ingestionRunId,
                        ValidFromUtc = DateTime.UtcNow,
                        IsActive = false,
                        ContentHash = HashUtils.ComputeSha256(page),
                        RawMarkdown = page,
                        RawPageSource = page,
                        Description = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='description']")?.GetAttributeValue("content", null),
                        MetaDate = metaDate,
                        Isgarbage = false,
                        Title = title,
                        Language = "en-US"
                    };


                    List<DocSection> sections = [];

                    var orderIndex = 0;



                    static int? TryGetHeadingLevel(string name)
                    {
                        return string.IsNullOrWhiteSpace(name)
                            ? null
                            : name.Length == 2 && name.StartsWith("h", StringComparison.OrdinalIgnoreCase) && char.IsDigit(name[1])
                            ? int.Parse(name.Substring(1))
                            : null;
                    }



                    foreach ((HtmlNode Heading, HtmlNode Paragraph) in combined)
                    {
                        var headingText = Heading?.InnerText?.Trim();
                        var paragraphText = Paragraph?.InnerText?.Trim();
                        var sectionContent = string.IsNullOrWhiteSpace(headingText)
                                ? paragraphText ?? string.Empty
                                : string.IsNullOrWhiteSpace(paragraphText)
                                        ? headingText
                                        : $"{headingText} {paragraphText}";
                        var level = TryGetHeadingLevel(Heading?.Name);
                        var semanticUid = HashUtils.ComputeSemanticUidForSection(address, sectionContent, level ?? 2, Heading?.InnerStartIndex ?? 0);
                        var contentHash = HashUtils.ComputeSha256(sectionContent);
                        DocSection sec = new()
                        {
                            Id = Guid.NewGuid(),
                            SemanticUid = semanticUid,
                            Level = level,
                            OrderIndex = orderIndex++,
                            SemanticUidHash = HashUtils.ComputeSha256(semanticUid),
                            ValidFromUtc = DateTime.UtcNow,
                            ContentHash = contentHash,
                            Heading = headingText,
                            ContentMarkdown = paragraphText,
                            //                CreatedIngestionRunId = ingestionRunId,
                            DocPageId = pageId
                        };
                        sections.Add(sec);

                    }

                    LearnPageParseResult result = new()
                    {
                        Page = dp,
                        Sections = sections
                    };



                    await _docRepository.InsertPageAsync(result).ConfigureAwait(false);
                    _logger.LogInformation("Ingested page {Page}", address);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Ingestion cancelled while processing {Page}", address);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to ingest page {Page}", address);
                }




            }

            //      await _docRepository.CompleteIngestionRunAsync(ingestionRunId, cancellationToken).ConfigureAwait(false);
            return "Completed";
        }


        */








    private static void SaveSqlRemoteKnowledgeSource(RemoteRAG rag)
    {
        SqlConnection conn = new(Environment.GetEnvironmentVariable("CONN_STRING"));
        //add rag to database
        var insertQuery = @"INSERT INTO RemoteRAG (title, description, og_url, document_id, updated_at, ms_date, summary, keywords, embedding, token_count, version)
                                VALUES (@title, @description, @og_url, @document_id, @updated_at, @ms_date, @summary, @keywords, @embedding, @token_count, @version)";
        using (SqlCommand cmd = new(insertQuery, conn))
        {
            cmd.Parameters.AddWithValue("@title", rag.Title ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@description", rag.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@og_url", rag.OgUrl ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@document_id", rag.DocumentId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@updated_at", rag.UpdatedAt == DateTime.MinValue ? DBNull.Value : rag.UpdatedAt);
            cmd.Parameters.AddWithValue("@ms_date", rag.MSDate == DateTime.MinValue ? DBNull.Value : rag.MSDate);
            cmd.Parameters.AddWithValue("@summary", rag.Summary ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@keywords", rag.Keywords ?? (object)DBNull.Value);
            // SqlVector<float> is not directly supported by SqlClient, needs conversion or a custom type handler.
            // For simplicity, assuming a method to convert SqlVector to byte array or similar.
            // This part might need further implementation based on how SqlVector is stored.
            // As a placeholder, we'll set it to DBNull if it's empty or invalid.
            cmd.Parameters.AddWithValue("@embedding", rag.Embedding);
            cmd.Parameters.AddWithValue("@token_count", rag.TokenCount);
            cmd.Parameters.AddWithValue("@version", rag.Version);

            conn.Open();
            cmd.ExecuteNonQuery();
        }





    }








    private async Task<AgentResponse?> SummarizeContent(string content)
    {

        var ins = """
                  - You are a senior documentation writer. Your task is to summarize the provided content and create a concise summary of the content in a paragraph that does not exceed 450 tokens.
                  - The summary should be clear, informative, and helpful for someone who wants to quickly understand the essence of the documentation without reading the entire content.
                  - The paragraph should be well-structured and coherent, providing an overview of the content.
                  - You are to respond with a single paragraph with no more than 350 tokens in length.
                  """;

        try
        {
            ChatClientAgent api = _ollamaApiClient.AsAIAgent(ins, "", "");
            var r = await api.RunAsync<string>(content).ConfigureAwait(false);
            return r;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to summarize content for remote knowledge source.");
        }

        return null;
    }







    








    public sealed class RemoteRAG
    {
        public required string Description { get; init; }
        public required string DocumentId { get; init; }
        public SqlVector<float> Embedding { get; init; }
        public int Id { get; init; }
        public required string Keywords { get; init; }
        public DateTime MSDate { get; init; }
        public required string OgUrl { get; init; }
        public required string Summary { get; init; }
        public required string Title { get; init; }
        public int TokenCount { get; init; }
        public DateTime UpdatedAt { get; init; }
        public int Version { get; init; }
    }
}





public static class TocHrefExtractor
{

    private static void Extract(TocNode item, List<string> list)
    {
        if (!string.IsNullOrWhiteSpace(item.Href))
        {
            list.Add(item.Href);
        }

        if (item.Children == null)
        {
            return;
        }

        foreach (TocNode child in item.Children)
        {
            Extract(child, list);
        }
    }








    public static async Task<List<string>> GetTocList()
    {
        var tocUrl = "https://learn.microsoft.com/en-us/agent-framework/toc.json";

        using HttpClient client = new();

        var json = await client.GetStringAsync(tocUrl).ConfigureAwait(false);

        TocRoot tocDoc = JsonConvert.DeserializeObject<TocRoot>(json)
                         ?? throw new InvalidOperationException("Failed to deserialize TOC JSON.");

        List<string> list = [];

        foreach (TocNode item in tocDoc.Items)
        {
            Extract(item, list);
        }

        return list;
    }
}





public sealed class TocNode
{
    public required List<TocNode> Children { get; init; }
    public required string Href { get; init; }
    public required List<TocNode> Items { get; init; }
    public required string Name { get; init; }
}





public sealed class TocRoot
{
    public required List<TocNode> Items { get; init; }
}





public sealed class ContentBlock
{
    public string Kind { get; init; } = "";
    public int Level { get; init; }
    public string Text { get; init; } = "";
}