namespace Bedrock.BuildingBlocks.Web.Logging.Middlewares;

// Mascara valores de headers e parametros sensiveis nos logs.
// Substitui o valor por "[REDACTED]" se o nome do header esta na lista de sensiveis.
// Comparacao case-insensitive.
internal static class SensitiveDataMasker
{
    private const string RedactedValue = "[REDACTED]";

    internal static string MaskHeaderValue(string headerName, string? value, HashSet<string> sensitiveHeaders)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return sensitiveHeaders.Contains(headerName) ? RedactedValue : value;
    }

    internal static Dictionary<string, string> MaskHeaders(
        IEnumerable<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>> headers,
        HashSet<string> sensitiveHeaders)
    {
        var masked = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var header in headers)
        {
            masked[header.Key] = sensitiveHeaders.Contains(header.Key)
                ? RedactedValue
                : header.Value.ToString();
        }

        return masked;
    }
}
