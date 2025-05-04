using Microsoft.AspNetCore.Mvc;

namespace WebApplication17.Executor;

[ApiController]
[Route("/")]
public class ExecutorIndexController : ControllerBase
{
    
    private readonly IWebHostEnvironment _env;

    public ExecutorIndexController(IWebHostEnvironment env)
    {
        _env = env;
    }
    
    [HttpGet]
    public IActionResult ServeIndexAsync()
    {
        return Ok("Hello world!");
        var filepath = Path.Combine("../files/index"); //TODO arbitrary. Change this
        return PhysicalFile(filepath, "text/html");
    }
}