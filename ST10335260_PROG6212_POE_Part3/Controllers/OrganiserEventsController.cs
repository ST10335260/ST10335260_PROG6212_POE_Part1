using Microsoft.AspNetCore.Mvc;
using ST10335260_PROG6212_POE_Part3.Filters;
using ST10335260_PROG6212_POE_Part3.Models;
using ST10335260_PROG6212_POE_Part3.Services;

namespace ST10335260_PROG6212_POE_Part3.Controllers;

[SessionAuthorize("Organiser")]
[Route("Organiser/Events")]
public class OrganiserEventsController : RaceDayController
{
    public OrganiserEventsController(ApiClient api) : base(api) { }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var result = await Api.GetAsync<List<EventSummary>>("api/events/mine");
        if (!result.Success)
            Error(result.Message ?? "We could not load your events.");

        return View(result.Data ?? new List<EventSummary>());
    }

    [HttpGet("Create")]
    public IActionResult Create() =>
        View("Form", new EventFormViewModel { EventDate = DateTime.Today.AddDays(30).AddHours(8) });

    [HttpPost("Create")]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        if (!ModelState.IsValid) return View("Form", model);

        var result = await Api.PostAsync<EventSummary>("api/events", ToRequest(model));
        if (!result.Success || result.Data is null)
        {
            AddApiErrors(result, nameof(model.EventName), nameof(model.Description), nameof(model.EventDate), nameof(model.Location), nameof(model.DistanceKm));
            return View("Form", model);
        }

        var bannerFailed = await TryUploadBanner(result.Data.EventId, model.Banner);
        if (bannerFailed is null)
            Success($"\"{result.Data.EventName}\" has been created. Next, add race categories so participants can enter.");
        else
            Warning($"\"{result.Data.EventName}\" was created, but the banner could not be uploaded: {bannerFailed}");

        return RedirectToAction("Index", "OrganiserCategories", new { eventId = result.Data.EventId });
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await Api.GetAsync<EventSummary>($"api/events/{id}");
        if (result.IsNotFound) return View("NotFound");
        if (!result.Success || result.Data is null)
        {
            Error(result.Message ?? "We could not load that event.");
            return RedirectToAction(nameof(Index));
        }

        if (result.Data.OrganiserId != CurrentUser!.UserId)
            return RedirectToAction("AccessDenied", "Account");

        var ev = result.Data;
        return View("Form", new EventFormViewModel
        {
            EventId = ev.EventId,
            EventName = ev.EventName,
            Description = ev.Description,
            EventDate = ev.EventDate,
            Location = ev.Location,
            DistanceKm = ev.DistanceKm,
            EventType = ev.EventType,
            IsActive = ev.IsActive,
            BannerUrl = ev.BannerUrl
        });
    }

    [HttpPost("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, EventFormViewModel model)
    {
        model.EventId = id;
        if (!ModelState.IsValid) return View("Form", model);

        var result = await Api.PutAsync<EventSummary>($"api/events/{id}", ToRequest(model));
        if (!result.Success)
        {
            if (result.IsForbidden) return RedirectToAction("AccessDenied", "Account");

            AddApiErrors(result, nameof(model.EventName), nameof(model.Description), nameof(model.EventDate), nameof(model.Location), nameof(model.DistanceKm));
            return View("Form", model);
        }

        var bannerFailed = await TryUploadBanner(id, model.Banner);
        if (bannerFailed is null)
            Success("Your changes have been saved.");
        else
            Warning($"Your changes were saved, but the banner could not be uploaded: {bannerFailed}");

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await Api.DeleteAsync($"api/events/{id}");

        if (result.Success)
            Success("The event has been deleted.");
        else
            Error(result.Message ?? "We could not delete that event.");

        return RedirectToAction(nameof(Index));
    }

    private async Task<string?> TryUploadBanner(int eventId, IFormFile? banner)
    {
        if (banner is null || banner.Length == 0) return null;

        var upload = await Api.UploadAsync<UploadResponse>($"api/events/{eventId}/banner", banner);
        return upload.Success ? null : upload.Message ?? "Unknown error.";
    }

    private static object ToRequest(EventFormViewModel model) => new
    {
        model.EventName,
        model.Description,
        EventDate = model.EventDate!.Value,
        model.Location,
        model.DistanceKm,
        model.EventType,
        model.IsActive
    };
}
