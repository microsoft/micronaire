// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Micronaire.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Micronaire.RetrievalClaimEvaluation;

/// <summary>
/// Evaluates retrieval using claims.
/// </summary>
public class RetrievalClaimEvaluator : IRetrievalClaimEvaluator
{
    private readonly IClaimExtractor _claimExtractor;
    private readonly ILogger<RetrievalClaimEvaluator> _logger;

    public RetrievalClaimEvaluator(
        IClaimExtractor claimExtractor,
        ILogger<RetrievalClaimEvaluator> logger
    )
    {
        _claimExtractor = claimExtractor;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<RetrievalClaimReport> EvaluateAsync(
        Kernel evaluator,
        IEnumerable<Claim> groundTruthClaims,
        IEnumerable<Claim> contextClaims,
        CancellationToken cancellationToken = default
    )
    {
        var totalGroundTruthClaims = groundTruthClaims.Count();
        var coveredClaims = 0;
        int relevantChunks = 0;
        foreach (var contextClaim in contextClaims)
        {
            bool foundRelevantClaim = false;
            foreach (var groundTruthClaim in groundTruthClaims)
            {
                var score = await ClaimOperations.CalculateClaimSimilarityAsync(
                    evaluator,
                    contextClaim,
                    groundTruthClaim,
                    _logger,
                    cancellationToken
                );
                if (score >= 3)
                {
                    coveredClaims++;
                    if (!foundRelevantClaim)
                    {
                        relevantChunks++;
                        foundRelevantClaim = true;
                    }
                }
            }
        }

        double claimRecall =
            totalGroundTruthClaims > 0 ? (double)coveredClaims / totalGroundTruthClaims : 0;
        _logger.LogInformation("Claim Recall: {claimRecall}", claimRecall);

        double contextPrecision =
            contextClaims.Count() > 0 ? (double)relevantChunks / contextClaims.Count() : 0;
        this._logger.LogInformation("Context Precision: {contextPrecision}", contextPrecision);

        return new RetrievalClaimReport()
        {
            ClaimRecall = claimRecall,
            ContextPrecision = contextPrecision,
        };
    }
}
