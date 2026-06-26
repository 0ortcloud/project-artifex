using Microsoft.AspNetCore.Mvc;
using Artifex.Services;
using Artifex.Request;
using Artifex.Models;
using System.Text.Json;
using System.Text.Encodings.Web;

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
        public async Task<IActionResult> InsertOneChat([FromBody] InsertOneChatRequest request)
        {
            var SessionId = request.SessionId;
            var MessageRole = request.MessageRole;
            var Content = request.Content;
            var Score = request.Score;
            var ToolName = request.ToolName;

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
                    return Ok(null);
                }
            }
            Chat? response = _service.InsertMyOneChat(SessionId, MessageRole, Content, Score, ToolName);
            if (response != null)
            {
                _logger.LogInformation("チャット追加成功。");
                var llmResponse = await _llmService.ChatAsync(Content);
                Console.WriteLine(JsonSerializer.Serialize(
    llmResponse,
    new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    }));
                return Ok(response);
            }
            _logger.LogError("チャット追加失敗。");
            return Ok(false);
        }
    }
}