namespace Identity.API.DTOs;

public record LoginDto(
    string Email,
    string Password);