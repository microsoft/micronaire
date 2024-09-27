// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Micronaire.Claims;
using Microsoft.SemanticKernel;

namespace Micronaire.RetrievalClaimEvaluation;

/// <summary>
/// Evaluates retrieval using claims.
/// </summary>
public interface IRetrievalClaimEvaluator
{
    /// <summary>
    /// Evaluates retrieval using the context and ground truth answer.
    /// </summary>
    /// <param name="evaluator">The evaluator to use for calculating similarity.</param>
    /// <param name="groundTruthClaims">The ground truth claims.</param>
    /// <param name="contextClaims">The context claims.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The evaluation report for the retrieval claim.</returns>
    Task<RetrievalClaimReport> EvaluateAsync(
        Kernel evaluator,
        IEnumerable<Claim> groundTruthClaims,
        IEnumerable<Claim> contextClaims,
        CancellationToken cancellationToken = default
    );
}
