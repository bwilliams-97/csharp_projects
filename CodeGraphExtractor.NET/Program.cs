using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeStructureExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 3)
            {
                args = new string[3];
                args[0] = @"C:\Users\t-bewill\Documents\Project_1\AIResidency_PredictiveProfiling\syntax-tree-parser\TestProject";
                args[1] = @"C:\Users\t-bewill\Documents\Project_1\AIResidency_PredictiveProfiling\syntax-tree-parser\TestProject\TestProject.sln";
                args[2] = @"C:\Users\t-bewill\Documents\Personal experiments\graphs";
            }

            string baseDir = args[0];
            string solutionPath = args[1];
            string outputDir = args[2];

            void ExploreSolution(SyntaxTree syntaxTree, SemanticModel semanticModel)
            {
                string fileName = syntaxTree.FilePath.Split('\\').Last();

                string saveDir = Path.Combine(outputDir, fileName);
                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }

                var callGraph = CallGraphVisitor.ExtractCallGraph(syntaxTree, semanticModel);
                string callGraphOutputPath = Path.Combine(saveDir, "callGraph.png");
                GraphWriter.GenerateDotGraph(callGraph, callGraphOutputPath);

                var ast = ASTVisitor.ExtractAST(syntaxTree, semanticModel);
                string astOutputPath = Path.Combine(saveDir, "ast.png");
                GraphWriter.GenerateDotGraph(ast, astOutputPath);
            }

            CSharpSolutionBuilder.BuildSolution(solutionPath, ExploreSolution);
        }
    }
}
