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

        public HashSet<(int parentNode, int childNode)> EncodedEdges { get; private set; }

        public Dictionary<SyntaxKind, string> ColorMap { get; private set; }

        public bool ColorNodes { get; private set; }

        private SemanticModel _semanticModel { get; set; }

        private Random _random { get; set; }

        public CodeGraph(SemanticModel semanticModel, bool colorNodes)
        {
            Vocabulary = new Dictionary<SyntaxNode, NodeInformation>();
            _edges = new List<(SyntaxNode parentNode, SyntaxNode childNode)>();
            EncodedEdges = new HashSet<(int parentNode, int childNode)>();

            _semanticModel = semanticModel;

            _random = new Random();

            ColorNodes = colorNodes;

            ColorMap = new Dictionary<SyntaxKind, string>();
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

            // TODO: find a better way of removing quotation marks.
            nodeName = nodeName.Replace("\"", "%");

            // Convert from .ctor to className
            if(nodeName == ".ctor")
            {
                string nodeClassName = _semanticModel.GetDeclaredSymbol(node).ContainingType.Name.ToString();
                nodeName = nodeClassName.Split('.').Last();
            }
            SyntaxKind nodeType = node.Kind();
            string fileName = node.SyntaxTree.FilePath.ToString();
            int lineNumber = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            
            Vocabulary.Add(
                node,
                new NodeInformation(Vocabulary.Count, nodeName, nodeType, fileName, lineNumber)
                );

            if (ColorNodes)
            {
                AddNodeTypeToColorMap(nodeType);
            }
            else
            {
                AddNodeTypeToColorMap(nodeType, "0.0 0.0 1.0");
            }  
        }

        private void AddNodeTypeToColorMap(SyntaxKind nodeType)
        {
            if (!ColorMap.ContainsKey(nodeType))
            {
                string hue = string.Format("{0:0.000}", _random.NextDouble());
                string saturation = string.Format("{0:0.000}", (0.6*_random.NextDouble()));
                string value = string.Format("{0:0.000}", (1 - 0.5 * _random.NextDouble()));
                string hsvColor = String.Join(" ", new string[]{
                    hue,
                    saturation,
                    value
                    });
                ColorMap.Add(nodeType, hsvColor);
            }
        }

        private void AddNodeTypeToColorMap(SyntaxKind nodeType, string nodeColor)
        {
            if (!ColorMap.ContainsKey(nodeType))
            {
                ColorMap.Add(nodeType, nodeColor);
            }
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

        public SyntaxKind NodeType { get; private set; }

        public string FileName { get; private set; }

        public int LineNumber { get; private set; }

        public NodeInformation(
            int nodeEncoding,
            string nodeName, 
            SyntaxKind nodeType,
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
