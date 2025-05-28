using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Valuator.Service;

namespace Valuator.Pages;

public class RegisterModel : PageModel
{
    private readonly IRedis _redisService;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(IRedis redisService, ILogger<RegisterModel> logger)
    {
        _redisService = redisService;
        _logger = logger;
    }

    public string ErrorMessage { get; set; } = string.Empty;

    public async Task<IActionResult> OnPostAsync(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ErrorMessage = "Логин и пароль обязательны";
            return Page();
        }

        // Проверяем существование пользователя (регион USERS)
        string existingUser = _redisService.Get($"USER-{username}", "USERS");
        if (!string.IsNullOrEmpty(existingUser))
        {
            ErrorMessage = "Пользователь с таким логином уже существует";
            _logger.LogWarning($"Registration failed: user {username} already exists");
            return Page();
        }

        // Хешируем пароль
        string passwordHash = HashPassword(password);

        // Сохраняем пользователя в Redis (регион USERS)
        _redisService.Set($"USER-{username}", passwordHash, "USERS");
        _logger.LogInformation($"User {username} registered successfully");

        // Автоматический вход после регистрации
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username)
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));

        return RedirectToPage("/Index");
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}