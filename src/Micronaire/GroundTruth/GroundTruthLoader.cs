// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace Micronaire.GroundTruth;

static class GroundTruthLoader
{
    /// <summary>
    /// Loads the question and answer dataset from the given path.
    /// </summary>
    /// <param name="path">The path to the question and answer dataset.</param>
    /// <returns>An enumerable of tuples containing the query and the answer.</returns>
    public static IEnumerable<(string Query, string Answer)> LoadQADataSet(string path)
    {
        string fullpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        Console.WriteLine($"Loading ground truth from {fullpath}");
        var json = File.ReadAllText(fullpath);
        Console.WriteLine($"json: {json}");
        var queriesAndAnswers =
            JsonConvert.DeserializeObject<List<QuestionAndAnswer>>(json)
            ?? throw new Exception("Deserialization failed. Please check the input file.");
        foreach (var item in queriesAndAnswers)
        {
            yield return (item.Question, item.Answer);
        }
    }
}
