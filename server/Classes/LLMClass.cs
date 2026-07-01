using System.Text.Json.Serialization;

namespace Artifex.MyClass
{
    public class UserSendMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

    }
    public class LLMMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("thinking")]
        public string? Thinking { get; set; } = string.Empty;
    }

    public class FrontEndLLMMessage
    {
        [JsonPropertyName("isContent")]
        public bool IsContent { get; init; }

        [JsonPropertyName("text")]
        public string Text { get; init; } = string.Empty;
    }
}