using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            var solutionDir = Directory.GetCurrentDirectory();

#if DEBUG
            var solution = @"C:\Users\Raix\Documents\insysgo-sdk-mobile\src";
            var nuget = "ipott.sdk";
            var version = "5.4.334";
#else
            var solution = Directory.GetCurrentDirectory();
            var (nuget, version) = GetArgument();
#endif

            UpdateSolution(solution, nuget, version);

            Console.WriteLine("Press any key to terminate");
            Console.ReadKey();
        }

        private static (string, string) GetArgument()
        {
            Console.WriteLine("Type package name and version e.g. insys.sdk 5.7.11");
            try
            {
                var input = Console.ReadLine();
                var parameters = input.Split(" ");
                return (parameters[0], parameters[1]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wrong input format");
                return GetArgument();
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
                Console.WriteLine($"Could not found any .csproj files inside: {solutionPath}");
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
            else
            {
                Console.WriteLine($"Could not found '{nugetPackage}' in {csprojname}");
            }
        }
    }
}
