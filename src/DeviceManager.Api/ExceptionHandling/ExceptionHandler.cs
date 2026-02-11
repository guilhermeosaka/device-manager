using DeviceManager.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace DeviceManager.Api.ExceptionHandling;

public class ExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        // TODO: log unexpected errors

        var result = exception switch
        {
            DeviceInUseException ex =>
                Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest),
            _ => Results.Problem(statusCode: StatusCodes.Status500InternalServerError)
        };

        await result.ExecuteAsync(context);
        return true;
    }
}