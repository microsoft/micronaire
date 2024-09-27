// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Micronaire.Claims;

/// <summary>
/// Represents a claim extracted from a text which is the actual claim
/// and the IDs for the sentences the claim attributes to.
/// </summary>
public class Claim
{
    /// <summary>
    /// Gets or Sets The extracted claim from the text.
    /// </summary>
    public required string ExtractedClaim { get; set; }

    /// <summary>
    /// Gets or Sets The IDs for the sentences the extracted claim attributes to.
    /// </summary>
    public required List<int> ExtractedClaimReferenceSentenceIds { get; set; }

    /// <summary>
    /// Gets or Sets a value indicating whether claim is triplet or sentence claim.
    /// </summary>
    public required bool IsTriplet { get; set; }

    /// <summary>
    /// Gets or Sets a value indicating whether the claim has been processed.
    /// </summary>
    public required bool IsProcessed { get; set; }
}
