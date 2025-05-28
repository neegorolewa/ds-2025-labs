using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class LoginModel : PageModel
{
    private readonly Service.IRedis _redisService;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(Service.IRedis redisService, ILogger<LoginModel> logger)
    {
        _redisService = redisService;
        _logger = logger;
    }

    public string ErrorMessage { get; set; } = string.Empty;

    public async Task<IActionResult> OnPostAsync(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ErrorMessage = "Username and password can't be empty or null";
            return Page();
        }

        string storedHash = _redisService.Get($"USER-{username}", "USERS");

        if (string.IsNullOrEmpty(storedHash))
        {
            ErrorMessage = "Неверный логин или пароль";
            _logger.LogWarning($"Login failed for user {username}: user not found");
            return Page();
        }

        // Проверяем пароль
        string inputHash = HashPassword(password);
        if (inputHash != storedHash)
        {
            ErrorMessage = "Неверный логин или пароль";
            _logger.LogWarning($"Login failed for user {username}: invalid password");
            return Page();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username)
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));

        _logger.LogInformation($"User {username} logged in successfully");

        return RedirectToPage("/Index");
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
