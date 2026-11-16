using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Restoran.Application.Common;

namespace Restoran.Api.Infrastructure;

/// <summary>
/// Ühtne veakäsitlus: iga erand muudetakse ProblemDetails vastuseks.
/// Tänu sellele pole kontrollerites try-catch plokke.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetails;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetails)
    {
        _logger = logger;
        _problemDetails = problemDetails;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Ei leitud"),
            ConflictException => (StatusCodes.Status409Conflict, "Konflikt"),
            BusinessRuleException => (StatusCodes.Status400BadRequest, "Vigane päring"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Autentimine ebaõnnestus"),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Keelatud"),
            _ => (StatusCodes.Status500InternalServerError, "Serveri viga")
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Ootamatu viga: {Path}", context.Request.Path);
        else
            _logger.LogWarning("{Title} ({Status}): {Message}", title, status, exception.Message);

        context.Response.StatusCode = status;
        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == 500 ? "Midagi läks valesti. Proovi hiljem uuesti." : exception.Message,
                Instance = context.Request.Path
            }
        });
    }
}
