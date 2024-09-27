// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Micronaire.GenerationClaimEvaluation;
using Micronaire.LLMEvaluation;
using Micronaire.OverallClaimEvaluation;
using Micronaire.RetrievalClaimEvaluation;

namespace Micronaire;

/// <summary>
/// Report for a question in the evaluation.
/// </summary>
public class QuestionReport
{
    /// <summary>
    /// The question being evaluated.
    /// </summary>
    public required string Question { get; set; }

    /// <summary>
    /// The report from the LLM evaluator.
    /// </summary>
    public required LLMEvaluationReport LLMReport { get; set; }

    /// <summary>
    /// The report from the overall claim evaluator.
    /// </summary>
    public required OverallClaimReport OverallClaimReport { get; set; }

    /// <summary>
    /// The report from the retrieval claim evaluator.
    /// </summary>
    public required RetrievalClaimReport RetrievalClaimReport { get; set; }

    /// <summary>
    /// The report from the generation claim evaluator.
    /// </summary>
    public required GenerationClaimReport GenerationClaimReport { get; set; }
}
