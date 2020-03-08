using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeStructureExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 3)
            {
                args = new string[3];
                args[0] = "";
                args[1] = "";
                args[3] = "";
            }

            string baseDir = args[0];
            string solutionPath = args[1];
            string outputDir = args[2];
        }
    }
}
