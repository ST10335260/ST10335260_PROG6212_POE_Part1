using Microsoft.AspNetCore.Mvc;
using ST10335260_PROG6212_POE_Part3.Filters;
using ST10335260_PROG6212_POE_Part3.Models;
using ST10335260_PROG6212_POE_Part3.Services;

namespace ST10335260_PROG6212_POE_Part3.Controllers;

[SessionAuthorize("Organiser")]
[Route("Organiser/Categories")]
public class OrganiserCategoriesController : RaceDayController
{
    public OrganiserCategoriesController(ApiClient api) : base(api) { }

    [HttpGet("")]
    public async Task<IActionResult> Index(int? eventId)
    {
        var events = await Api.GetAsync<List<EventSummary>>("api/events/mine");
        var model = new CategoriesPageViewModel { Events = events.Data ?? new List<EventSummary>() };

        if (!events.Success)
            Error(events.Message ?? "We could not load your events.");

        model.Selected = model.Events.FirstOrDefault(e => e.EventId == eventId) ?? model.Events.FirstOrDefault();
        if (model.Selected is null) return View(model);

        var assigned = await Api.GetAsync<List<EventCategoryInfo>>($"api/events/{model.Selected.EventId}/categories");
        var all = await Api.GetAsync<List<CategoryInfo>>("api/categories");

        model.EventCategories = assigned.Data ?? new List<EventCategoryInfo>();
        var usedIds = model.EventCategories.Select(c => c.CategoryId).ToHashSet();
        model.AvailableCategories = (all.Data ?? new List<CategoryInfo>()).Where(c => !usedIds.Contains(c.CategoryId)).ToList();

        return View(model);
    }

    [HttpPost("Add")]
    public async Task<IActionResult> Add(AddEventCategoryForm form)
    {
        if (!ModelState.IsValid)
        {
            Error(FirstModelError());
            return RedirectToAction(nameof(Index), new { eventId = form.EventId });
        }

        var result = await Api.PostAsync<EventCategoryInfo>($"api/events/{form.EventId}/categories",
            new { form.CategoryId, form.EntryFee, form.MaxParticipants });

        if (result.Success)
            Success("The category has been added to your event.");
        else
            Error(result.Message ?? "We could not add that category.");

        return RedirectToAction(nameof(Index), new { eventId = form.EventId });
    }

    [HttpPost("Update")]
    public async Task<IActionResult> Update(EditEventCategoryForm form)
    {
        if (!ModelState.IsValid)
        {
            Error(FirstModelError());
            return RedirectToAction(nameof(Index), new { eventId = form.EventId });
        }

        var result = await Api.PutAsync<EventCategoryInfo>($"api/event-categories/{form.EventCategoryId}",
            new { form.CategoryId, form.EntryFee, form.MaxParticipants });

        if (result.Success)
            Success("The category has been updated.");
        else
            Error(result.Message ?? "We could not update that category.");

        return RedirectToAction(nameof(Index), new { eventId = form.EventId });
    }

    [HttpPost("Remove")]
    public async Task<IActionResult> Remove(int eventId, int eventCategoryId)
    {
        var result = await Api.DeleteAsync($"api/event-categories/{eventCategoryId}");

        if (result.Success)
            Success("The category has been removed from the event.");
        else
            Error(result.Message ?? "We could not remove that category.");

        return RedirectToAction(nameof(Index), new { eventId });
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(NewCategoryForm form)
    {
        if (!ModelState.IsValid)
        {
            Error(FirstModelError());
            return RedirectToAction(nameof(Index), new { eventId = form.EventId });
        }

        var result = await Api.PostAsync<CategoryInfo>("api/categories",
            new { form.CategoryName, form.Description, form.DefaultDistance });

        if (result.Success)
            Success($"The \"{form.CategoryName.Trim()}\" category is now available. You can add it to your event below.");
        else
            Error(result.Message ?? "We could not create that category.");

        return RedirectToAction(nameof(Index), new { eventId = form.EventId });
    }
}
