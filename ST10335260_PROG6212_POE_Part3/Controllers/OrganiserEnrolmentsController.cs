using Microsoft.AspNetCore.Mvc;
using ST10335260_PROG6212_POE_Part3.Filters;
using ST10335260_PROG6212_POE_Part3.Models;
using ST10335260_PROG6212_POE_Part3.Services;

namespace ST10335260_PROG6212_POE_Part3.Controllers;

[SessionAuthorize("Organiser")]
[Route("Organiser/Enrolments")]
public class OrganiserEnrolmentsController : RaceDayController
{
    public OrganiserEnrolmentsController(ApiClient api) : base(api) { }

    [HttpGet("")]
    public async Task<IActionResult> Index(int? eventId, EnrolmentStatus? status)
    {
        var events = await Api.GetAsync<List<EventSummary>>("api/events/mine");
        var model = new EnrolmentsPageViewModel
        {
            Events = events.Data ?? new List<EventSummary>(),
            StatusFilter = status
        };

        if (!events.Success)
            Error(events.Message ?? "We could not load your events.");

        model.Selected = model.Events.FirstOrDefault(e => e.EventId == eventId) ?? model.Events.FirstOrDefault();
        if (model.Selected is null) return View(model);

        var enrolments = await Api.GetAsync<List<OrganiserEnrolmentInfo>>($"api/events/{model.Selected.EventId}/enrolments");
        if (!enrolments.Success)
            Error(enrolments.Message ?? "We could not load the enrolments.");

        model.Enrolments = enrolments.Data ?? new List<OrganiserEnrolmentInfo>();
        return View(model);
    }

    [HttpPost("Status")]
    public async Task<IActionResult> SetStatus(int eventId, int enrolmentId, EnrolmentStatus status)
    {
        var result = await Api.PutAsync<OrganiserEnrolmentInfo>($"api/enrolments/{enrolmentId}/status", new { status });

        if (result.Success)
            Success($"Enrolment marked as {status}.");
        else
            Error(result.Message ?? "We could not update that enrolment.");

        return RedirectToAction(nameof(Index), new { eventId });
    }
}
