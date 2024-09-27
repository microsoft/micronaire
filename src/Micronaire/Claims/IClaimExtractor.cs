// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.SemanticKernel;

namespace Micronaire.Claims;

/// <summary>
/// Extracts claims from a chunk of text.
/// </summary>
public interface IClaimExtractor
{
    /// <summary>
    /// We enforce the following requirement.The input should be some chunk of text.
    /// The chunk shall be tokenized into sentences prefixed with a sentence ID.
    /// </summary>
    /// <param name="evaluator">The evaluator to use for extracting claims.</param>
    /// <param name="chunk">Text with sentences separated by sentence ending punctuation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A set of <see cref="Claim"/> objects, each representing a claim extracted from the input text and reference sentence ids..
    /// </returns>
    public Task<IEnumerable<Claim>> ExtractClaimsAsync(
        Kernel evaluator,
        string chunk,
        CancellationToken cancellationToken = default
    );
}
