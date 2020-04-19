using System;

namespace PackageUpdater
{
    public class InputParameter
    {
        public InputParameter(string parameter, string optionalParameter, string description, Action<string[]> action)
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