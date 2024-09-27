// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Micronaire.RetrievalClaimEvaluation;

/// <summary>
/// Report for evaluating retrieval using claims.
/// </summary>
public class RetrievalClaimReport
{
    /// <summary>
    /// The proportion of ground-truth claims covered by retrieved chunks.
    /// </summary>
    public double ClaimRecall { get; set; }

    /// <summary>
    /// The proportion of relevant chunks to total retrieved chunks.
    /// </summary>
    public double ContextPrecision { get; set; }
}
