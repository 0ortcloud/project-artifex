using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Artifex.Services;

namespace Artifex.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly ChatService _service;
        public ChatController(ILogger<ChatController> logger, ChatService service)
        {
            _logger = logger;
            _service = service;
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
        public IActionResult PatchOneSessionTitle(int id, string title)
        {
            if (_service.EditMyOneChatSessionTitle(id, title) == 1)
            {
                _logger.LogInformation($"（ID：{id}）チャットセッションのタイトル変更成功。");
                return Ok("変更成功");
            }
            else
            {
                _logger.LogError($"（ID：{id}）チャットセッションのタイトル変更失敗。");
                return Ok("変更失敗");
            }
        }

        [HttpDelete("session/{id}")]
        public IActionResult DeleteOneSession(int id)
        {
            if (_service.RemoveMyOneChatSession(id) == 1)
            {
                _logger.LogInformation($"（ID：{id}）チャットセッション削除成功。");
                return Ok("変更成功");
            }
            else
            {
                _logger.LogError($"（ID：{id}）チャットセッション削除失敗。");
                return Ok("変更失敗");
            }
        }

        [HttpPost("session")]
        public IActionResult InsertOneSession(string title)
        {
            if (_service.InsertMyOneChatSession(title) == 1)
            {
                return Ok("追加成功");
            }
            else
            {
                return Ok("追加失敗");
            }
        }

        [HttpPost]
        public IActionResult InsertOneChat(int sessionId, string role, string content, int score, string toolName)
        {
            if (sessionId == -1)
            {
                int newSessionId = _service.InsertMyOneChatSession("New Chat Session");
                if (newSessionId != -1)
                {
                    sessionId = newSessionId;
                }
                else
                {
                    _logger.LogError("チャットセッション生成エラー。");
                    return Ok("追加失敗");
                }
            }
            if (_service.InsertMyOneChat(sessionId, role, content, score, toolName) == 1)
            {
                return Ok("追加成功");
            }
            else
            {
                return Ok("追加失敗");
            }
        }
    }
}