using Microsoft.AspNetCore.Mvc;
using ST10335260_PROG6212_POE_Part3.Filters;
using ST10335260_PROG6212_POE_Part3.Models;
using ST10335260_PROG6212_POE_Part3.Services;

namespace ST10335260_PROG6212_POE_Part3.Controllers;

[SessionAuthorize("Organiser")]
[Route("Organiser")]
public class OrganiserController : RaceDayController
{
    public OrganiserController(ApiClient api) : base(api) { }

    [HttpGet("")]
    [HttpGet("Dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var result = await Api.GetAsync<DashboardInfo>("api/organiser/dashboard");
        if (!result.Success)
            Error(result.Message ?? "We could not load your dashboard.");

        return View(result.Data ?? new DashboardInfo());
    }
}
