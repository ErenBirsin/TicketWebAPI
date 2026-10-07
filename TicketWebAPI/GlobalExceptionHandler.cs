using Microsoft.AspNetCore.Diagnostics;
using TS.Result;

namespace TicketWebAPI;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        int statusCode = 500;
        string message = "Beklenmeyen bir hata oluştu";

        if (exception is BadHttpRequestException)
        {
            statusCode = 400;
            message = "Gönderilen veri hatalı lütfen alanların formatını kontrol edin";
        }

        httpContext.Response.StatusCode = statusCode;

        var res = Result<string>.Failure(statusCode, message);
        await httpContext.Response.WriteAsJsonAsync(res, cancellationToken);

        return true;
    }
}