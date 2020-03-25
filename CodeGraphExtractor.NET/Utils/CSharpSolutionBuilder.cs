using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace CodeStructureExtractor
{
    class CSharpSolutionBuilder
    {
        /// <summary>
        /// Build solution and call specified action on syntax tree and semantic model of a particular file.
        /// </summary>
        /// <param name="solutionPath"></param>
        /// <param name="solutionExplorer"></param>
        public static void BuildSolution(string solutionPath, Action<SyntaxTree, SemanticModel> solutionExplorer)
        {
            var workspace = CreateMSBuildWorkspace();

            Solution solution = OpenSolution(workspace, solutionPath);

            // Set Parser options
            CSharpParseOptions options = CSharpParseOptions.Default
           .WithFeatures(new[] { new KeyValuePair<string, string>("flow-analysis", "") });

            var seenProjects = new ConcurrentDictionary<string, bool>();

            Microsoft.CodeAnalysis.ProjectDependencyGraph projectGraph = solution.GetProjectDependencyGraph();
            foreach (var projectId in projectGraph.GetTopologicallySortedProjects())
            {

                if (!seenProjects.TryAdd(solution.GetProject(projectId).FilePath, true))
                {
                    continue;
                }
                Compilation projectCompilation;
                try
                {
                    var solutionWithOptions = solution.WithProjectParseOptions(projectId, options);

                    // Get project compilation
                    projectCompilation = GetCompilation(solutionWithOptions, projectId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception occured while compiling project {projectId}. {ex.Message}: {ex.StackTrace}");
                    continue;
                }
                switch (projectCompilation)
                {
                    case CSharpCompilation cSharpCompilation:

                        foreach (SyntaxTree syntaxTree in projectCompilation.SyntaxTrees)
                        {
                            var semanticModel = projectCompilation.GetSemanticModel(syntaxTree);

                            solutionExplorer(syntaxTree, semanticModel);
                        }
                        break;
                }
            }
        }

        public static MSBuildWorkspace CreateMSBuildWorkspace()
        {
            try
            {
                MSBuildLocator.RegisterDefaults();
            }
            catch
            {
                Console.WriteLine("Workspace defaults already registered");
            }

            MSBuildWorkspace workspace = MSBuildWorkspace.Create(new Dictionary<string, string> { { "DebugSymbols", "False" } });

            workspace.WorkspaceFailed += (e, d) => Console.WriteLine(e.ToString() + ":" + d.ToString());
            return workspace;
        }

        public static Solution OpenSolution(MSBuildWorkspace workspace, string solutionLocation = "")
        {
            Microsoft.CodeAnalysis.Solution solution = workspace.OpenSolutionAsync(solutionLocation).Result;
            Console.WriteLine(String.Format("\nOpening solution: {0}\n", solutionLocation));
            return solution;
        }

        public static Compilation GetCompilation(Solution solutionWithOptions, ProjectId projectId)
        {
            // Compilation is representation of single invocation of compiler
            Microsoft.CodeAnalysis.Compilation compilation = solutionWithOptions.GetProject(projectId).GetCompilationAsync().Result;
            return compilation;
        }
    }
}
