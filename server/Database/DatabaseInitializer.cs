using Microsoft.Data.Sqlite;

namespace Artifex
{
    public static class DatabaseUtilClass
    {
        public static readonly string ConnectionString = "Data Source=Database/Artifex.db";

        public static void Init()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS ChatSessions
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Condensation TEXT NOT NULL,
                    CreatedAt INTEGER NOT NULL,
                    UpdatedAt INTEGER NOT NULL
                );

                CREATE TABLE IF NOT EXISTS ChatMessages
                (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SessionId INTEGER NOT NULL,
                    MessageRole INTEGER NOT NULL,
                    Content TEXT NOT NULL,
                    Score INTEGER NULL,
                    CreatedAt INTEGER NOT NULL,
                    ToolName INTEGER NULL,
                    FOREIGN KEY(SessionId)
                        REFERENCES ChatSessions(Id)
                );
            ";

            command.ExecuteNonQuery();
        }
    }
}