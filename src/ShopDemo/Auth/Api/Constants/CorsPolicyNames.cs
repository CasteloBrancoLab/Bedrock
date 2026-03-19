namespace ShopDemo.Auth.Api.Constants;

// Nomes das politicas CORS especificas da Auth API.
// Usados no Bootstrapper (registro) e referenciados via [EnableCors] se necessario.
public static class CorsPolicyNames
{
    public const string Default = "auth-default";
}
