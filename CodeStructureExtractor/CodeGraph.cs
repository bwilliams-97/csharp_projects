using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeStructureExtractor
{
    class CodeGraph
    {
        public List<(int parentNode, int childNode)> Edges { get; private set; }

        public Dictionary<int, NodeInformation> Nodes { get; private set; }

        private SemanticModel _semanticModel { get; set; }

        public CodeGraph(SemanticModel semanticModel)
        {
            Nodes = new Dictionary<int, NodeInformation>();
            Edges = new List<(int parentNode, int childNode)>();

            _semanticModel = semanticModel;
        }

        public void AddNode(SyntaxNode node)
        {
            string nodeName =;// GET NODE NAME FROM SEMANTIC MODEL
            string nodeType = node.Kind().ToString();
            string fileName = node.SyntaxTree.FilePath.ToString();
            int lineNumber = 0;
            
            Nodes.Add(
                Nodes.Count,
                new NodeInformation(nodeName, nodeType, fileName, lineNumber)
                );
        }

        public void AddEdge ()
    }

    class NodeInformation
    {
        public string NodeName { get; private set; }

        public string NodeType { get; private set; }

        public string FileName { get; private set; }

        public int LineNumber { get; private set; }

        public NodeInformation(
            string nodeName, 
            string nodeType,
            string fileName,
            int lineNumber
            )
        {
            NodeName = nodeName;
            NodeType = nodeType;
            FileName = fileName;
            LineNumber = lineNumber;
        }
    }
}
