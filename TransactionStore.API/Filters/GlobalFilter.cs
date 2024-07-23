using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using TransactionStore.Core.Exceptions;
using ILogger = Serilog.ILogger;

namespace TransactionStore.API.Filters;

public class GlobalFilter(IConfiguration configuration) : IActionFilter
{
    private readonly ILogger _logger = Log.ForContext<GlobalFilter>();

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.HttpContext.Connection.RemoteIpAddress.ToString() != configuration["ServicesUrlSettings:Crm"])
        {
            _logger.Debug($"Access from adress {context.HttpContext.Connection.RemoteIpAddress} denied.");
            throw new ForbiddenException($"Access from adress {context.HttpContext.Connection.RemoteIpAddress} denied.");
        }
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }
}