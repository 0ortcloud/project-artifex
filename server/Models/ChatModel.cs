namespace Artifex.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public MessageRole MessageRole { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Score { get; set; }
        public long CreatedAt { get; set; }
        public ToolName ToolName { get; set; }
    }
    public enum MessageRole
    {
        System,
        User,
        Assistant
    }

    public enum ToolName
    {
        None,
        Search,
        Database,
        Calculator,
        HttpRequest,
        CustomFunction
    }
}