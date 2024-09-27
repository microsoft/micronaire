// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Micronaire;

/// <summary>
/// An evaluator for RAG pipelines.
/// </summary>
public interface IEvaluator
{
    /// <summary>
    /// Evaluates the given pipeline using the ground truth data at the given path.
    /// </summary>
    /// <param name="pipeline">The pipeline to use for evaluation.</param>
    /// <param name="groundTruthPath">The path to json file with ground truth answers and questions.</param>
    /// <returns>An <see cref="EvaluationReport"/> that has all the gathered evaluation data.</returns>
    public Task<EvaluationReport> EvaluateAsync(
        IRagPipeline pipeline,
        string groundTruthPath,
        CancellationToken cancellationToken = default
    );
}
