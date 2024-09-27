// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Micronaire.LLMEvaluation;

public class LLMEvaluator : ILLMEvaluator
{
    private readonly ILogger<LLMEvaluator> _logger;

    public LLMEvaluator(ILogger<LLMEvaluator> logger)
    {
        _logger = logger;
    }

    public async Task<LLMEvaluationReport> EvaluateAsync(
        Kernel kernel,
        string question,
        string context,
        string answer,
        string groundTruth,
        CancellationToken cancellationToken = default
    )
    {
        var ctx = new KernelArguments()
        {
            { "question", question },
            { "context", context },
            { "answer", answer },
            { "ground_truth", groundTruth },
        };
        var groundednessResult = (
            await kernel.InvokeAsync("Plugins", "Groundedness", ctx)
        )?.GetValue<string>();
        _logger.LogInformation("Got groundedness: {res}", groundednessResult);
        var groundedness =
            (groundednessResult?.FirstOrDefault(c => "12345".Contains(c), '1') ?? '1') - '0';

        var relevanceResult = (
            await kernel.InvokeAsync("Plugins", "Relevance", ctx)
        )?.GetValue<string>();
        _logger.LogInformation("Got relevance: {res}", relevanceResult);
        var relevance =
            (relevanceResult?.FirstOrDefault(c => "12345".Contains(c), '1') ?? '1') - '0';

        var coherenceResult = (
            await kernel.InvokeAsync("Plugins", "Coherence", ctx)
        )?.GetValue<string>();
        _logger.LogInformation("Got coherence: {res}", coherenceResult);
        var coherence =
            (coherenceResult?.FirstOrDefault(c => "12345".Contains(c), '1') ?? '1') - '0';

        var fluencyResult = (
            await kernel.InvokeAsync("Plugins", "Fluency", ctx)
        )?.GetValue<string>();
        _logger.LogInformation("Got fluency: {res}", fluencyResult);
        var fluency = (fluencyResult?.FirstOrDefault(c => "12345".Contains(c), '1') ?? '1') - '0';

        var retrievalResult = (
            await kernel.InvokeAsync("Plugins", "RetrievalScore", ctx)
        )?.GetValue<string>();
        _logger.LogInformation("Got retrieval score: {res}", retrievalResult);
        var retrieval =
            (retrievalResult?.FirstOrDefault(c => "12345".Contains(c), '1') ?? '1') - '0';

        var similarityResult = (
            await kernel.InvokeAsync("Plugins", "Similarity", ctx)
        )?.GetValue<string>();
        _logger.LogInformation("Got similarity: {res}", similarityResult);
        var similarity =
            (similarityResult?.FirstOrDefault(c => "12345".Contains(c), '1') ?? '1') - '0';

        return new()
        {
            Groundedness = (double)(groundedness - 1) / 4,
            Relevance = (double)(relevance - 1) / 4,
            Coherence = (double)(coherence - 1) / 4,
            Fluency = (double)(fluency - 1) / 4,
            RetrievalScore = (double)(retrieval - 1) / 4,
            Similarity = (double)(similarity - 1) / 4,
        };
    }
}
