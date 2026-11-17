using Microsoft.AspNetCore.Mvc;
using ST10335260_PROG6212_POE_Part3.Filters;
using ST10335260_PROG6212_POE_Part3.Models;
using ST10335260_PROG6212_POE_Part3.Services;

namespace ST10335260_PROG6212_POE_Part3.Controllers;

public class AccountController : RaceDayController
{
    public AccountController(ApiClient api) : base(api) { }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null, string? reason = null)
    {
        var user = CurrentUser;
        if (user is not null) return HomeForRole(user);

        if (reason == "signin")
            Warning("Please sign in to continue.");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        HttpContext.Session.Clear();

        var result = await Api.PostAsync<LoginResponse>("api/auth/login", new { model.Email, model.Password });

        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.StatusCode == 401
                ? "That email and password combination is not correct."
                : result.Message ?? "We could not sign you in right now.");
            model.Password = string.Empty;
            return View(model);
        }

        HttpContext.Session.SignIn(result.Data);
        var user = CurrentUser!;
        Success($"Welcome back, {user.FirstName}!");

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            var url = model.ReturnUrl;
            var blocked = (user.IsOrganiser && url.StartsWith("/Participant", StringComparison.OrdinalIgnoreCase))
                          || (user.IsParticipant && url.StartsWith("/Organiser", StringComparison.OrdinalIgnoreCase));
            if (!blocked) return LocalRedirect(url);
        }

        return HomeForRole(user);
    }

    [HttpGet]
    public IActionResult Register(string? role = null)
    {
        var user = CurrentUser;
        if (user is not null) return HomeForRole(user);

        return View(new RegisterViewModel { Role = role == "Organiser" ? "Organiser" : "Participant" });
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (model.Role != "Organiser" && model.Role != "Participant")
            ModelState.AddModelError(nameof(model.Role), "Choose how you will use RaceDay.");

        if (model.DateOfBirth is { } dob && dob.Date >= DateTime.Today)
            ModelState.AddModelError(nameof(model.DateOfBirth), "Your date of birth must be in the past.");

        if (!ModelState.IsValid) return View(model);

        var result = await Api.PostAsync<object>("api/auth/register", new
        {
            model.Email,
            model.Password,
            model.FirstName,
            model.LastName,
            DateOfBirth = model.DateOfBirth!.Value,
            model.Role
        });

        if (!result.Success)
        {
            if (result.IsConflict)
                ModelState.AddModelError(nameof(model.Email), "An account with that email address already exists.");
            else
                AddApiErrors(result, nameof(model.Email), nameof(model.Password), nameof(model.FirstName), nameof(model.LastName), nameof(model.Role));

            return View(model);
        }

        var login = await Api.PostAsync<LoginResponse>("api/auth/login", new { model.Email, model.Password });
        if (login.Success && login.Data is not null)
        {
            HttpContext.Session.SignIn(login.Data);
            Success($"Welcome to RaceDay, {login.Data.FirstName}! Your account is ready.");
            return HomeForRole(CurrentUser!);
        }

        Success("Your account has been created. Please sign in.");
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await Api.PostAsync<object>("api/auth/logout");
        }
        catch (Exception ex) when (ex is ApiUnavailableException or SessionExpiredException)
        {
        }

        HttpContext.Session.Clear();
        Success("You have been signed out.");
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    [SessionAuthorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var result = await Api.GetAsync<UserProfile>("api/users/me");
        if (!result.Success || result.Data is null)
        {
            Error(result.Message ?? "We could not load your profile.");
            return RedirectToAction("Index", "Home");
        }

        return View(ToViewModel(result.Data));
    }

    [SessionAuthorize]
    [HttpPost]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(await WithCurrentPicture(model));

        var result = await Api.PutAsync<UserProfile>("api/users/me", new { model.FirstName, model.LastName, model.Email });
        if (!result.Success)
        {
            if (result.IsConflict)
                ModelState.AddModelError(nameof(model.Email), result.Message ?? "That email address is already in use.");
            else
                AddApiErrors(result, nameof(model.Email), nameof(model.FirstName), nameof(model.LastName));

            return View(await WithCurrentPicture(model));
        }

        var current = CurrentUser!;
        HttpContext.Session.UpdateProfile(model.FirstName, model.LastName, model.Email.Trim().ToLowerInvariant(), current.ProfilePictureUrl);
        Success("Your profile has been updated.");
        return RedirectToAction(nameof(Profile));
    }

    [SessionAuthorize]
    [HttpPost]
    public async Task<IActionResult> UploadPicture(IFormFile? picture)
    {
        if (picture is null || picture.Length == 0)
        {
            Error("Choose an image to upload first.");
            return RedirectToAction(nameof(Profile));
        }

        var result = await Api.UploadAsync<UploadResponse>("api/users/me/profile-picture", picture);
        if (!result.Success || result.Data is null)
        {
            Error(result.Message ?? "We could not upload that picture.");
            return RedirectToAction(nameof(Profile));
        }

        var current = CurrentUser!;
        var names = current.FullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        HttpContext.Session.UpdateProfile(names.FirstOrDefault() ?? string.Empty, names.Length > 1 ? names[1] : string.Empty, current.Email, result.Data.Url);
        Success("Your profile picture has been updated.");
        return RedirectToAction(nameof(Profile));
    }

    private static ProfileViewModel ToViewModel(UserProfile profile) => new()
    {
        FirstName = profile.FirstName,
        LastName = profile.LastName,
        Email = profile.Email,
        DateOfBirth = profile.DateOfBirth,
        Role = profile.Role,
        ProfilePictureUrl = profile.ProfilePictureUrl
    };

    private async Task<ProfileViewModel> WithCurrentPicture(ProfileViewModel model)
    {
        var result = await Api.GetAsync<UserProfile>("api/users/me");
        if (result.Success && result.Data is not null)
        {
            model.DateOfBirth = result.Data.DateOfBirth;
            model.Role = result.Data.Role;
            model.ProfilePictureUrl = result.Data.ProfilePictureUrl;
        }

        return model;
    }
}
