using Artifex.MyClass;

namespace Artifex.Request
{
    public class LLMChatRequest
    {
        public string Model { get; set; } = string.Empty;
        public List<LLMMessage> Messages { get; set; } = [];
        public bool Stream { get; set; } = false;
    }
}