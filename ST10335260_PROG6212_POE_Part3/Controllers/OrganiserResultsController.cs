using Microsoft.AspNetCore.Mvc;
using ST10335260_PROG6212_POE_Part3.Filters;
using ST10335260_PROG6212_POE_Part3.Models;
using ST10335260_PROG6212_POE_Part3.Services;

namespace ST10335260_PROG6212_POE_Part3.Controllers;

[SessionAuthorize("Organiser")]
[Route("Organiser/Results")]
public class OrganiserResultsController : RaceDayController
{
    public OrganiserResultsController(ApiClient api) : base(api) { }

    [HttpGet("")]
    public async Task<IActionResult> Index(int? eventId)
    {
        var events = await Api.GetAsync<List<EventSummary>>("api/events/mine");
        var model = new ResultsPageViewModel { Events = events.Data ?? new List<EventSummary>() };

        if (!events.Success)
            Error(events.Message ?? "We could not load your events.");

        model.Selected = model.Events.FirstOrDefault(e => e.EventId == eventId) ?? model.Events.FirstOrDefault();
        if (model.Selected is null) return View(model);

        var enrolments = await Api.GetAsync<List<OrganiserEnrolmentInfo>>($"api/events/{model.Selected.EventId}/enrolments");
        if (!enrolments.Success)
            Error(enrolments.Message ?? "We could not load the participants.");

        model.Enrolments = (enrolments.Data ?? new List<OrganiserEnrolmentInfo>())
            .Where(e => e.Status == EnrolmentStatus.Confirmed)
            .ToList();

        return View(model);
    }

    [HttpPost("Save")]
    public async Task<IActionResult> Save(ResultFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            Error(FirstModelError());
            return RedirectToAction(nameof(Index), new { eventId = form.EventId });
        }

        if (!TimeSpan.TryParse(form.FinishTime, System.Globalization.CultureInfo.InvariantCulture, out var finishTime) || finishTime <= TimeSpan.Zero)
        {
            Error("Finish time must be greater than 00:00:00.");
            return RedirectToAction(nameof(Index), new { eventId = form.EventId });
        }

        var body = new
        {
            form.EnrolmentId,
            FinishTime = finishTime,
            form.Position,
            OverallRank = form.Position,
            CategoryRank = form.Position,
            form.Verified
        };

        var result = form.ResultId > 0
            ? await Api.PutAsync<ResultInfo>($"api/results/{form.ResultId}", body)
            : await Api.PostAsync<ResultInfo>("api/results", body);

        if (result.Success)
            Success(form.Verified ? "The result has been saved and published." : "The result has been saved as a draft. Publish it when you are ready.");
        else
            Error(result.Message ?? "We could not save that result.");

        return RedirectToAction(nameof(Index), new { eventId = form.EventId });
    }
}
