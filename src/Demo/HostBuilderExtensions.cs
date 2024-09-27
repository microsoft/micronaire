// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Net;
using Micronaire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.SemanticKernel;

namespace Demo;

public static class HostBuilderExtensions
{
    public static IHostApplicationBuilder AddDemo(this IHostApplicationBuilder builder)
    {
        var kernelConfig =
            builder
                .Configuration.GetRequiredSection(nameof(AzureOpenAIConfig))
                .Get<AzureOpenAIConfig>()
            ?? throw new NullReferenceException($"Missing {nameof(AzureOpenAIConfig)}");

        var kernelBuilder = builder.Services.AddKernel();
        kernelBuilder.Services.ConfigureHttpClientDefaults(c =>
        {
            // Use a standard resiliency policy, augmented to retry on 401 Unauthorized for this example
            c.AddStandardResilienceHandler()
                .Configure(o =>
                {
                    o.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(2);
                    o.Retry.ShouldHandle = args =>
                        ValueTask.FromResult(
                            args.Outcome.Result?.StatusCode is HttpStatusCode.Unauthorized
                                || args.Outcome.Result?.StatusCode is HttpStatusCode.TooManyRequests
                        );
                    o.Retry.MaxRetryAttempts = 2;
                    o.Retry.BackoffType = Polly.DelayBackoffType.Constant;
                    o.Retry.UseJitter = false;
                    o.Retry.Delay = TimeSpan.FromSeconds(40.0);
                });
        });

#pragma warning disable SKEXP0020, CS0612
        kernelBuilder.AddQdrantVectorStore("localhost");
#pragma warning restore SKEXP0020, CS0612

        kernelBuilder.AddAzureOpenAIChatCompletion(
            kernelConfig.ChatModelDeploymentName,
            kernelConfig.Endpoint,
            kernelConfig.ApiKey
        );

#pragma warning disable SKEXP0010
        kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
            kernelConfig.EmbeddingModelDeploymentName,
            kernelConfig.Endpoint,
            kernelConfig.ApiKey
        );
#pragma warning restore SKEXP0010
        kernelBuilder.Plugins.AddFromType<Search>();

        builder.Services.AddSingleton<IRagPipeline, RagPipeline>();

        return builder;
    }
}
