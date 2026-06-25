namespace Artifex.Models
{
    public class Session
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }
    }

    public class SessionMini
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}