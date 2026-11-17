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
        var creatingNew = string.Equals(form.Mode, "new", StringComparison.OrdinalIgnoreCase);

        if (creatingNew)
        {
            if (string.IsNullOrWhiteSpace(form.NewCategoryName))
                ModelState.AddModelError(nameof(form.NewCategoryName), "Give the new category a name.");
        }
        else if (form.CategoryId is null or < 1)
        {
            ModelState.AddModelError(nameof(form.CategoryId), "Choose a category from the list, or create a custom one.");
        }

        if (!ModelState.IsValid)
        {
            Error(FirstModelError());
            return RedirectToAction(nameof(Index), new { eventId = form.EventId });
        }

        var categoryId = form.CategoryId ?? 0;

        if (creatingNew)
        {
            var name = form.NewCategoryName!.Trim();
            var created = await Api.PostAsync<CategoryInfo>("api/categories",
                new { CategoryName = name, Description = form.NewDescription?.Trim(), DefaultDistance = form.NewDistance });

            if (created.Success && created.Data is not null)
            {
                categoryId = created.Data.CategoryId;
            }
            else if (created.IsConflict)
            {
                var all = await Api.GetAsync<List<CategoryInfo>>("api/categories");
                var existing = all.Data?.FirstOrDefault(c => string.Equals(c.CategoryName, name, StringComparison.OrdinalIgnoreCase));
                if (existing is null)
                {
                    Error(created.Message ?? "A category with that name already exists.");
                    return RedirectToAction(nameof(Index), new { eventId = form.EventId });
                }

                categoryId = existing.CategoryId;
            }
            else
            {
                Error(created.Message ?? "We could not create that category.");
                return RedirectToAction(nameof(Index), new { eventId = form.EventId });
            }
        }

        var result = await Api.PostAsync<EventCategoryInfo>($"api/events/{form.EventId}/categories",
            new { CategoryId = categoryId, EntryFee = form.EntryFee, MaxParticipants = form.MaxParticipants });

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
}
