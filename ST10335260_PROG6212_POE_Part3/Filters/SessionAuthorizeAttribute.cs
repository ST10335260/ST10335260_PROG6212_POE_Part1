using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ST10335260_PROG6212_POE_Part3.Services;

namespace ST10335260_PROG6212_POE_Part3.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public string? Role { get; }

    public SessionAuthorizeAttribute(string? role = null) => Role = role;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.Session.GetSessionUser();

        if (user is null)
        {
            var request = context.HttpContext.Request;
            var returnUrl = request.Method == HttpMethods.Get ? request.Path + request.QueryString : null;
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl, reason = "signin" });
            return;
        }

        if (Role is not null && !string.Equals(user.Role, Role, StringComparison.Ordinal))
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
        }
    }
}
