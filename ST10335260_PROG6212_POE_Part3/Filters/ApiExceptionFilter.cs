using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using ST10335260_PROG6212_POE_Part3.Services;

namespace ST10335260_PROG6212_POE_Part3.Filters;

public class ApiExceptionFilter : IExceptionFilter
{
    private readonly IModelMetadataProvider _metadataProvider;
    private readonly ITempDataDictionaryFactory _tempDataFactory;
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(
        IModelMetadataProvider metadataProvider,
        ITempDataDictionaryFactory tempDataFactory,
        ILogger<ApiExceptionFilter> logger)
    {
        _metadataProvider = metadataProvider;
        _tempDataFactory = tempDataFactory;
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case SessionExpiredException:
            {
                var tempData = _tempDataFactory.GetTempData(context.HttpContext);
                tempData["Warning"] = "Your session expired. Please sign in again.";
                var request = context.HttpContext.Request;
                var returnUrl = request.Method == HttpMethods.Get ? request.Path + request.QueryString : null;
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
                context.ExceptionHandled = true;
                break;
            }
            case ApiUnavailableException ex:
            {
                _logger.LogError(ex, "The RaceDay API could not be reached.");
                var view = new ViewResult
                {
                    ViewName = "ApiUnavailable",
                    StatusCode = StatusCodes.Status503ServiceUnavailable,
                    ViewData = new ViewDataDictionary(_metadataProvider, context.ModelState)
                };
                context.Result = view;
                context.ExceptionHandled = true;
                break;
            }
        }
    }
}
