using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Drawing;
using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;

namespace CodeStructureExtractor
{
    // Should take a graph of form node [node index, node label, node type] and edges [(parent, child)]
    // and write to (a) json.gz, (b) image output. Should also be able to read a json.gz graph into 
    // an object and write this to image format.
    class GraphWriter
    {
        // These three instances can be injected via the IGetStartProcessQuery, 
        //                                               IGetProcessStartInfoQuery and 
        //                                               IRegisterLayoutPluginCommand interfaces
        public static void GenerateDotGraph(CodeGraph codeGraph, string outputFileName)
        {
            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);

            // GraphGeneration can be injected via the IGraphGeneration interface

            var wrapper = new GraphGeneration(getStartProcessQuery,
                                              getProcessStartInfoQuery,
                                              registerLayoutPluginCommand);

            string graphDotString = GenerateDotString(codeGraph);
            byte[] output = wrapper.GenerateGraph(graphDotString, Enums.GraphReturnType.Png);
            WriteGraphToImageFile(output, outputFileName);
        }

        public static string GenerateDotString(CodeGraph codeGraph)
        {
            StringBuilder dotString = new StringBuilder();

            dotString.Append("digraph {");

            foreach(var node in codeGraph.Vocabulary)
            {
                NodeInformation nodeInfo = node.Value;
                int nodeLabel = nodeInfo.Encoding;
                string nodeName = nodeInfo.NodeName;

                dotString.Append($"{nodeLabel} [label={nodeName}];");
            }

            foreach(var edge in codeGraph.EncodedEdges)
            {
                dotString.Append($"{edge.parentNode} -> {edge.childNode};");
            }
            dotString.Append("}");

            return dotString.ToString();
        }

        public static Image ConvertByteArrayToImage(byte[] byteArrayIn)
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
    }
}
