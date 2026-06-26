using System.Text.Json.Serialization;

namespace Artifex.MyClass
{
    public class LLMMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("thinking")]
        public string? Thinking { get; set; }
    }
}