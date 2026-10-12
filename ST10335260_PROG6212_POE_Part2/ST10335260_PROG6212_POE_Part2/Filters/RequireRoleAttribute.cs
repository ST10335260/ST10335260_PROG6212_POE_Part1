using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ST10335260_PROG6212_POE_Part2.Filters;

public class RequireRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _role;
    public RequireRoleAttribute(string role) => _role = role;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;
        var userId = session.GetInt32("UserId");
        var role = session.GetString("Role");

        if (userId is null)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Not authenticated." });
            return;
        }

        if (role != _role)
        {
            context.Result = new ObjectResult(new { message = $"This action requires the {_role} role." })
            {
                StatusCode = 403
            };
        }
    }
}