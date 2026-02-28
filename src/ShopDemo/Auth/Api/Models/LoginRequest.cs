using System.ComponentModel.DataAnnotations;

namespace ShopDemo.Auth.Api.Models;

public sealed record LoginRequest(
    [Required] string Email,
    [Required] string Password
);
