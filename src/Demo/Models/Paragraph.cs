// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Microsoft.Extensions.VectorData;

namespace Demo;

public class Paragraph
{
    [VectorStoreRecordKey]
    public required Guid ParagraphId { get; set; }

    [VectorStoreRecordData(IsFullTextSearchable = true)]
    public required string Text { get; set; }

    [VectorStoreRecordVector(Dimensions: 1536)]
    public ReadOnlyMemory<float> Embedding { get; set; }
}
