// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Micronaire;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Text;

namespace Demo;

#pragma warning disable SKEXP0001
public class RagPipeline : IRagPipeline
{
    private readonly IVectorStore _vectorStore;
    private readonly ITextEmbeddingGenerationService _embeddingGenerationService;
    private readonly IChatCompletionService _chatCompletionService;
    private ChatHistory _history;
    private readonly ILogger<RagPipeline> _logger;
    private readonly Kernel _kernel;
    private readonly IVectorStoreRecordCollection<Guid, Paragraph> _collection;
    private readonly AzureOpenAIPromptExecutionSettings _execSettings;

    public RagPipeline(
        IVectorStore vectorStore,
        ITextEmbeddingGenerationService embeddingGenerationService,
        IChatCompletionService chatCompletionService,
        Kernel kernel,
        ILogger<RagPipeline> logger
    )
    {
        _vectorStore = vectorStore;
        _embeddingGenerationService = embeddingGenerationService;
        _chatCompletionService = chatCompletionService;
        _history = new();
        _history.AddSystemMessage(
            "You are an assistant that answers questions about Romeo and Juliet. You can use search function to find needed information."
        );
        _logger = logger;
        _kernel = kernel;
        var collection = vectorStore.GetCollection<Guid, Paragraph>("Collection");
        collection.CreateCollectionIfNotExistsAsync().GetAwaiter().GetResult();
        _collection = collection;
        _execSettings = new AzureOpenAIPromptExecutionSettings()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
        };
    }

    public async Task LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var lines = File.ReadLines(filePath);
        var tokensPerParagraph = 1000;

#pragma warning disable SKEXP0050
        var plainParagraphs = TextChunker.SplitPlainTextParagraphs(lines, tokensPerParagraph);
#pragma warning restore SKEXP0050

        var paragraphs = plainParagraphs.Select(p => new Paragraph()
        {
            ParagraphId = Guid.NewGuid(),
            Text = p,
        });

        var embeddedParagraphs = paragraphs.Select(p =>
        {
            p.Embedding = _embeddingGenerationService
                .GenerateEmbeddingAsync(p.Text, cancellationToken: cancellationToken)
                .GetAwaiter()
                .GetResult();
            return p;
        });

        foreach (var p in embeddedParagraphs)
        {
            _logger.LogInformation("Upserting {documentGuid}", p.ParagraphId);
            await _collection.UpsertAsync(p, cancellationToken: cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task<(string, IEnumerable<RagContext>)> GenerateAsync(
        string searchString,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // since pipeline will run multiple queries. Clear previous history to prevent exceeding token limit.
            _history = new();
            _history.AddSystemMessage(
                "You are an assistant that answers questions about Romeo and Juliet. You can use search function to find needed information."
            );
            _history.AddUserMessage(searchString);
            var response = await _chatCompletionService.GetChatMessageContentAsync(
                _history,
                _execSettings,
                _kernel
            );
            var responseString = response.Content ?? string.Empty;
            _history.AddMessage(response.Role, responseString);

            // Collect all tool outputs that match the specified role
            var contexts = _history
                .Where(m => m.Role == AuthorRole.Tool)
                .Select(
                    (m, index) =>
                        new RagContext
                        {
                            Context = m.Content ?? string.Empty,
                            ChunkNumber =
                                index
                                + 1 // Assigning chunk numbers starting from 1
                            ,
                        }
                )
                .ToList();

            return (responseString, contexts);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during Generate: {ex.Message}");
            throw;
        }
    }
}
#pragma warning restore SKEXP0001
