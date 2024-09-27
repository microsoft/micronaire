// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Micronaire.Claims;

/// <summary>
/// The ClaimExtractor class implements the IClaimExtractor interface
/// and is responsible for extracting claims from a given text using a Semantic Kernel.
/// </summary>
public class ClaimExtractor : IClaimExtractor
{
    private readonly ILogger<ClaimExtractor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClaimExtractor"/> class.
    /// </summary>
    public ClaimExtractor(ILogger<ClaimExtractor> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Claim>> ExtractClaimsAsync(
        Kernel evaluator,
        string chunk,
        CancellationToken cancellationToken = default
    )
    {
        var tokenizedText = TokenizeText(chunk);
        string claimsList = await GetClaimsWithKernel(evaluator, tokenizedText, cancellationToken)
            .ConfigureAwait(false);
        var claims = ParseInputToClaims(claimsList);
        _logger.LogInformation("Extracted {numberOfClaims} claims.", claims.Count);
        return claims;
    }

    private List<Claim> ParseInputToClaims(string input)
    {
        var claims = new List<Claim>();
        var sentences = input.Split('\n').Select(s => s.Trim()).ToList();

        var claimTexts = new Dictionary<string, List<int>>();

        foreach (var sentence in sentences)
        {
            // Extract claim and reference IDs using regex
            var match = Regex.Match(sentence, @"^(.*?)\s*\[\d+\]$");
            if (match.Success)
            {
                var claimText = match.Groups[1].Value.Trim();
                var sentenceIdMatch = Regex.Match(sentence, @"\[(\d+)\]$");

                if (sentenceIdMatch.Success)
                {
                    var sentenceId = int.Parse(sentenceIdMatch.Groups[1].Value);

                    if (!claimTexts.ContainsKey(claimText))
                    {
                        claimTexts[claimText] = new List<int>();
                    }

                    claimTexts[claimText].Add(sentenceId);
                }
            }
        }

        // Convert dictionary to list of Claims and determine IsTriplet
        foreach (var kvp in claimTexts)
        {
            // Determine if the claim is in triplet format
            bool isTriplet = Regex.IsMatch(kvp.Key, @"^\(.*?,.*?,.*?\).*$");
            _logger.LogInformation(
                "Claim: {claimText}, Reference sentence IDs: {referenceSentenceIds}, IsTriplet: {isTriplet}",
                kvp.Key,
                string.Join(", ", kvp.Value),
                isTriplet
            );
            claims.Add(
                new Claim
                {
                    ExtractedClaim = kvp.Key,
                    ExtractedClaimReferenceSentenceIds = kvp.Value,
                    IsTriplet = isTriplet,
                    IsProcessed = false,
                }
            );
        }

        return claims;
    }

    /// <summary>
    /// Tokenizes the input text into sentences prefixed with a unique sentence ID.
    /// </summary>
    /// <param name="text">Text to tokenize.</param>
    /// <returns>sentences prefixed with a unique sentence ID.</returns>
    private static string TokenizeText(string text)
    {
        // Example below.
        // This paragraph:
        // Semantic Kernel is a lightweight, open-source development kit that lets you easily build AI agents and integrate the latest AI models into your C#, Python, or Java codebase.
        // It serves as an efficient middleware that enables rapid delivery of enterprise-grade solutions.
        // Is tokenized to:
        // [1] Semantic Kernel is a lightweight, open-source development kit that lets you easily build AI agents and integrate the latest AI models into your C#, Python, or Java codebase.
        // [2] It serves as an efficient middleware that enables rapid delivery of
        // enterprise-grade solutions.
        // The sentence IDs are prefixed in square brackets.
        var sentences = Regex.Split(text, @"(?<=[.!?])\s+");
        var tokenizedText = new StringBuilder();

        for (int i = 0; i < sentences.Length; i++)
        {
            tokenizedText.AppendLine($"[{i + 1}] {sentences[i].Trim()}");
        }

        return tokenizedText.ToString().Trim();
    }

    private async Task<string> GetClaimsWithKernel(
        Kernel evaluator,
        string tokenizedText,
        CancellationToken cancellationToken = default
    )
    {
        var contextVariables = new KernelArguments() { { "response", tokenizedText } };
        try
        {
            var result = await evaluator
                .InvokeAsync(
                    "ExtractorPlugins",
                    "ExtractTripletClaims",
                    contextVariables,
                    cancellationToken
                )
                .ConfigureAwait(false);
            string claimsList =
                result?.GetValue<string>()
                ?? throw new InvalidOperationException(
                    "The result from the kernel invocation is null."
                );
            if (string.IsNullOrWhiteSpace(claimsList))
            {
                throw new InvalidOperationException("The claims list is null or empty.");
            }
            return claimsList.Trim();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while extracting claims", ex);
        }
    }
}
