using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace CodeStructureExtractor
{
    class CallGraphVisitor : CSharpSyntaxWalker
    {
        private CodeGraph _callGraph;

        private SemanticModel _semanticModel;

        private CallGraphVisitor(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            _callGraph = new CodeGraph(semanticModel, false);
            _semanticModel = semanticModel;
        }

        /// <summary>
        /// Visit all method declaration nodes and invocation nodes. Add nodes to call graph (CodeGraph) object.
        /// </summary>
        /// <param name="syntaxTree"></param>
        /// <param name="semanticModel"></param>
        /// <returns></returns>
        public static CodeGraph ExtractCallGraph(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            Console.WriteLine($"Extracting call graph from {syntaxTree.FilePath}");
            var graphVisitor = new CallGraphVisitor(syntaxTree, semanticModel);

            graphVisitor.Visit(syntaxTree.GetRoot());

            graphVisitor._callGraph.ConvertEdgesToEncodedEdges();

            return graphVisitor._callGraph;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            _callGraph.AddNode(node);
            base.VisitMethodDeclaration(node);
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            _callGraph.AddNode(node);
            base.VisitConstructorDeclaration(node);
        }

        public override void VisitDestructorDeclaration(DestructorDeclarationSyntax node)
        {
            _callGraph.AddNode(node);
            base.VisitDestructorDeclaration(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            SyntaxNode parentNode = node.Parent;
            // TODO: deal with other cases (local functions, accessors, anonymous functions).
            // Move up the syntax tree until method declaration (parent) is found.
            while(!(parentNode is BaseMethodDeclarationSyntax))
            {
                parentNode = parentNode.Parent;
            }
            // TODO: deal with cases where we can't match back to original declaration (e.g. system calls).
            var originalMethodSymbol = _semanticModel.GetSymbolInfo(node).Symbol;
            var syntaxReference = originalMethodSymbol.DeclaringSyntaxReferences.FirstOrDefault();
            if(syntaxReference != null)
            {
                // Will return true only if we can match back to original function declaration.
                var originalInvocationDeclaration = syntaxReference.GetSyntax();
                _callGraph.AddEdge(parentNode, originalInvocationDeclaration);
            }
            
            base.VisitInvocationExpression(node);
        }
    }
}
