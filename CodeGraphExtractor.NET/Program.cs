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
                // Alternative to set default arguments here.
                args = new string[3];
                args[0] = @"";
                args[1] = @"";
                args[2] = @"";
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

                // Extract call graph and write to file.
                var callGraph = CallGraphVisitor.ExtractCallGraph(syntaxTree, semanticModel);
                string callGraphOutputPath = Path.Combine(saveDir, "callGraph.png");
                GraphWriter.GenerateDotGraph(callGraph, callGraphOutputPath);

                // Extract AST and write to file.
                var ast = ASTVisitor.ExtractAST(syntaxTree, semanticModel);
                string astOutputPath = Path.Combine(saveDir, "ast.png");
                GraphWriter.GenerateDotGraph(ast, astOutputPath);
            }

            CSharpSolutionBuilder.BuildSolution(solutionPath, ExploreSolution);
        }
    }
}
