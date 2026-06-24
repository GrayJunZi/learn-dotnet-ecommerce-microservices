namespace Identity.API.DTOs;

public record RegisterDto(
    string Name,
    string Email,
    string Password);