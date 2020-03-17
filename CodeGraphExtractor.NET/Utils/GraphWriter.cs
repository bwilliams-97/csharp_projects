using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
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
        public static void GenerateDotGraph()
        {
            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);

            // GraphGeneration can be injected via the IGraphGeneration interface

            var wrapper = new GraphGeneration(getStartProcessQuery,
                                              getProcessStartInfoQuery,
                                              registerLayoutPluginCommand);

            byte[] output = wrapper.GenerateGraph("digraph{a -> b; b -> c; c -> a;}", Enums.GraphReturnType.Png);
        }

        public static void WriteGraphToFile(byte[] graphArray)
        {

        }
    }
}
