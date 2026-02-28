namespace ShopDemo.Auth.Api.Models;

public sealed record ErrorResponse(
    string Code,
    string Message
);
