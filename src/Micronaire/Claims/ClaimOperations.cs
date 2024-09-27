// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Micronaire.Claims;

/// <summary>
/// Contains operations related to claims.
/// </summary>
public static class ClaimOperations
{
    /// <summary>
    /// Calculates the similarity between two claims.
    /// </summary>
    /// <param name="evaluator">The evaluator to use for calculating similarity.</param>
    /// <param name="claim1">The first claim.</param>
    /// <param name="claim2">The second claim.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The similarity score between the two claims.</returns>
    public static async Task<double> CalculateClaimSimilarityAsync(
        Kernel evaluator,
        Claim claim1,
        Claim claim2,
        ILogger logger,
        CancellationToken cancellationToken = default
    )
    {
        var ctx = new KernelArguments()
        {
            { "claim_1", claim1.ExtractedClaim },
            { "claim_2", claim2.ExtractedClaim },
        };
        FunctionResult similarityScore = await evaluator.InvokeAsync(
            "ExtractorPlugins",
            "ClaimLevelComparison",
            ctx
        );
        var comparison = similarityScore.GetValue<string>() ?? string.Empty;
        var score = (comparison.FirstOrDefault(c => "12345".Contains(c), '1')) - '0';
        if (score < 1)
        {
            score = 1;
        }
        else if (score > 5)
        {
            score = 5;
        }
        logger.LogInformation("Explanation: {explanation}", comparison);
        logger.LogInformation("Claim similarity score: {score}", score);
        return score;
    }
}
