using Microsoft.AspNetCore.Diagnostics;

namespace Angelus.Api.Common;

/// <summary>
/// Captura qualquer exceção não tratada e devolve o contrato <see cref="ApiError"/>
/// com status 500, evitando vazar stack trace para o cliente.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext http,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        logger.LogError(exception, "Erro não tratado em {Path}", http.Request.Path);

        var error = new ApiError(
            "Ocorreu um erro interno. Tente novamente mais tarde.",
            "INTERNAL_ERROR",
            StatusCodes.Status500InternalServerError,
            http.Request.Path
        );

        http.Response.StatusCode = error.StatusCode;
        await http.Response.WriteAsJsonAsync(error, cancellationToken);

        return true;
    }
}
