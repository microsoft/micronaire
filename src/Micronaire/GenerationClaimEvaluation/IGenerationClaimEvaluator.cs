// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Micronaire.Claims;
using Microsoft.SemanticKernel;

namespace Micronaire.GenerationClaimEvaluation;

/// <summary>
/// Evaluates generation using claims.
/// </summary>
public interface IGenerationClaimEvaluator
{
    /// <summary>
    /// Evaluates generation using the context and ground truth answer and generated claims.
    /// </summary>
    /// <param name="evaluator">The evaluator to use for calculating similarity.</param>
    /// <param name="contextClaims">The context claims.</param>
    /// <param name="contextChunkClaims">The context claims per chunk.</param>
    /// <param name="generatedClaims">The generated claims.</param>
    /// <param name="groundTruthClaims">The ground truth claims.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The evaluation report for the generation claim.</returns>
    public Task<GenerationClaimReport> EvaluateGeneratorMetricsAsync(
        Kernel evaluator,
        IEnumerable<Claim> contextClaims,
        IEnumerable<IEnumerable<Claim>> contextChunkClaims,
        IEnumerable<Claim> generatedClaims,
        IEnumerable<Claim> groundTruthClaims,
        CancellationToken cancellationToken = default
    );
}
