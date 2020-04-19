using System.Collections.Generic;

namespace PackageUpdater
{
    internal class Program
    {
        private static readonly EnvironmentVariablesManager EnvironmentVariablesManager = new EnvironmentVariablesManager();
        private static readonly NugetUpdater NugetUpdater = new NugetUpdater();

        private static readonly List<InputParameter> ConsoleParameters = new List<InputParameter>()
        {
            new InputParameter("-p", null, "Add Current location to environment variable PATH (Windows Only)", _ => EnvironmentVariablesManager.UpdatePathVariable()),
            new InputParameter("-u", "[package_name] [new_version]", "Updates versions of nuget packages in current folder. e.g. '-u Xamarin.Forms 4.3.0.991211'."
                + " Where [packagename] is nuget package name reference (Program will use .Contains([packagename])),"
                + " [new_version] new version to replace current."
                , (string[] args) => NugetUpdater.UpdateSolution(args))
        };

        private static void Main(string[] args)
        {
            var consoleManager = new ConsoleManager(ConsoleParameters);
            consoleManager.ParseArguments(args);
        }
    }
}