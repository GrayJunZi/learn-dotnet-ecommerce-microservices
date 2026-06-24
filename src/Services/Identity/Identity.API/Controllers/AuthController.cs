using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Identity.API.DTOs;
using Identity.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IConfiguration configuration,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto register)
    {
        var user = new ApplicationUser
        {
            UserName = register.Email,
            Email = register.Email,
            Name = register.Name,
        };

        var result = await userManager.CreateAsync(user, register.Password);
        logger.LogInformation($"User {register.Email} registration attempted.");

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { Message = "Registration successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto login)
    {
        var user = await userManager.FindByEmailAsync(login.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, login.Password))
            return Unauthorized();

        var token = generateToken(user);
        logger.LogInformation($"User {login.Email} logged in successfully.");
        return Ok(new { Token = token });
    }

    private string generateToken(ApplicationUser user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("uid", user.Id),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(configuration["Jwt:DurationInMinutes"])),
            signingCredentials: credentials
        );

        logger.LogInformation($"JWT Token Generated for user {user.Email}");
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}