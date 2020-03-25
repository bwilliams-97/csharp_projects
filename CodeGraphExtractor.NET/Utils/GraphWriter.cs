using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Drawing;
using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using Newtonsoft.Json;
using System.Linq;
using System.Configuration;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

namespace CodeStructureExtractor
{
    class GraphWriter
    {
        /// <summary>
        /// Generate dot string that represents graph and write to file.
        /// </summary>
        /// <param name="codeGraph"></param>
        /// <param name="outputFileName"></param>
        public static void GenerateDotGraph(CodeGraph codeGraph, string outputFileName)
        {
            Console.WriteLine($"Writing code graph to {outputFileName}");

            GraphGeneration wrapper = GetGraphvizWrapper();

            string graphDotString = ConvertGraphToDotString(codeGraph);
            byte[] output = wrapper.GenerateGraph(graphDotString, Enums.GraphReturnType.Png);
            WriteGraphToImageFile(output, outputFileName);
        }

        /// <summary>
        /// Set up graphviz wrapper to generate graphviz graph with.
        /// </summary>
        /// <returns></returns>
        private static GraphGeneration GetGraphvizWrapper()
        {
            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);

            // GraphGeneration can be injected via the IGraphGeneration interface

            var wrapper = new GraphGeneration(getStartProcessQuery,
                                              getProcessStartInfoQuery,
                                              registerLayoutPluginCommand);

            return wrapper;
        }

        public static string ConvertGraphToDotString(CodeGraph codeGraph)
        {
            StringBuilder dotString = new StringBuilder();

            // Start line of dot string
            dotString.Append("digraph {");

            // Add nodes to dot string
            foreach(var node in codeGraph.Vocabulary)
            {
                NodeInformation nodeInfo = node.Value;
                int nodeLabel = nodeInfo.Encoding;
                string nodeName = nodeInfo.NodeName;
                string nodeColor = codeGraph.ColorMap[nodeInfo.NodeType];

                string nodeEntry = $"{nodeLabel} [label=\"{nodeName}\" style=filled fillcolor=\"{nodeColor}\"];";
                dotString.Append(nodeEntry);
            }

            // Add edges to dot string
            foreach(var edge in codeGraph.EncodedEdges)
            {
                dotString.Append($"{edge.parentNode} -> {edge.childNode};");
            }

            // TO DO: find better way of showing key on output image.
            // Add key (for colormap) to dot string.
            dotString = AddKeyToDotString(dotString, codeGraph);

            // End line of dot string
            dotString.Append("}");

            return dotString.ToString();
        }

        /// <summary>
        /// Add unconnected nodes to dot string. Each node has label of syntax node type
        /// and color of associated color in color map.
        /// </summary>
        /// <param name="dotString"></param>
        /// <param name="codeGraph"></param>
        /// <returns></returns>
        private static StringBuilder AddKeyToDotString(StringBuilder dotString, CodeGraph codeGraph)
        {
            if (codeGraph.ColorNodes)
            {
                int keyIndex = -1;
                foreach(KeyValuePair<SyntaxKind, string> colorMapping in codeGraph.ColorMap)
                {
                    string syntaxKey = colorMapping.Key.ToString();
                    string hsvColor = colorMapping.Value;
                    dotString.Append($"{keyIndex} [label=\"{syntaxKey}\" style=filled shape=box fillcolor=\"{hsvColor}\"];");
                    keyIndex--;
                }
            }
            return dotString;
        }

        private static Image ConvertByteArrayToImage(byte[] byteArrayIn)
        {
            using (MemoryStream memoryStream = new MemoryStream(byteArrayIn))
            {
                return Image.FromStream(memoryStream);
            }
        }

        public static void WriteGraphToImageFile(byte[] graphArray, string outputFileName)
        {
            var image = ConvertByteArrayToImage(graphArray);

            image.Save(outputFileName, System.Drawing.Imaging.ImageFormat.Png);
        }

        public static string ConvertGraphToJsonString(CodeGraph codeGraph)
        {
            string codeGraphString = JsonConvert.SerializeObject(codeGraph);
            return codeGraphString;
        }

        public static void WriteGraphToJsonGzFile(CodeGraph codeGraph, string outputJsonFileName)
        {
            string jsonString = ConvertGraphToJsonString(codeGraph);
            using (FileStream fileStream = File.Create(outputJsonFileName))
            {
                using(GZipStream gzipStream = new GZipStream(fileStream, CompressionLevel.Fastest))
                {
                    using (StreamWriter textStream = new StreamWriter(gzipStream))
                    {
                        textStream.Write(jsonString);
                    }
                }
            }
        }

        public static CodeGraph ReadGraphFromJsonGzFile(string inputFileName)
        {
            using (FileStream fileStream = File.OpenRead(inputFileName))
            {
                using (GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
                {
                    using (StreamReader unzip = new StreamReader(gzipStream))
                    {
                        string codeGraphString = unzip.ReadLine();
                        CodeGraph codeGraph = (CodeGraph)JsonConvert.DeserializeObject(codeGraphString);

                        return codeGraph;
                    }
                }
            }
        }
    }
}
