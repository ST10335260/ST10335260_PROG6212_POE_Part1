using Microsoft.AspNetCore.Mvc;
using ST10335260_PROG6212_POE_Part3.Services;

namespace ST10335260_PROG6212_POE_Part3.Controllers;

public abstract class RaceDayController : Controller
{
    protected RaceDayController(ApiClient api) => Api = api;

    protected ApiClient Api { get; }

    protected SessionUser? CurrentUser => HttpContext.Session.GetSessionUser();

    protected void Success(string message) => TempData["Success"] = message;

    protected void Error(string message) => TempData["Error"] = message;

    protected void Warning(string message) => TempData["Warning"] = message;

    protected void AddApiErrors<T>(ApiResult<T> result, params string[] knownFields)
    {
        var added = false;

        foreach (var (key, messages) in result.Errors)
        {
            var field = knownFields.FirstOrDefault(f => key.EndsWith(f, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
            foreach (var message in messages)
            {
                ModelState.AddModelError(field, message);
                added = true;
            }
        }

        if (!added && !string.IsNullOrWhiteSpace(result.Message))
            ModelState.AddModelError(string.Empty, result.Message);
    }

    protected string FirstModelError() =>
        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
        ?? "Please check the details you entered.";

    protected IActionResult HomeForRole(SessionUser user) =>
        user.IsOrganiser ? RedirectToAction("Dashboard", "Organiser") : RedirectToAction("Index", "Home");
}
