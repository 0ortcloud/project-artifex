using Artifex.Enum;
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
                SELECT s.Id, s.Title, s.Condensation
                FROM ChatSessions AS s
                ORDER BY s.UpdatedAt DESC;
            ";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new SessionMini
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Condensation = reader.GetString(2)
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

        public Chat? InsertMyOneChat(int id, int role, string content, int score, int toolName)
        {
            using var connection = new SqliteConnection(DatabaseUtilClass.ConnectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                long unixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                // INSERT
                using var insertCommand = connection.CreateCommand();
                insertCommand.Transaction = transaction;
                insertCommand.CommandText = @"
                    INSERT INTO ChatMessages
                        (SessionId, MessageRole, Content, Score, CreatedAt, ToolName)
                    VALUES
                        (@SessionId, @MessageRole, @Content, @Score, @CreatedAt, @ToolName);
                ";

                insertCommand.Parameters.AddWithValue("@SessionId", id);
                insertCommand.Parameters.AddWithValue("@MessageRole", role);
                insertCommand.Parameters.AddWithValue("@Content", content);
                insertCommand.Parameters.AddWithValue("@Score", score);
                insertCommand.Parameters.AddWithValue("@CreatedAt", unixSeconds);
                insertCommand.Parameters.AddWithValue("@ToolName", toolName);

                int insertResult = insertCommand.ExecuteNonQuery();

                // UPDATE
                using var updateCommand = connection.CreateCommand();
                updateCommand.Transaction = transaction;
                updateCommand.CommandText = @"
                    UPDATE ChatSessions
                    SET UpdatedAt = @UpdatedAt
                    WHERE Id = @Id;
                ";

                updateCommand.Parameters.AddWithValue("@UpdatedAt", unixSeconds);
                updateCommand.Parameters.AddWithValue("@Id", id);

                int updateResult = updateCommand.ExecuteNonQuery();

                if (insertResult != 1 || updateResult != 1)
                {
                    transaction.Rollback();
                    return null;
                }

                // INSERT된 행 조회
                using var selectCommand = connection.CreateCommand();
                selectCommand.Transaction = transaction;
                selectCommand.CommandText = @"
                    SELECT
                        Id,
                        SessionId,
                        MessageRole,
                        Content,
                        Score,
                        CreatedAt,
                        ToolName
                    FROM ChatMessages
                    WHERE Id = last_insert_rowid();
                ";

                using var reader = selectCommand.ExecuteReader();

                if (!reader.Read())
                {
                    transaction.Rollback();
                    return null;
                }

                var chat = new Chat
                {
                    Id = reader.GetInt32(0),
                    SessionId = reader.GetInt32(1),
                    MessageRole = (MessageRole)reader.GetInt32(2),
                    Content = reader.GetString(3),
                    Score = reader.GetInt32(4),
                    CreatedAt = reader.GetInt64(5),
                    ToolName = (ToolName)reader.GetInt32(6)
                };

                transaction.Commit();

                return chat;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public Session? InsertMyOneChatSession(string title)
        {
            if (title == null || title == "") title = "New Chat Session";
            using var connection = new SqliteConnection(DatabaseUtilClass.ConnectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                using var insertCommand = connection.CreateCommand();
                insertCommand.Transaction = transaction;
                insertCommand.CommandText = @"
                    INSERT INTO ChatSessions (Title, Condensation, CreatedAt, UpdatedAt)
                    VALUES (@title, @Condensation, @CreatedAt, @UpdatedAt);
                ";
                long unixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                insertCommand.Parameters.AddWithValue("@title", title);
                insertCommand.Parameters.AddWithValue("@Condensation", string.Empty);
                insertCommand.Parameters.AddWithValue("@CreatedAt", unixSeconds);
                insertCommand.Parameters.AddWithValue("@UpdatedAt", unixSeconds);

                int insertResult = insertCommand.ExecuteNonQuery();

                using var getCommand = connection.CreateCommand();
                getCommand.CommandText = @"
                    SELECT
                        Id, Title, Condensation, CreatedAt, UpdatedAt 
                    FROM ChatSessions
                    WHERE Id = last_insert_rowid();
                ";

                if (insertResult != 1)
                {
                    transaction.Rollback();
                    return null;
                }

                using var reader = getCommand.ExecuteReader();

                if (!reader.Read())
                {
                    transaction.Rollback();
                    return null;
                }

                var session = new Session
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Condensation = reader.GetString(2),
                    CreatedAt = reader.GetInt32(3),
                    UpdatedAt = reader.GetInt32(4)
                };

                transaction.Commit();

                return session;

            }
            catch
            {
                transaction.Rollback();
                throw;
            }
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

        public int? RemoveMyOneChatSession(int id)
        {
            using var connection = new SqliteConnection(DatabaseUtilClass.ConnectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                var removeChatCommand = connection.CreateCommand();
                removeChatCommand.Transaction = transaction;
                removeChatCommand.CommandText = @"
                    DELETE FROM ChatMessages
                    WHERE SessionId = @SessionId;
                ";
                removeChatCommand.Parameters.AddWithValue("@SessionId", id);
                removeChatCommand.ExecuteNonQuery();

                var deleteSessionCommand = connection.CreateCommand();
                deleteSessionCommand.Transaction = transaction;
                deleteSessionCommand.CommandText = @"
                    DELETE FROM ChatSessions
                    WHERE Id = @id;
                ";
                deleteSessionCommand.Parameters.AddWithValue("@id", id);

                int affectedRows = deleteSessionCommand.ExecuteNonQuery();

                if (affectedRows != 1)
                {
                    transaction.Rollback();
                    return 0;
                }

                transaction.Commit();
                return 1;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
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