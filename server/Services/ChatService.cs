using Artifex.Models;
using Microsoft.Data.Sqlite;

namespace Artifex.Services
{
    public class ChatService
    {
        public List<SessionMini> BringMyAllChatSessionList()
        {
            var result = new List<SessionMini>();
            using var connection = new SqliteConnection(DatabaseUtilClass.ConnectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT s.Id, s.Title
                FROM ChatSessions AS s
                ORDER BY s.UpdatedAt DESC;
            ";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new SessionMini
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1)
                });
            }
            return result;
        }
        public List<Chat> BringMyOneChatSession(int id)
        {
            var result = new List<Chat>();
            using var connection = new SqliteConnection(DatabaseUtilClass.ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT cm.Id, cm.SessionId, cm.MessageRole, cm.Content, cm.Score, cm.CreatedAt, cm.ToolName
                FROM ChatMessages AS cm
                WHERE cm.SessionId = @sessionId;
            ";

            command.Parameters.AddWithValue("@sessionId", id);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Chat
                {
                    Id = reader.GetInt32(0),
                    SessionId = reader.GetInt32(1),
                    MessageRole = (MessageRole)reader.GetInt32(2),
                    Content = reader.GetString(3),
                    Score = reader.GetInt32(4),
                    CreatedAt = reader.GetInt64(5),
                    ToolName = (ToolName)reader.GetInt32(6),
                });
            }
            return result;
        }

        public int InsertMyOneChat(int id, string content)
        {
            using var connection = new SqliteConnection(DatabaseUtilClass.ConnectionString);
            connection.Open();
            // var command = connection.CreateCommand();
            // command.CommandText = @"
            //     INSERT INTO ChatSessions (Title, CreatedAt, UpdatedAt)
            //     VALUES (@title, @CreatedAt, @UpdatedAt);

            //     SELECT last_insert_rowid();
            // ";
            // long unixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // command.Parameters.AddWithValue("@title", title);
            // command.Parameters.AddWithValue("@CreatedAt", unixSeconds);
            // command.Parameters.AddWithValue("@UpdatedAt", unixSeconds);

            // object? result = command.ExecuteScalar();

            // if (result == null || result == DBNull.Value)
            // {
            //     return -1;
            // }

            // return Convert.ToInt32(result);
            return 1;
        }

        public int InsertMyOneChatSession(string title)
        {
            if (title == null || title == "") title = "New Chat Session";
            using var connection = new SqliteConnection(DatabaseUtilClass.ConnectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO ChatSessions (Title, CreatedAt, UpdatedAt)
                VALUES (@title, @CreatedAt, @UpdatedAt);

                SELECT last_insert_rowid();
            ";
            long unixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            command.Parameters.AddWithValue("@title", title);
            command.Parameters.AddWithValue("@CreatedAt", unixSeconds);
            command.Parameters.AddWithValue("@UpdatedAt", unixSeconds);

            object? result = command.ExecuteScalar();

            if (result == null || result == DBNull.Value)
            {
                return -1;
            }

            return Convert.ToInt32(result);
        }

        public int EditMyOneChatSessionTitle(int id, string title)
        {
            using var connection = new SqliteConnection(DatabaseUtilClass.ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE ChatSessions
                SET Title = @title
                WHERE Id = @id;
            ";

            command.Parameters.AddWithValue("@title", title);
            command.Parameters.AddWithValue("@id", id);

            int affectedRows = command.ExecuteNonQuery();

            return affectedRows;
        }

        public int RemoveMyOneChatSession(int id)
        {
            using var connection = new SqliteConnection(DatabaseUtilClass.ConnectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM ChatSessions WHERE Id = @id;
            ";
            command.Parameters.AddWithValue("@id", id);

            int affectedRows = command.ExecuteNonQuery();

            return affectedRows;
        }

        public int InsertMyOneChat(int sessionId, string role, string content, int score, string toolName)
        {
            using var connection = new SqliteConnection(DatabaseUtilClass.ConnectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO ChatMessages (SessionId, Role, Content, Score, CreatedAt, ToolName)
                VALUES (@sessionId, @role, @content, @score, @createdAt, @toolName);
            ";
            long unixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            command.Parameters.AddWithValue("@sessionId", sessionId);
            command.Parameters.AddWithValue("@role", role);
            command.Parameters.AddWithValue("@content", content);
            command.Parameters.AddWithValue("@score", score);
            command.Parameters.AddWithValue("@createdAt", unixSeconds);
            command.Parameters.AddWithValue("@toolName", toolName);

            int affectedRows = command.ExecuteNonQuery();

            return affectedRows;
        }

    }
}