using Microsoft.AspNetCore.Mvc;
using SeoLeadPlatform.Application.Common;

namespace SeoLeadPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult From<T>(Result<T> result, int successStatus = StatusCodes.Status200OK)
    {
        if (result.IsSuccess) return StatusCode(successStatus, result.Value);
        return MapError(result);
    }

    protected IActionResult From(Result result)
    {
        if (result.IsSuccess) return NoContent();
        return MapError(result);
    }

    private IActionResult MapError(Result result)
    {
        var code = result.ErrorCode ?? "error";
        var status = code switch
        {
            "auth.required" => StatusCodes.Status401Unauthorized,
            "auth.invalid_credentials" => StatusCodes.Status401Unauthorized,
            "auth.email_taken" => StatusCodes.Status409Conflict,
            var c when c.EndsWith(".not_found") => StatusCodes.Status404NotFound,
            var c when c.EndsWith(".duplicate_url") => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };
        return StatusCode(status, new
        {
            title = result.Error,
            code = result.ErrorCode,
            status
        });
    }
}
