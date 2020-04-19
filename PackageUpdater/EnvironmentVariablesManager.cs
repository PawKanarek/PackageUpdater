using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;

namespace PackageUpdater
{
    public class EnvironmentVariablesManager
    {
        private readonly char PathSeparator = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? ':' : ';';

        public void UpdatePathVariable()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("Option available only on Windows");
                return;
            }

            if (this.DirectoryIsAdded())
            {
                return;
            }

            Console.WriteLine($"Add current location ({Directory.GetCurrentDirectory()}) to environment PATH variable? y/n");
            try
            {
                var input = Console.ReadLine();
                if (input.Length == 1 && input[0] == 'y')
                {
                    this.AddNewPath();
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
                Console.WriteLine($"Could not update PATH environment because: {ex.Message} Try open application with administrator privileges.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not complete request because: {ex.Message}");
            }
        }

        public bool DirectoryIsAdded()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var pathvar = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            var variables = pathvar?.Split(";");
            var isAdded = variables?.Any(v => v == currentDir) ?? false;
            if (isAdded)
            {
                Console.WriteLine($"Location {currentDir} is already added to PATH.");
            }
            return isAdded;
        }

        public void AddNewPath()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var pathvar = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            var variables = pathvar.Split(this.PathSeparator);

            Console.WriteLine($"Current path variables:\n{string.Join("\n", variables.Where(v => !string.IsNullOrWhiteSpace(v)))}\n");

            if (variables.Any(v => v == currentDir))
            {
                Console.WriteLine($"Variable {currentDir} is already added.");
            }
            else
            {
                var value = $@"{pathvar}{this.PathSeparator}{currentDir}";
                Environment.SetEnvironmentVariable("PATH", value, EnvironmentVariableTarget.Machine);
                Console.WriteLine($"Added {currentDir} to PATH.");
            }
        }
    }
}