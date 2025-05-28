using Microsoft.AspNetCore.Authentication.Cookies;
using Valuator.Service;

namespace Valuator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var redisConfig = new Dictionary<string, string>
        {
            ["RU"] = Environment.GetEnvironmentVariable("DB_RU"),
            ["EU"] = Environment.GetEnvironmentVariable("DB_EU"),
            ["ASIA"] = Environment.GetEnvironmentVariable("DB_ASIA"),
            ["USERS"] = Environment.GetEnvironmentVariable("DB_USERS"),

        };

        builder.Services.AddSingleton<IRedis>(new Redis(
            Environment.GetEnvironmentVariable("DB_MAIN"),
            redisConfig
        ));

        // Add services to the container.
        builder.Services.AddRazorPages();

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Login";
                options.LogoutPath = "/Logout";

                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();
        app.UseAuthentication();

        app.MapRazorPages();

        app.Run();
    }
}
