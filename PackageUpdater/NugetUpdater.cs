using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace PackageUpdater
{
    public class NugetUpdater
    {
        public void UpdateSolution(string[] args)
        {
            var solutionPath = Directory.GetCurrentDirectory();
            var nuget = args.Length > 1 ? args[1] : string.Empty;
            var version = args.Length > 2 ? args[2] : string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(nuget) || string.IsNullOrWhiteSpace(version))
                {
                    throw new ArgumentException($"{string.Join(" ", args)} is not valid input parameter.");
                }

                var solutionDir = new DirectoryInfo(solutionPath);
                var allSolutionfiles = solutionDir.GetDirectories().SelectMany(d => d.GetFiles());
                var allCsprojs = allSolutionfiles.Where(f => f.Name.Contains(".csproj"));
                if (allCsprojs.Any())
                {
                    var updatedAnyNuget = false;
                    foreach (var item in allCsprojs)
                    {
                        if (this.UpdateNuget(item.FullName, nuget, version))
                        {
                            updatedAnyNuget = true;
                        }
                    }

                    if (!updatedAnyNuget)
                    {
                        throw new ArgumentException($"There is no package reference that contains {nuget} in {solutionPath}");
                    }
                }
                else
                {
                    throw new ArgumentException($"There is no .csproj in {solutionPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not complete request because: {ex.Message}");
            }
        }

        public bool UpdateNuget(string filePath, string nugetPackage, string newVersion)
        {
            var csprojname = filePath.Split("\\").LastOrDefault();
            var document = XDocument.Load(filePath);
            var itemGroups = document.Elements().Elements().Where(e => e.Name.LocalName.Equals("ItemGroup", StringComparison.OrdinalIgnoreCase));
            var packageReferences = itemGroups.Elements().Where(e => e.Name.LocalName.Equals("PackageReference", StringComparison.OrdinalIgnoreCase));
            var nugets = packageReferences.Where(p => p.Attributes().Any(a => a.Value.Contains(nugetPackage, StringComparison.OrdinalIgnoreCase)));
            var updatedAnyNugets = false;

            foreach (var nuget in nugets)
            {
                updatedAnyNugets = true;
                var oldVersion = string.Empty;

                // look for attribute in projects that targets .netstandard2.0
                var versionAttribute = nuget.Attributes().FirstOrDefault(a => a.Name.LocalName.Equals("Version", StringComparison.OrdinalIgnoreCase));
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
            return updatedAnyNugets;
        }
    }
}