using Microsoft.AspNetCore.Mvc;

namespace ST10335260_PROG6212_POE_Part2.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;

    protected string? CurrentRole => HttpContext.Session.GetString("Role");

    protected IActionResult Forbidden(string message) =>
        StatusCode(StatusCodes.Status403Forbidden, new { message });
}
