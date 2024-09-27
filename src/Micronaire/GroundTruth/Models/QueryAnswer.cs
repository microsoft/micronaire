// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Micronaire.GroundTruth;

/// <summary>
/// Represents a question and answer pair.
/// </summary>
sealed class QuestionAndAnswer
{
    /// <summary>
    /// The question.
    /// </summary>
    public required string Question { get; set; }

    /// <summary>
    /// The answer.
    /// </summary>
    public required string Answer { get; set; }
}
