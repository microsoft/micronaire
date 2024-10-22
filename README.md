<p align="center"><img src='https://github.com/user-attachments/assets/36a76139-89e6-4d5e-8c25-15d5bff895be' width='200px' /> </p>

# Micronaire

RAG pipelines enable developers to augment their chat experiences with informational documents that their agents can leverage to provide better answers. Evaluating these pipelines is a new area of study, with solutions like [RAGChecker](https://github.com/amazon-science/RAGChecker) and [RAGAS](https://github.com/explodinggradients/ragas) being state of the art frameworks that are implemented in Python. This project aims to take these ideas and bring them to DotNet through Semantic Kernel.

Micronaire brings actionable metrics to RAG pipeline evaluation by taking a set of ground truth questions and answers as well as a RAG pipeline, evaluating the pipeline against the ground truth using our metrics (see below), and then producing an evaluation report.

## Packaging

Before packaging, ensure that the version of the package has been bumped according to [Semantic Versioning guidelines](https://semver.org/). This can be automated in the future.

To package the project, run the following command:

```powershell
dotnet pack .\src\Micronaire\Micronaire.csproj
```

This will generate a NuGet package in the `bin` directory. This package can then be uploaded to [NuGet](nuget.org)!

In the future, this will be automated through a GitHub action.

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you
to agree to a Contributor License Agreement (CLA) declaring that you have the right
to, and actually do, grant us the rights to use your contribution. For details,
visit <https://cla.opensource.microsoft.com>.

When you submit a pull request, a CLA bot will automatically determine whether you
need to provide a CLA and decorate the PR appropriately (e.g., status check,
comment). Simply follow the instructions provided by the bot. You will only need to
do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the
[Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional
questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services.
Authorized use of Microsoft trademarks or logos is subject to and must follow
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not
cause confusion or imply Microsoft sponsorship. Any use of third-party trademarks
or logos are subject to those third-party's policies.
