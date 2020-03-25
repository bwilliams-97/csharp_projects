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
        /// <summary>
        /// Adjacency list of code graph.
        /// </summary>
        private List<(SyntaxNode parentNode, SyntaxNode childNode)> _edges { get; set; }

        /// <summary>
        /// Mapping from syntax node to information about node.
        /// Doubles as store of nodes in graph.
        /// </summary>
        public Dictionary<SyntaxNode, NodeInformation> Vocabulary { get; private set; }

        /// <summary>
        /// Adjacency list of code graph with integer syntax node encodings.
        /// </summary>
        public HashSet<(int parentNode, int childNode)> EncodedEdges { get; private set; }

        /// <summary>
        /// Map from node syntax kind to a color for dot file rendering.
        /// </summary>
        public Dictionary<SyntaxKind, string> ColorMap { get; private set; }

        /// <summary>
        /// Track whether to add non-B/W color information to nodes.
        /// </summary>
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

        /// <summary>
        /// Add a node to the code graph.
        /// </summary>
        /// <param name="node"></param>
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
            // Remove quotation marks for dot string parsing.
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
                // Set node color to white background
                AddNodeTypeToColorMap(nodeType, "0.0 0.0 1.0");
            }  
        }

        /// <summary>
        /// Generate random color which is added to node color map.
        /// </summary>
        /// <param name="nodeType"></param>
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

        /// <summary>
        /// Add specified node color to colormap for specified node type.
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="nodeColor"></param>
        private void AddNodeTypeToColorMap(SyntaxKind nodeType, string nodeColor)
        {
            if (!ColorMap.ContainsKey(nodeType))
            {
                ColorMap.Add(nodeType, nodeColor);
            }
        }

        /// <summary>
        /// Add connection to code graph
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="childNode"></param>
        public void AddEdge(SyntaxNode parentNode, SyntaxNode childNode)
        {
            _edges.Add((parentNode, childNode));
        }

        /// <summary>
        /// Use vocabulary to map edges with SyntaxNode IDs to edges with int IDs.
        /// </summary>
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
        /// <summary>
        /// Integer encoding of syntax node.
        /// </summary>
        public int Encoding { get; private set; }
        /// <summary>
        /// String representation of syntax node.
        /// </summary>
        public string NodeName { get; private set; }
        /// <summary>
        /// Type of node.
        /// </summary>
        public SyntaxKind NodeType { get; private set; }
        /// <summary>
        /// File in which syntax node can be found.
        /// </summary>
        public string FileName { get; private set; }
        /// <summary>
        /// Location in file of syntax node.
        /// </summary>
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
