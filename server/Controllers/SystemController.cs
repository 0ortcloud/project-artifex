using Microsoft.AspNetCore.Mvc;

namespace Artifex.Controllers
{
    [ApiController]
    [Route("api/system")]
    public class SystemController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll()
        {
            Console.WriteLine("QWER");
            return Ok("QWER");
        }
        // public IActionResult GetAll()
        // {
        //     var sentences = _service.BringMyAllSentenceList();
        //     return Ok(sentences);
        // }
    }
}