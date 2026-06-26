using Artifex.Models;

namespace Artifex.AI
{
    public class Tool
    {
        public string Name { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public ToolParameter Parameters { get; init; } = new();
    }
    public class ToolParameter
    {
        public string Type { get; init; } = "object";

        public Dictionary<string, ToolProperty> Properties { get; init; } = [];

        public List<string> Required { get; init; } = [];
    }
    public class ToolProperty
    {
        public string Type { get; init; } = "string";

        public string Description { get; init; } = string.Empty;
    }
}

