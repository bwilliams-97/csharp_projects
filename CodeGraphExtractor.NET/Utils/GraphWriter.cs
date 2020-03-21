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
        public static void GenerateDotGraph(CodeGraph codeGraph, string outputFileName)
        {
            Console.WriteLine($"Writing code graph to {outputFileName}");

            GraphGeneration wrapper = GetGraphvizWrapper();

            string graphDotString = ConvertGraphToDotString(codeGraph);
            byte[] output = wrapper.GenerateGraph(graphDotString, Enums.GraphReturnType.Png);
            WriteGraphToImageFile(output, outputFileName);
        }

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

            foreach(var node in codeGraph.Vocabulary)
            {
                NodeInformation nodeInfo = node.Value;
                int nodeLabel = nodeInfo.Encoding;
                string nodeName = nodeInfo.NodeName;
                string nodeColor = codeGraph.ColorMap[nodeInfo.NodeType];
                string nodeEntry = $"{nodeLabel} [label=\"{nodeName}\" style=filled fillcolor=\"{nodeColor}\"];";
                dotString.Append(nodeEntry);
            }

            foreach(var edge in codeGraph.EncodedEdges)
            {
                dotString.Append($"{edge.parentNode} -> {edge.childNode};");
            }

            dotString = AddKeyToDotString(dotString, codeGraph);

            // End line of dot string
            dotString.Append("}");

            return dotString.ToString();
        }

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
