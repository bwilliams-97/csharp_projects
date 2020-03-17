using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace CodeStructureExtractor
{
    class CodeGraph
    {
        private List<(SyntaxNode parentNode, SyntaxNode childNode)> _edges { get; set; }

        public Dictionary<SyntaxNode, NodeInformation> Vocabulary { get; private set; }

        public List<(int parentNode, int childNode)> EncodedEdges { get; private set; }

        private SemanticModel _semanticModel { get; set; }

        public CodeGraph(SemanticModel semanticModel)
        {
            Vocabulary = new Dictionary<SyntaxNode, NodeInformation>();
            _edges = new List<(SyntaxNode parentNode, SyntaxNode childNode)>();
            EncodedEdges = new List<(int parentNode, int childNode)>();

            _semanticModel = semanticModel;
        }

        public void AddNode(SyntaxNode node)
        {
            ISymbol nodeSymbol = _semanticModel.GetDeclaredSymbol(node);
            string nodeName;
            if(nodeSymbol != null)
            {
                nodeName = _semanticModel.GetDeclaredSymbol(node).Name.ToString();
            }
            else
            {
                nodeName = node.ToString();
            }

            nodeName = nodeName.Replace("\"", "");

            // Convert from .ctor to className
            if(nodeName == ".ctor")
            {
                string nodeClassName = _semanticModel.GetDeclaredSymbol(node).ContainingType.Name.ToString();
                nodeName = nodeClassName.Split('.').Last();
            }
            string nodeType = node.Kind().ToString();
            string fileName = node.SyntaxTree.FilePath.ToString();
            int lineNumber = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            
            Vocabulary.Add(
                node,
                new NodeInformation(Vocabulary.Count, nodeName, nodeType, fileName, lineNumber)
                );
        }

        public void AddEdge(SyntaxNode parentNode, SyntaxNode childNode)
        {
            _edges.Add((parentNode, childNode));
        }

        public void ConvertEdgesToEncodedEdges()
        {
            foreach(var edge in _edges)
            {
                EncodedEdges.Add((Vocabulary[edge.parentNode].Encoding, Vocabulary[edge.childNode].Encoding));
            }
        }
    }

    class NodeInformation
    {
        public int Encoding { get; private set; }
        public string NodeName { get; private set; }

        public string NodeType { get; private set; }

        public string FileName { get; private set; }

        public int LineNumber { get; private set; }

        public NodeInformation(
            int nodeEncoding,
            string nodeName, 
            string nodeType,
            string fileName,
            int lineNumber
            )
        {
            Encoding = nodeEncoding;
            NodeName = nodeName;
            NodeType = nodeType;
            FileName = fileName;
            LineNumber = lineNumber;
        }
    }
}
