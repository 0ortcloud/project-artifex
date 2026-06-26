namespace Artifex.Enum
{
    public enum MessageRole
    {
        System,
        User,
        Assistant
    }

    public enum ToolName
    {
        None,

        // プロジェクト
        ReadFile,
        WriteFile,
        EditFile,
        DeleteFile,

        // コード探索
        SearchCode,
        SearchFile,

        // 開発
        Build,
        Test,
        Terminal,

        // Git
        GitStatus,
        GitDiff,
        GitCommit,

        // 診断
        Diagnostics,

        // 外部
        Search,
        HttpRequest
    }
}