using System;
using System.Collections.Generic;
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
                args[2] = @"C:\Users\t-bewill\Documents\Personal experiments";
            }

            string baseDir = args[0];
            string solutionPath = args[1];
            string outputDir = args[2];

            void ExploreSolution(SyntaxTree syntaxTree, SemanticModel semanticModel)
            {
                var callGraph = CallGraphVisitor.ExtractCallGraph(syntaxTree, semanticModel);
                var ast = ASTVisitor.ExtractAST(syntaxTree, semanticModel);

                GraphWriter.GenerateDotGraph(callGraph, @"..\..\file.png");
            }

            CSharpSolutionBuilder.BuildSolution(solutionPath, ExploreSolution);
        }
    }
}
