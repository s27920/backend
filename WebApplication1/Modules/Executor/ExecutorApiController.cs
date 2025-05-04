using Microsoft.AspNetCore.Mvc;

namespace WebApplication17.Executor;

[ApiController]
[Route("/api/execute")]
public class ExecutorApiController(IExecutorService executorService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> ExecuteCode([FromBody] ExecuteRequestDto executeRequest)
    {
        return Ok(await executorService.FullExecute(executeRequest));
    }
    [HttpPost("dry")]
    public async Task<IActionResult> DryExecuteCode([FromBody] ExecuteRequestDto executeRequest)
    {
        return Ok(await executorService.DryExecute(executeRequest));
    }
}