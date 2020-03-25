using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace CodeStructureExtractor
{
    class ASTVisitor : CSharpSyntaxWalker
    {
        private CodeGraph _syntaxGraph;

        private SyntaxTree _syntaxTree;

        /// <summary>
        /// Visit all nodes in syntax tree of a file. Add nodes to syntax graph (CodeGraph) object.
        /// </summary>
        /// <param name="syntaxTree"></param>
        /// <param name="semanticModel"></param>
        /// <returns></returns>
        public static CodeGraph ExtractAST(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            Console.WriteLine($"Extracting AST from {syntaxTree.FilePath}");
            ASTVisitor astVisitor = new ASTVisitor(syntaxTree, semanticModel);

            astVisitor.Visit(syntaxTree.GetRoot());

            astVisitor._syntaxGraph.ConvertEdgesToEncodedEdges();

            return astVisitor._syntaxGraph;
        }

        private ASTVisitor(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            _syntaxTree = syntaxTree;

            _syntaxGraph = new CodeGraph(semanticModel, true);
        }

        public override void Visit(SyntaxNode node)
        {
            _syntaxGraph.AddNode(node);
            if(!(node == _syntaxTree.GetRoot()))
            {
                _syntaxGraph.AddEdge(node.Parent, node);
            }
            
            base.Visit(node);
        }
    }
}
