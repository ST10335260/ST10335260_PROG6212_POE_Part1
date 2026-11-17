using Microsoft.AspNetCore.Mvc;
using ST10335260_PROG6212_POE_Part3.Filters;
using ST10335260_PROG6212_POE_Part3.Models;
using ST10335260_PROG6212_POE_Part3.Services;

namespace ST10335260_PROG6212_POE_Part3.Controllers;

public class EventsController : RaceDayController
{
    public EventsController(ApiClient api) : base(api) { }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var eventResult = await Api.GetAsync<EventSummary>($"api/events/{id}");
        if (eventResult.IsNotFound) return View("NotFound");
        if (!eventResult.Success || eventResult.Data is null)
        {
            Error(eventResult.Message ?? "We could not load that event.");
            return RedirectToAction("Index", "Home");
        }

        var categoriesResult = await Api.GetAsync<List<EventCategoryInfo>>($"api/events/{id}/categories");

        var model = new EventDetailsViewModel
        {
            Event = eventResult.Data,
            Categories = categoriesResult.Data ?? new List<EventCategoryInfo>()
        };

        var user = CurrentUser;
        if (user is { IsParticipant: true })
        {
            var mine = await Api.GetAsync<List<EnrolmentInfo>>("api/users/me/enrolments");
            if (mine.Success && mine.Data is not null)
            {
                model.MyEnrolments = mine.Data
                    .Where(e => e.EventId == id)
                    .ToDictionary(e => e.EventCategoryId);
            }
        }

        if (model.Event.IsPast)
        {
            foreach (var category in model.Categories)
            {
                var board = await Api.GetAsync<List<ResultInfo>>($"api/event-categories/{category.EventCategoryId}/results");
                if (board.Success && board.Data is { Count: > 0 })
                    model.Leaderboards[category.EventCategoryId] = board.Data;
            }
        }

        return View(model);
    }

    [HttpPost]
    [SessionAuthorize("Participant")]
    public async Task<IActionResult> Enrol(int id, int eventCategoryId)
    {
        var result = await Api.PostAsync<EnrolmentInfo>($"api/event-categories/{eventCategoryId}/enrolments");

        if (result.Success && result.Data is not null)
        {
            var bib = result.Data.BibNumber is { } number ? $" Your bib number is #{number}." : string.Empty;
            Success($"You are entered for {result.Data.EventName} ({result.Data.CategoryName}).{bib} The organiser will confirm your place.");
            return RedirectToAction("Enrolments", "Participant");
        }

        Error(result.Message ?? "We could not complete your enrolment.");
        return RedirectToAction(nameof(Details), new { id });
    }
}
