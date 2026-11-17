using Microsoft.AspNetCore.Mvc;
using ST10335260_PROG6212_POE_Part3.Models;
using ST10335260_PROG6212_POE_Part3.Services;

namespace ST10335260_PROG6212_POE_Part3.Controllers;

public class HomeController : RaceDayController
{
    public HomeController(ApiClient api) : base(api) { }

    public async Task<IActionResult> Index(EventFilterViewModel filter)
    {
        var user = CurrentUser;
        if (user is { IsOrganiser: true })
            return RedirectToAction("Dashboard", "Organiser");

        var result = await Api.GetAsync<List<EventSummary>>(filter.ToApiPath());

        return View(new HomeViewModel
        {
            Filter = filter,
            Events = result.Data ?? new List<EventSummary>(),
            LoadFailed = !result.Success
        });
    }

    [Route("Home/Error")]
    [IgnoreAntiforgeryToken]
    public IActionResult Error()
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        return View();
    }

    [Route("Home/Status")]
    [IgnoreAntiforgeryToken]
    public IActionResult Status(int code)
    {
        Response.StatusCode = code;
        return code == 404 ? View("NotFound") : View("Error");
    }
}
