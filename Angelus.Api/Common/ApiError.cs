namespace Angelus.Api.Common;

/// <summary>
/// Contrato único de erro retornado pela API. Serializado como
/// { "message", "code", "statusCode", "path" }.
/// </summary>
public sealed record ApiError(string Message, string Code, int StatusCode, string Path)
{
    /// <summary>
    /// Cria um ApiError para status gerados pelo próprio framework
    /// (ex.: 401 sem token, 404 rota inexistente), sem passar por um handler.
    /// </summary>
    public static ApiError FromStatus(int statusCode, string path) =>
        statusCode switch
        {
            401 => new("Autenticação necessária.", "UNAUTHORIZED", statusCode, path),
            403 => new("Acesso negado.", "FORBIDDEN", statusCode, path),
            404 => new("Recurso não encontrado.", "NOT_FOUND", statusCode, path),
            405 => new("Método não permitido.", "METHOD_NOT_ALLOWED", statusCode, path),
            _ => new("Ocorreu um erro ao processar a requisição.", "ERROR", statusCode, path),
        };
}
