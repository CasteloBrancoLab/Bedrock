using System.ComponentModel.DataAnnotations;

namespace ShopDemo.Auth.Api.Models;

public sealed record RegisterRequest(
    [Required] string Email,
    [Required] [MinLength(12)] string Password
);
