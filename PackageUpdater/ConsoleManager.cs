using System;
using System.Collections.Generic;
using System.Linq;

namespace PackageUpdater
{
    public class ConsoleManager
    {
        private readonly List<InputParameter> consoleParameters;

        private bool isFirstRun = true;

        public ConsoleManager(IEnumerable<InputParameter> inputParams)
        {
            this.consoleParameters = new List<InputParameter>(4)
            {
                new InputParameter("-h", null, "Displays this help", _ => this.DisplayHelp())
            };
            this.consoleParameters.AddRange(inputParams);
        }

        public void DisplayHelp()
        {
            Console.WriteLine("Usage:\n PackageUpdater [options] [package_name] [new version]\n\nOptions:");

            foreach (var action in this.consoleParameters)
            {
                Console.WriteLine(" " + string.Join(" ", new List<string> { action.Parameter, action.OptionalParameter, "->", action.Description }.Where(s => !string.IsNullOrWhiteSpace(s))));
            }
        }

        public void ParseArguments(string[] args)
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
            var input = this.consoleParameters?.FirstOrDefault(a => a.Parameter == firstArg);
            if (this.isFirstRun && firstArg == null)
            {
                this.isFirstRun = false;
                this.DisplayHelp();
            }
            else if (input != null)
            {
                input.Action.Invoke(args);
            }
            else
            {
                Console.WriteLine($"{string.Join(" ", args)} is not valid input parameter.");
            }
        }
    }
}