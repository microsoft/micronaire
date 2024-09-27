// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Micronaire;

/// <summary>
/// All RAG pipelines should implement this interface.
/// </summary>
public interface IRagPipeline
{
    /// <summary>
    /// The generate method implemented must return a tuple of generator response and retriever generated context.
    /// </summary>
    /// <param name="query">Query to ask RAG pipeline.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public Task<(string Response, IEnumerable<RagContext> Context)> GenerateAsync(
        string query,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Loads documents from the given file path to pipeline.
    /// </summary>
    /// <param name="filePath">path to the document.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    public Task LoadAsync(string filePath, CancellationToken cancellationToken = default);
}
