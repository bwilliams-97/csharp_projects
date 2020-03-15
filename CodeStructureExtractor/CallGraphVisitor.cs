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

        private SyntaxTree _syntaxTree;

        private CallGraphVisitor(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            _syntaxTree = syntaxTree;
            _callGraph = new CodeGraph(semanticModel);
        }

        public static CodeGraph ExtractCallGraph(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
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
            while(!(parentNode is BaseMethodDeclarationSyntax))
            {
                parentNode = parentNode.Parent;
            }
            _callGraph.AddEdge(parentNode, node);
            base.VisitInvocationExpression(node);
        }
    }
}
