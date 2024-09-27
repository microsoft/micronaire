// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Micronaire.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Micronaire.OverallClaimEvaluation;

public class OverallClaimEvaluator : IOverallClaimEvaluator
{
    private readonly IClaimExtractor _claimExtractor;
    private readonly ILogger<OverallClaimEvaluator> _logger;

    public OverallClaimEvaluator(
        IClaimExtractor claimExtractor,
        ILogger<OverallClaimEvaluator> logger
    )
    {
        _claimExtractor = claimExtractor;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<OverallClaimReport> EvaluateAsync(
        Kernel evaluator,
        IEnumerable<Claim> generatedClaims,
        IEnumerable<Claim> groundTruthClaims,
        CancellationToken cancellationToken = default
    )
    {
        var correctClaims = 0;
        var pairs = generatedClaims.Zip(groundTruthClaims);
        foreach ((var generatedClaim, var groundTruthClaim) in pairs)
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
                correctClaims++;
            }
        }

        // Precision: proportion of correct claims in all response claims
        var precision = (double)correctClaims / generatedClaims.Count();

        // Recall: proportion of correct claims in all ground truth claims
        var recall = (double)correctClaims / groundTruthClaims.Count();

        // F1 score: harmonic mean of precision and recall
        var f1Score = 2 / ((precision == 0 ? 0 : 1 / precision) + (recall == 0 ? 0 : 1 / recall));

        return new()
        {
            Precision = (double)precision,
            Recall = (double)recall,
            F1Score = (double)f1Score,
        };
    }
}
