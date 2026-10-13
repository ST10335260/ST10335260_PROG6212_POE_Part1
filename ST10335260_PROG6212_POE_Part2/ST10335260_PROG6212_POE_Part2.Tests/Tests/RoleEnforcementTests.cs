using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using ST10335260_PROG6212_POE_Part2.Filters;
using Xunit;

public class RoleEnforcementTests
{
    [Fact]
    public void ParticipantRole_IsRejected_FromOrganiserOnlyAction()
    {
        var httpContext = new DefaultHttpContext { Session = new TestSession() };
        httpContext.Session.SetInt32("UserId", 1);
        httpContext.Session.SetString("Role", "Participant");

        var filterContext = new AuthorizationFilterContext(
            new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>());

        var attribute = new RequireRoleAttribute("Organiser");
        attribute.OnAuthorization(filterContext);

        var result = Assert.IsType<ObjectResult>(filterContext.Result);
        Assert.Equal(403, result.StatusCode);
    }

    [Fact]
    public void NoSession_IsRejected_AsUnauthorized()
    {
        var httpContext = new DefaultHttpContext { Session = new TestSession() };
        var filterContext = new AuthorizationFilterContext(
            new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>());

        var attribute = new RequireRoleAttribute("Organiser");
        attribute.OnAuthorization(filterContext);

        Assert.IsType<UnauthorizedObjectResult>(filterContext.Result);
    }
}