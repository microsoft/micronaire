// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Micronaire.OverallClaimEvaluation;

/// <summary>
/// Overall report based on claim analysis.
/// </summary>
public class OverallClaimReport
{
    /// <summary>
    /// Proportion of correct claims in response.
    /// </summary>
    public double Precision { get; set; }

    /// <summary>
    /// Proportion of correct claims in ground-truth.
    /// </summary>
    public double Recall { get; set; }

    /// <summary>
    /// Harmonic average of precision and recall.
    /// </summary>
    public double F1Score { get; set; }
}
