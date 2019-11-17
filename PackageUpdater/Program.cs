using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Xml;
using System.Xml.Linq;

namespace PackageUpdater
{
    class Program
    {
        private const string pathVarName = "PATH";

        static void Main(string[] args)
        {
            if (args.Length <= 0) // no input parameters? Must be wrong use of my excelent app or user hasn't added .exe directory to path environment variable
            {
                UpdatePathVariable();
            }
            else
            {
                // var solution = @"C:\Bitbucket\insysgo-sdk-mobile-clone\src";
                // var nuget = "insys.sdk";
                // var version = "5.4.334";
                var solution = Directory.GetCurrentDirectory();
                var (nuget, version) = args.Length >= 2 ? (args[0], args[1]) : GetArguments();
                UpdateSolution(solution, nuget, version);
            }
        }

        private static void UpdatePathVariable()
        {
            Console.WriteLine($"Add current location ({Directory.GetCurrentDirectory()}) to environment PATH variable? y/n");
            try
            {
                var input = Console.ReadLine();
                if (input.Length == 1 && input[0] == 'y')
                {
                    AddNewPath();
                }
                else if (input.Length == 1 && input[0] == 'n')
                {
                    Console.WriteLine("Terminating...");
                    Environment.Exit(0);
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (SecurityException ex)
            {
                Console.WriteLine($"Couln't complete request because: {ex.Message} Try open application with administrator privileges. Terminating...");
                Console.ReadLine();
            }
            catch (Exception)
            {
                Console.WriteLine($"Couln't complete request. Try type 'y' or 'n'");
                UpdatePathVariable();
            }
        }

        private static void AddNewPath()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var pathvar = Environment.GetEnvironmentVariable(pathVarName, EnvironmentVariableTarget.Machine);
            var variables = pathvar.Split(";");

            Console.WriteLine($"Current path variables:\n{string.Join("\n", variables.Where(v=>!string.IsNullOrWhiteSpace(v)))}\n");
        
            if (variables.Any(v => v == currentDir))
            {
                Console.WriteLine($"Variable {currentDir} is already added. Terminating...");
            }
            else
            {
                var value = pathvar + $@";{currentDir}";
                var target = EnvironmentVariableTarget.Machine; 
                Environment.SetEnvironmentVariable(pathVarName, value, target);
                Console.WriteLine($"Added {currentDir} to PATH. Terminating...");
            }
            Console.ReadLine();
        }

        private static (string, string) GetArguments()
        {
            Console.WriteLine("Input parmeters were wrong. Type package name and version e.g. insys.sdk 5.7.11");
            try
            {
                var input = Console.ReadLine();
                var parameters = input.Split(" ");
                return (parameters[0], parameters[1]);
            }
            catch (Exception)
            {
                Console.WriteLine("Wrong input format");
                return GetArguments();
            }
        }

        static void UpdateSolution(string solutionPath, string nuget, string version)
        {
            var solutionDir = new DirectoryInfo(solutionPath);
            var allSolutionfiles = solutionDir.GetDirectories().SelectMany(d => d.GetFiles());
            var allCsprojs = allSolutionfiles.Where(f => f.Name.Contains(".csproj"));
            if (allCsprojs.Any())
            {
                foreach (var item in allCsprojs)
                {
                    UpdateNuget(item.FullName, nuget, version);
                }
            }
            else
            {
                Console.WriteLine($"Could not found any .csproj files inside: {solutionPath}. Terminating...");
                Console.ReadLine();
            }
        }

        static void UpdateNuget(string filePath, string nugetPackage, string newVersion)
        {
            var csprojname = filePath.Split("\\").LastOrDefault();
            var document = XDocument.Load(filePath);
            var itemGroups = document.Elements().Elements().Where(e => e.Name.LocalName == "ItemGroup");
            var packageReferences = itemGroups.Elements().Where(e => e.Name.LocalName == "PackageReference");
            var nugets = packageReferences.Where(p => p.Attributes().Any(a => a.Value.ToLower().Contains(nugetPackage)));
            var updatedAnyNugets = false;

            foreach (var nuget in nugets)
            {
                updatedAnyNugets = true;
                var oldVersion = string.Empty;

                var versionAttribute = nuget.Attributes().FirstOrDefault(a => a.Name.LocalName == "Version"); // look for attribute in projects that targets .netstandard2.0
                if (versionAttribute != null)
                {
                    oldVersion = versionAttribute.Value;
                    versionAttribute.SetValue(newVersion);
                }
                else
                {
                    oldVersion = nuget.Value;
                    nuget.Elements().FirstOrDefault().SetValue(newVersion);
                }
                Console.WriteLine($"Updating {csprojname} {nugetPackage} from {oldVersion} to {newVersion}");
            }

            if (updatedAnyNugets)
            {
                var settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                settings.Indent = true;
                using var xmlWriter = XmlWriter.Create(filePath, settings);
                document.Save(xmlWriter);
            }
        }
    }
}
