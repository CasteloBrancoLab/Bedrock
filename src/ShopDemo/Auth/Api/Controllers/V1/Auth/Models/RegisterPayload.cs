using System.ComponentModel.DataAnnotations;

namespace ShopDemo.Auth.Api.Controllers.V1.Auth.Models;

public sealed record RegisterPayload(
    [Required] string Email,
    [Required] [MinLength(12)] string Password
);
