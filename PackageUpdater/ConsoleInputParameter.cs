using System;

namespace PackageUpdater
{
    public class ConsoleInputParameter
    {
        public ConsoleInputParameter(string parameter, string optionalParameter, string description, Action<string[]> action)
        {
            this.Parameter = parameter;
            this.OptionalParameter = optionalParameter;
            this.Description = description;
            this.Action = action;
        }

        public string Parameter { get; }
        public string OptionalParameter { get; }
        public string Description { get; }
        public Action<string[]> Action { get; }
    }
}