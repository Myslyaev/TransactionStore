using Serilog;
using System.Net;
using TransactionStore.API.Configuration.Exceptions;
using TransactionStore.Core.Exceptions;

namespace TransactionStore.API.Configuration;

public class ExceptionMiddleware(RequestDelegate next)
{
    private readonly Serilog.ILogger _logger = Log.ForContext<ExceptionMiddleware>();

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (ValidationException ex)
        {
            await HandleExceptionAsync(httpContext, ex, HttpStatusCode.UnprocessableEntity, "Validation error.");
        }
        catch (NotFoundException ex)
        {
            await HandleExceptionAsync(httpContext, ex, HttpStatusCode.NotFound, "Content not found error.");
        }
        catch (ServiceUnavailableException ex)
        {
            await HandleExceptionAsync(httpContext, ex, HttpStatusCode.ServiceUnavailable, "There is no connection to the database.");
        }
        catch (ForbiddenException ex)
        {
            await HandleExceptionAsync(httpContext, ex, HttpStatusCode.Forbidden, "Access denied.");
        }
        catch (Core.Exceptions.ArgumentNullException ex)
        {
            await HandleExceptionAsync(httpContext, ex, HttpStatusCode.BadRequest, "The value must not be null.");
        }
        catch (NoConfigurationException ex)
        {
            await HandleExceptionAsync(httpContext, ex, HttpStatusCode.VariantAlsoNegotiates, "Configuration not found.");
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex, HttpStatusCode.InternalServerError, $"Something went wrong.");
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, HttpStatusCode statusCode, string logMessage)
    {
        _logger.Error($"{logMessage}: {exception.Message}");
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(new ErrorDetails()
        {
            StatusCode = context.Response.StatusCode,
            Message = exception.Message
        }.ToString());
    }
}