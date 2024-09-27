// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.SemanticKernel;

namespace Micronaire.LLMEvaluation;

public interface ILLMEvaluator
{
    public Task<LLMEvaluationReport> EvaluateAsync(
        Kernel kernel,
        string question,
        string context,
        string answer,
        string groundTruth,
        CancellationToken cancellationToken = default
    );
}
