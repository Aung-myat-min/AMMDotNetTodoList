using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotNetTodoList.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoAPIController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetTodos() {
        
        }
    }
}
