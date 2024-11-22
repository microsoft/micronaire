// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Demo;
using Micronaire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var qdrantFixture = new VectorStoreQdrantContainerFixture();
await qdrantFixture.ManualInitializeAsync();

try
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.AddDemo();
    builder.AddMicronaire();
    var host = builder.Build();

    var pipeline = host.Services.GetRequiredService<IRagPipeline>();
    var filePath = "data/Romeo and Juliet.txt";

    // var searchString = "Why does Friar Lawrence decide to marry Romeo and Juliet?";

    await pipeline.LoadAsync(filePath);

    //var (response, context) = await pipeline.Generate(searchString);
    //Console.WriteLine(response);

    // TODO: Call evaluator passing in the RAG pipeline to generate metrics for the json file
    var ragEvaluator = host.Services.GetRequiredService<IEvaluator>();
    await ragEvaluator.EvaluateAsync(pipeline, "data\\GroundTruthAnswers.json");

    // TODO:  evaluator should provide method to load ground truth answers and query from file path as json file
    // then evaluator would run through each query and ground truth answer, run RAG pipeline and compare response with ground truth
    // and get metrics for each query response. Can you aggregate over all questions to get aggregate
    // metrics for reporting.

    // We would want evaluator to take in the RAG pipeline so that it can run the pipeline for each query and get response
    // so essentially we simplify API to take in RAG pipeline that implements IRagPipeline interface and path to json file with
    // query and ground truth and output metrics object.The evaluator should also save json file with question and answer generated
    // by RAG pipeline generator.

    // We impose requirement that RAG pipeline must implement a method to take in query and must return response and context.
}
finally
{
    await qdrantFixture.DisposeAsync();
}
