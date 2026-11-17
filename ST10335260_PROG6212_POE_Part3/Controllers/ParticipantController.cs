using Microsoft.AspNetCore.Mvc;
using ST10335260_PROG6212_POE_Part3.Filters;
using ST10335260_PROG6212_POE_Part3.Models;
using ST10335260_PROG6212_POE_Part3.Services;

namespace ST10335260_PROG6212_POE_Part3.Controllers;

[SessionAuthorize("Participant")]
public class ParticipantController : RaceDayController
{
    public ParticipantController(ApiClient api) : base(api) { }

    [HttpGet]
    public async Task<IActionResult> Enrolments()
    {
        var result = await Api.GetAsync<List<EnrolmentInfo>>("api/users/me/enrolments");
        if (!result.Success)
            Error(result.Message ?? "We could not load your enrolments.");

        return View(result.Data ?? new List<EnrolmentInfo>());
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await Api.DeleteAsync($"api/enrolments/{id}");

        if (result.Success)
            Success("Your enrolment has been cancelled and your place released.");
        else
            Error(result.Message ?? "We could not cancel that enrolment.");

        return RedirectToAction(nameof(Enrolments));
    }

    [HttpGet]
    public async Task<IActionResult> Results()
    {
        var result = await Api.GetAsync<List<MyResultInfo>>("api/users/me/results");
        if (!result.Success)
            Error(result.Message ?? "We could not load your results.");

        return View(result.Data ?? new List<MyResultInfo>());
    }
}
