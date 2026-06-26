namespace Artifex.Request
{
    public class PatchSessionTitleRequest
    {
        public string Title { get; set; } = string.Empty;
    }

    public class InsertOneChatRequest
    {
        public int SessionId { get; set; }
        public int MessageRole { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Score { get; set; }
        public int ToolName { get; set; }
    }
}