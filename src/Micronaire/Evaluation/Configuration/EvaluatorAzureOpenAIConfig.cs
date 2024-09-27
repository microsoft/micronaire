// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Micronaire;

/// <summary>
/// Configuration settings for the Azure OpenAI Evaluator Model.
/// </summary>
sealed class EvaluatorAzureOpenAIConfig
{
    /// <summary>
    /// Gets or sets the name of the chat model deployment.
    /// </summary>
    public required string ChatModelDeploymentName { get; set; }

    /// <summary>
    /// Gets or sets the endpoint URL for the Azure OpenAI service.
    /// </summary>
    public required string Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the API key for accessing the Azure OpenAI service.
    /// </summary>
    public required string ApiKey { get; set; }
}
