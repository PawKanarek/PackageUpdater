using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Xml;
using System.Xml.Linq;

namespace PackageUpdater
{
    internal class Program
    {
        private static bool isFirstRun = true;
        private static readonly List<Input> consoleInput = new List<Input>()
        {
            new Input("-h", null, "Displays this help", _ => DisplayHelp()),
            new Input("-p", null, "Add Current location to environment variable PATH", _ => UpdatePathVariable()),
            new Input("-u", "[package_name] [new_version]", "Updates nuget packages to given version in current folder. e.g. '-u Xamarin.Forms 4.3.0.991211'.",
                (string [] args) => UpdateSolution(args))
        };

        private static void Main(string[] args)
        {
            ParseArguments(args);
        }

        private static void ParseArguments(string[] args)
        {
            if (args == null)
            {
                try
                {
                    args = Console.ReadLine().Split(" ");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not parse args. Reason: {ex.Message}");
                    return;
                }
            }

            var firstArg = args.FirstOrDefault();
            Input input = consoleInput?.FirstOrDefault(a => a.Parameter == firstArg);
            if (isFirstRun && firstArg == null)
            {
                isFirstRun = false;
                DisplayHelp();
            }
            else if (input != null) 
            {
                input.Action.Invoke(args);
            }
        }

        private static void DisplayHelp()
        {
            Console.WriteLine($"Usage:\n PackageUpdater [options] [package_name] [new version]\n\nOptions:");

            foreach (Input action in consoleInput)
            {
                Console.WriteLine(" " + string.Join(" ", new List<string> { action.Parameter, action.OptionalParameter, "->", action.Description }.Where(s => !string.IsNullOrWhiteSpace(s))));
            }
        }

        private static void UpdatePathVariable()
        {
            if (DirectoryIsAdded())
            {
                return;
            }

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
                    Environment.Exit(0);
                }
                else
                {
                    throw new ArgumentException($"{input} is not valid input parameter.");
                }
            }
            catch (SecurityException ex)
            {
                Console.WriteLine($"Couln't update PATH environment because: {ex.Message} Try open application with administrator privileges.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Couln't complete request because: {ex.Message}");
            }
        }

        private static bool DirectoryIsAdded()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var pathvar = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            var variables = pathvar.Split(";");
            var isAdded = variables.Any(v => v == currentDir);
            if (isAdded)
            {
                Console.WriteLine($"Location {currentDir} is already added to PATH.");
            }
            return isAdded;
        }

        private static void AddNewPath()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var pathvar = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            var variables = pathvar.Split(";");

            Console.WriteLine($"Current path variables:\n{string.Join("\n", variables.Where(v => !string.IsNullOrWhiteSpace(v)))}\n");

            if (variables.Any(v => v == currentDir))
            {
                Console.WriteLine($"Variable {currentDir} is already added.");
            }
            else
            {
                var value = pathvar + $@";{currentDir}";
                Environment.SetEnvironmentVariable("PATH", value, EnvironmentVariableTarget.Machine);
                Console.WriteLine($"Added {currentDir} to PATH.");
            }
        }

        private static void UpdateSolution(string[] args)
        {
            var solutionPath = Directory.GetCurrentDirectory();
            var nuget = args.Length > 1 ? args[1] : string.Empty;
            var version = args.Length > 2 ? args[2] : string.Empty;

            if (string.IsNullOrWhiteSpace(nuget) || string.IsNullOrWhiteSpace(version))
            {
                Console.WriteLine($"{string.Join(" ", args)} is not valid input parameter.");
                return;
            }

            var solutionDir = new DirectoryInfo(solutionPath);
            IEnumerable<FileInfo> allSolutionfiles = solutionDir.GetDirectories().SelectMany(d => d.GetFiles());
            IEnumerable<FileInfo> allCsprojs = allSolutionfiles.Where(f => f.Name.Contains(".csproj"));
            if (allCsprojs.Any())
            {
                foreach (FileInfo item in allCsprojs)
                {
                    UpdateNuget(item.FullName, nuget, version);
                }
            }
            else
            {
                Console.WriteLine($"Could not found any .csproj files inside: {solutionPath}");
            }
        }

        private static void UpdateNuget(string filePath, string nugetPackage, string newVersion)
        {
            var csprojname = filePath.Split("\\").LastOrDefault();
            var document = XDocument.Load(filePath);
            IEnumerable<XElement> itemGroups = document.Elements().Elements().Where(e => e.Name.LocalName == "ItemGroup");
            IEnumerable<XElement> packageReferences = itemGroups.Elements().Where(e => e.Name.LocalName == "PackageReference");
            IEnumerable<XElement> nugets = packageReferences.Where(p => p.Attributes().Any(a => a.Value.ToLower().Contains(nugetPackage)));
            var updatedAnyNugets = false;

            foreach (XElement nuget in nugets)
            {
                updatedAnyNugets = true;
                var oldVersion = string.Empty;

                XAttribute versionAttribute = nuget.Attributes().FirstOrDefault(a => a.Name.LocalName == "Version"); // look for attribute in projects that targets .netstandard2.0
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

    public class Input
    {
        public Input(string parameter, string optionalParameter, string description, Action<string[]> action)
        {
            this.Parameter = parameter;
            this.OptionalParameter = optionalParameter;
            this.Description = description;
            this.Action = action;
        }

        public string Parameter { get; set; }
        public string OptionalParameter { get; set; }
        public string Description { get; set; }
        public Action<string[]> Action { get; set; }
    }
}
