// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Micronaire.Claims;
using Micronaire.GenerationClaimEvaluation;
using Micronaire.LLMEvaluation;
using Micronaire.OverallClaimEvaluation;
using Micronaire.RetrievalClaimEvaluation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Micronaire;

public static class HostBuilderExtensions
{
    public static IServiceCollection AddMicronaire(this IServiceCollection services)
    {
        return services
            .AddSingleton<IEvaluator, Evaluator>()
            .AddSingleton<ILLMEvaluator, LLMEvaluator>()
            .AddSingleton<IClaimExtractor, ClaimExtractor>()
            .AddSingleton<IOverallClaimEvaluator, OverallClaimEvaluator>()
            .AddSingleton<IRetrievalClaimEvaluator, RetrievalClaimEvaluator>()
            .AddSingleton<IGenerationClaimEvaluator, GenerationClaimEvaluator>();
    }

    public static IHostApplicationBuilder AddMicronaire(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMicronaire();
        return builder;
    }
}
