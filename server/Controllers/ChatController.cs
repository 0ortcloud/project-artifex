using Microsoft.AspNetCore.Mvc;
using Artifex.Services;
using Artifex.Request;
using Artifex.Models;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text;

namespace Artifex.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly ChatService _service;
        private readonly LLMService _llmService;
        public ChatController(ILogger<ChatController> logger, ChatService service, LLMService llmService)
        {
            _logger = logger;
            _service = service;
            _llmService = llmService;
        }

        [HttpGet("session/list")]
        public IActionResult GetAllSession()
        {
            var response = _service.BringMyAllChatSessionList();
            _logger.LogInformation("全てのチャットセッションリスト出力成功。");
            return Ok(response);
        }

        [HttpGet("session/{id}")]
        public IActionResult GetOneSession(int id)
        {
            var response = _service.BringMyOneChatSession(id);
            _logger.LogInformation($"（ID：{id}）チャットセッション出力成功。");
            return Ok(response);
        }

        [HttpPatch("session/{id}")]
        public IActionResult PatchOneSessionTitle(int id, [FromBody] PatchSessionTitleRequest request)
        {
            var title = request.Title;
            if (title == null)
            {
                _logger.LogError($"チャットセッションのタイトルが見つかりません。");
                return Ok(false);
            }
            var response = _service.EditMyOneChatSessionTitle(id, title);
            if (response == 1)
            {
                _logger.LogInformation($"（ID：{id}）チャットセッションのタイトル変更成功。");
                return Ok(true);
            }
            else
            {
                _logger.LogError($"（ID：{id}）チャットセッションのタイトル変更失敗。");
                return Ok(false);
            }
        }

        [HttpDelete("session/{id}")]
        public IActionResult DeleteOneSession(int id)
        {
            if (_service.RemoveMyOneChatSession(id) == 1)
            {
                _logger.LogInformation($"（ID：{id}）チャットセッション削除成功。");
                return Ok(true);
            }
            else
            {
                _logger.LogError($"（ID：{id}）チャットセッション削除失敗。");
                return Ok(false);
            }
        }

        [HttpPost]
        public async Task InsertOneChat([FromBody] InsertOneChatRequest request)
        {
            var SessionId = request.SessionId;
            var MessageRole = request.MessageRole;
            var Content = request.Content;
            var Score = request.Score;
            var ToolName = request.ToolName;

            // 1. セッションが存在しない場合、新規作成
            if (request.SessionId == 0)
            {
                Session? newSession = _service.InsertMyOneChatSession("New Chat Session");
                if (newSession != null)
                {
                    SessionId = newSession.Id;
                }
                else
                {
                    _logger.LogError("チャットセッション生成エラー。");
                    Response.StatusCode = 500;
                    return;
                }
            }

            // 2. ユーザーのメッセージをDBに保存
            Chat? response = _service.InsertMyOneChat(SessionId, MessageRole, Content, Score, ToolName);
            if (response == null)
            {
                _logger.LogError("チャット追加失敗。");
                Response.StatusCode = 400;
                return;
            }

            _logger.LogInformation("チャット追加成功。");

            // 3. HTTPレスポンスをストリーミング（SSE）用に設定
            Response.ContentType = "text/event-stream";
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            var fullAnswerBuilder = new StringBuilder();

            try
            {
                // 4. 新規セッションIDの確定情報を最初に送信
                var metaData = JsonSerializer.Serialize(new { SessionId, UserChat = response });
                await Response.WriteAsync($"data: {metaData}\n\n");
                await Response.Body.FlushAsync();

                // 5. LLMからストリーミングデータを取得してフロントエンドへ転送
                await foreach (var textChunk in _llmService.ChatAsync(SessionId, Content))
                {
                    fullAnswerBuilder.Append(textChunk);

                    // クライアントへメッセージの断片を送信
                    var chunkData = JsonSerializer.Serialize(new { AnswerChunk = textChunk });
                    await Response.WriteAsync($"data: {chunkData}\n\n");
                    await Response.Body.FlushAsync(); // 即時伝送
                }

                // 6. ストリーミング完了後、完成したAIの回答を最終的にDBへ保存
                string aiAnswer = fullAnswerBuilder.ToString();
                long UnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // 完了時点の時間を取得

                _service.InsertMyOneChat(SessionId, 2, aiAnswer, 0, 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ストリーミング処理中にエラーが発生しました。");
                // クライアントにエラーを通知
                await Response.WriteAsync($"data: {JsonSerializer.Serialize(new { Error = "AIの応答処理中にエラーが発生しました。" })}\n\n");
            }
        }
    }
}