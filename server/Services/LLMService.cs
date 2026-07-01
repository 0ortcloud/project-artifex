using System.Runtime.CompilerServices;
using System.Text.Json;
using Artifex.Controllers;
using Artifex.MyClass;
using Artifex.Request;
using Artifex.Response;
using Microsoft.Data.Sqlite;

namespace Artifex.Services
{
    public class LLMService
    {
        private readonly ILogger<ChatController> _logger;
        private readonly HttpClient _client;

        public LLMService(ILogger<ChatController> logger, HttpClient client)
        {
            _logger = logger;
            _client = client;
        }
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private List<UserSendMessage> PreviousOfStory(int id)
        {
            List<UserSendMessage> result = [];
            using var connection = new SqliteConnection(DatabaseUtilClass.ConnectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*)
                FROM ChatMessages
                WHERE SessionId = @SessionId;
            ";
            command.Parameters.AddWithValue("@SessionId", id);
            var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return result;
            }
            if (reader.GetInt32(0) > 20)
            {

            }

            command = connection.CreateCommand();
            command.CommandText = @"
                SELECT MessageRole, Content
                    FROM (
                        SELECT MessageRole, Content, CreatedAt
                        FROM ChatMessages
                        WHERE SessionId = @SessionId
                        ORDER BY CreatedAt DESC
                        LIMIT 20
                    )
                ORDER BY CreatedAt ASC;
            ";
            command.Parameters.AddWithValue("@SessionId", id);
            reader = command.ExecuteReader();

            while (reader.Read())
            {
                int numbers = reader.GetInt32(0);
                string whos = string.Empty;
                if (numbers == 1)
                {
                    whos = "user";
                }
                else if (numbers == 2)
                {
                    whos = "assistant";
                }
                else
                {
                    whos = "system";
                }

                result.Add(new UserSendMessage
                {
                    Role = whos,
                    Content = reader.GetString(1),
                });
            }

            return result;
        }

        public async IAsyncEnumerable<string> ChatAsync(int sessionId,
        string prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            List<UserSendMessage> story = PreviousOfStory(sessionId);
            UserSendMessage system = new()
            {
                Role = "system",
                Content = "絵文字は使用しないでください。"
                // Content = "Emoji는 절대 사용하지 마세요. 사용자는 왕이며, 딩신은 그의 신하입니다. 말투는 언제나 하소서체를 써야 합니다. 마치 사극처럼 신하가 왕에게 하는 말을 쓰세요."
            };
            UserSendMessage newMessage = new()
            {
                Role = "user",
                Content = prompt
            };
            story.Insert(0, system);
            story.Add(newMessage);
            var request = new LLMChatRequest
            {
                Model = "gemma4:latest", // appsettings.json에서 읽도록 변경 권장
                Stream = true,
                Messages = story
            };

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/chat")
            {
                Content = JsonContent.Create(request)
            };

            using var response = await _client.SendAsync(
                requestMessage,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var line = await reader.ReadLineAsync(cancellationToken);

                if (line is null)
                    break;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                LLMChatResponse? chunk;

                try
                {
                    chunk = JsonSerializer.Deserialize<LLMChatResponse>(line, JsonOptions);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse Ollama response.");
                    continue;
                }

                if (chunk == null)
                    continue;

                if (chunk.Done)
                    break;

                if (!string.IsNullOrEmpty(chunk.Message?.Content))
                {
                    yield return chunk.Message.Content;
                }
            }

        }
    }
}

