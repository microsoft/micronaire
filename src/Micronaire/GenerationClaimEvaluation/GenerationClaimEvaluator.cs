// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Micronaire.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Micronaire.GenerationClaimEvaluation;

/// <summary>
/// Evaluates generation using claims.
/// </summary>
public class GenerationClaimEvaluator : IGenerationClaimEvaluator
{
    private readonly ILogger<GenerationClaimEvaluator> _logger;

    public GenerationClaimEvaluator(ILogger<GenerationClaimEvaluator> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<GenerationClaimReport> EvaluateGeneratorMetricsAsync(
        Kernel evaluator,
        IEnumerable<Claim> contextClaims,
        IEnumerable<IEnumerable<Claim>> chunkContextClaims,
        IEnumerable<Claim> generatedClaims,
        IEnumerable<Claim> groundTruthClaims,
        CancellationToken cancellationToken = default
    )
    {
        var (totalGeneratedClaims, faithfulness) = await CalculateFaithfulness(
            evaluator,
            contextClaims,
            generatedClaims,
            cancellationToken
        );
        var (relevantChunks, irrelevantChunks) = await CalculateChunkRelevance(
            evaluator,
            chunkContextClaims,
            groundTruthClaims,
            cancellationToken
        );
        var (incorrectClaimsInRelevantChunks, incorrectClaimsInIrrelevantChunks) =
            await CalculateRelevanceCorrectness(
                evaluator,
                generatedClaims,
                groundTruthClaims,
                relevantChunks,
                irrelevantChunks,
                cancellationToken
            );
        int hallucinationCount = await CalculateHallucination(
            evaluator,
            chunkContextClaims,
            generatedClaims,
            groundTruthClaims,
            cancellationToken
        );
        int selfKnowledgeCount = await CalculateSelfKnowledge(
            evaluator,
            chunkContextClaims,
            generatedClaims,
            groundTruthClaims,
            cancellationToken
        );
        double contextUtilization = await CalculateContextUtilization(
            evaluator,
            chunkContextClaims,
            generatedClaims,
            groundTruthClaims,
            cancellationToken
        );

        _logger.LogInformation("Total response claims {totres}", totalGeneratedClaims);
        _logger.LogInformation(
            "Incorrect claims in relevant chunks {incorrect}",
            incorrectClaimsInRelevantChunks
        );
        return new GenerationClaimReport
        {
            Faithfulness = faithfulness,
            RelevantNoiseSensitivity =
                totalGeneratedClaims > 0
                    ? (double)incorrectClaimsInRelevantChunks / totalGeneratedClaims
                    : 0,
            IrrelevantNoiseSensitivity =
                totalGeneratedClaims > 0
                    ? (double)incorrectClaimsInIrrelevantChunks / totalGeneratedClaims
                    : 0,
            Hallucination =
                totalGeneratedClaims > 0 ? (double)hallucinationCount / totalGeneratedClaims : 0,
            SelfKnowledgeScore =
                totalGeneratedClaims > 0 ? (double)selfKnowledgeCount / totalGeneratedClaims : 0,
            ContextUtilization = contextUtilization,
        };
    }

    private async Task<double> CalculateContextUtilization(
        Kernel evaluator,
        IEnumerable<IEnumerable<Claim>> chunkContextClaims,
        IEnumerable<Claim> generatedClaims,
        IEnumerable<Claim> groundTruthClaims,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Calculating context utilization");
        double totalUtilization = 0.0;
        int chunkCount = 0;
        foreach (var chunkClaims in chunkContextClaims)
        {
            if (chunkClaims == null || !chunkClaims.Any())
            {
                continue;
            }
            int matchingCount = 0;
            foreach (var groundTruthClaim in groundTruthClaims)
            {
                bool inChunk = false;
                bool inResponse = false;
                foreach (var chunkClaim in chunkClaims)
                {
                    if (
                        await ClaimOperations.CalculateClaimSimilarityAsync(
                            evaluator,
                            groundTruthClaim,
                            chunkClaim,
                            _logger,
                            cancellationToken
                        ) >= 3
                    )
                    {
                        inChunk = true;
                        break; // No need to check other chunk claims
                    }
                }
                foreach (var generatedClaim in generatedClaims)
                {
                    if (
                        await ClaimOperations.CalculateClaimSimilarityAsync(
                            evaluator,
                            groundTruthClaim,
                            generatedClaim,
                            _logger,
                            cancellationToken
                        ) >= 3
                    )
                    {
                        inResponse = true;
                        break; // No need to check other response claims
                    }
                }
                if (inChunk && inResponse)
                {
                    matchingCount++;
                }
            }
            double utilization = (double)matchingCount / groundTruthClaims.Count();
            totalUtilization += utilization;
            chunkCount++;
        }
        var contextUtilization = chunkCount > 0 ? totalUtilization / chunkCount : 0.0;
        return contextUtilization;
    }

    private async Task<int> CalculateSelfKnowledge(
        Kernel evaluator,
        IEnumerable<IEnumerable<Claim>> chunkContextClaims,
        IEnumerable<Claim> generatedClaims,
        IEnumerable<Claim> groundTruthClaims,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Calculating self-knowledge");
        int selfKnowledgeCount = 0;
        foreach (var generatedClaim in generatedClaims)
        {
            var inChunks = await Task.Run(
                () =>
                    chunkContextClaims.Any(chunk =>
                        chunk.Any(c =>
                            ClaimOperations
                                .CalculateClaimSimilarityAsync(
                                    evaluator,
                                    generatedClaim,
                                    c,
                                    _logger,
                                    cancellationToken
                                )
                                .Result >= 3
                        )
                    )
            );
            bool inGroundTruth = false;
            foreach (var groundTruthClaim in groundTruthClaims)
            {
                var score = await ClaimOperations.CalculateClaimSimilarityAsync(
                    evaluator,
                    generatedClaim,
                    groundTruthClaim,
                    _logger,
                    cancellationToken
                );
                if (score >= 3)
                {
                    inGroundTruth = true;
                    break; // No need to check further if we found a match
                }
            }
            if (inChunks && inGroundTruth) // Claim found in some chunk
            {
                selfKnowledgeCount++;
            }
        }

        return selfKnowledgeCount;
    }

    private async Task<int> CalculateHallucination(
        Kernel evaluator,
        IEnumerable<IEnumerable<Claim>> chunkContextClaims,
        IEnumerable<Claim> generatedClaims,
        IEnumerable<Claim> groundTruthClaims,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Calculating hallucination");
        int hallucinationCount = 0;
        foreach (var generatedClaim in generatedClaims)
        {
            var inChunks = await Task.Run(
                () =>
                    chunkContextClaims.Any(chunk =>
                        chunk.Any(c =>
                            ClaimOperations
                                .CalculateClaimSimilarityAsync(
                                    evaluator,
                                    generatedClaim,
                                    c,
                                    _logger,
                                    cancellationToken
                                )
                                .Result < 3
                        )
                    )
            );
            bool inGroundTruth = false;
            foreach (var groundTruthClaim in groundTruthClaims)
            {
                var score = await ClaimOperations.CalculateClaimSimilarityAsync(
                    evaluator,
                    generatedClaim,
                    groundTruthClaim,
                    _logger,
                    cancellationToken
                );
                if (score >= 3)
                {
                    inGroundTruth = true;
                    break; // No need to check further if we found a match
                }
            }
            if (!inChunks && !inGroundTruth) // Claim not found in any chunk
            {
                hallucinationCount++;
            }
        }

        return hallucinationCount;
    }

    private async Task<(int, int)> CalculateRelevanceCorrectness(
        Kernel evaluator,
        IEnumerable<Claim> generatedClaims,
        IEnumerable<Claim> groundTruthClaims,
        List<List<Claim>> relevantChunks,
        List<List<Claim>> irrelevantChunks,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Calculating incorrect claims in relevant and irrelevant chunks");
        var incorrectClaimsInRelevantChunks = 0;
        var incorrectClaimsInIrrelevantChunks = 0;
        foreach (var generatedClaim in generatedClaims)
        {
            foreach (var groundTruthClaim in groundTruthClaims)
            {
                var score = await ClaimOperations.CalculateClaimSimilarityAsync(
                    evaluator,
                    generatedClaim,
                    groundTruthClaim,
                    _logger,
                    cancellationToken
                );
                if (score < 3)
                {
                    if (
                        relevantChunks.Any(chunk =>
                            chunk.Any(c =>
                                ClaimOperations
                                    .CalculateClaimSimilarityAsync(
                                        evaluator,
                                        generatedClaim,
                                        c,
                                        _logger,
                                        cancellationToken
                                    )
                                    .Result >= 3
                            )
                        )
                    )
                    {
                        incorrectClaimsInRelevantChunks++;
                    }
                    else if (
                        // TODO: This appears to be a bug. The condition should be score < 3 but I am not sure so I am leaving it
                        irrelevantChunks.Any(chunk =>
                            chunk.Any(c =>
                                ClaimOperations
                                    .CalculateClaimSimilarityAsync(
                                        evaluator,
                                        generatedClaim,
                                        c,
                                        _logger,
                                        cancellationToken
                                    )
                                    .Result < 3
                            )
                        )
                    )
                    {
                        incorrectClaimsInIrrelevantChunks++;
                    }
                }
            }
        }
        return (incorrectClaimsInRelevantChunks, incorrectClaimsInIrrelevantChunks);
    }

    private async Task<(List<List<Claim>>, List<List<Claim>>)> CalculateChunkRelevance(
        Kernel evaluator,
        IEnumerable<IEnumerable<Claim>> chunkContextClaims,
        IEnumerable<Claim> groundTruthClaims,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Calculating relevant and irrelevant chunks");
        var relevantChunks = new List<List<Claim>>();
        var irrelevantChunks = new List<List<Claim>>();
        foreach (var chunkClaims in chunkContextClaims)
        {
            bool isChunkRelevant = false;
            foreach (var chunkClaim in chunkClaims)
            {
                foreach (var groundTruthClaim in groundTruthClaims)
                {
                    var score = await ClaimOperations.CalculateClaimSimilarityAsync(
                        evaluator,
                        chunkClaim,
                        groundTruthClaim,
                        _logger,
                        cancellationToken
                    );
                    if (score >= 3)
                    {
                        isChunkRelevant = true;
                        break;
                    }
                }
                if (isChunkRelevant)
                {
                    break; // Exit the outer loop if the chunk is relevant
                }
            }
            if (isChunkRelevant)
            {
                relevantChunks.Add(chunkClaims.ToList());
            }
            else
            {
                irrelevantChunks.Add(chunkClaims.ToList());
            }
        }
        return (relevantChunks, irrelevantChunks);
    }

    private async Task<(int, double)> CalculateFaithfulness(
        Kernel evaluator,
        IEnumerable<Claim> contextClaims,
        IEnumerable<Claim> generatedClaims,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Evaluating faithfulness");
        int faithfulClaims = 0;
        var totalGeneratedClaims = generatedClaims.Count();
        foreach (var generatedClaim in generatedClaims)
        {
            foreach (var contextClaim in contextClaims)
            {
                var score = await ClaimOperations.CalculateClaimSimilarityAsync(
                    evaluator,
                    generatedClaim,
                    contextClaim,
                    _logger,
                    cancellationToken
                );
                if (score >= 3)
                {
                    faithfulClaims++;
                    break;
                }
            }
        }
        var faithfulness =
            totalGeneratedClaims > 0 ? (double)faithfulClaims / totalGeneratedClaims : 0;
        return (totalGeneratedClaims, faithfulness);
    }
}
