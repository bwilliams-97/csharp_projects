# CodeGraphExtractor

This project reads in a C# solution and extracts graph representations of the code to be written to file.

## Files
- [Program](Program.cs) Contains Main() function for project.
- [CSharpSolutionBuilder](CSharpSolutionBuilder.cs) Build selected solution and apply relevant extraction function.
- [CodeGraph](Utils/CodeGraph.cs) Generic classes for graph and node structures.
- [GraphWriter](GraphWriter.cs) Write graph object to file using Graphviz.
- [ASTVisitor](ASTVisitor.cs) Based on CSharpSyntaxWalker, visit all nodes in AST and store.
- [CallGraphVisitor](CallGraphVisitor.cs) Based on CSharpSyntaxWalker, visit all method declaration and invocation nodes and store.

## Graphviz.NET
GraphWriter uses the Nuget Graphviz.NET package as a wrapper for graphviz. To use this package the following lines must be added to [App.config](App.config):

<appSettings>
    <add key="graphVizLocation" value=[graphvizLocation]/>
</appSettings>

where graphvizLocation is the local directory containing the [graphviz binary files](https://graphviz.gitlab.io/_pages/Download).
