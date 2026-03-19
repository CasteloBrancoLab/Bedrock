namespace ShopDemo.Auth.Api.Constants;

// Nomes das politicas de rate limiting especificas da Auth API.
// Usados no Bootstrapper (registro) e nas controllers (aplicacao via [EnableRateLimiting]).
public static class RateLimitPolicyNames
{
    public const string Login = "auth-login";
    public const string Register = "auth-register";
}
