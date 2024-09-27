// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Demo;

public class AzureOpenAIConfig
{
    public required string ChatModelDeploymentName { get; set; }
    public required string EmbeddingModelDeploymentName { get; set; }
    public required string Endpoint { get; set; }
    public required string ApiKey { get; set; }
}
