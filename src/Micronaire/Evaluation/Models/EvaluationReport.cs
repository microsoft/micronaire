// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Micronaire.GenerationClaimEvaluation;
using Micronaire.LLMEvaluation;
using Micronaire.OverallClaimEvaluation;
using Micronaire.RetrievalClaimEvaluation;

namespace Micronaire;

/// <summary>
/// Full evaluation report for the RAG pipeline.
/// </summary>
public class EvaluationReport
{
    /// <summary>
    /// The set of reports for each question in the evaluation.
    /// </summary>
    public required IEnumerable<QuestionReport> QuestionReports { get; set; }

    /// <summary>
    /// The report from the LLM evaluator.
    /// </summary>
    public required LLMEvaluationReport AverageLLMReport { get; set; }

    /// <summary>
    /// The report from the overall claim evaluator.
    /// </summary>
    public required OverallClaimReport AverageOverallClaimReport { get; set; }

    /// <summary>
    /// The report from the retrieval claim evaluator.
    /// </summary>
    public required RetrievalClaimReport AverageRetrievalClaimReport { get; set; }

    /// <summary>
    /// The report from the generation claim evaluator.
    /// </summary>
    public required GenerationClaimReport AverageGenerationClaimReport { get; set; }
}
