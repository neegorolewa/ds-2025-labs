using Valuator.Service;

namespace Valuator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Загружаем конфигурацию для всех шардов
        var redisConfig = new Dictionary<string, string>
        {
            ["RU"] = Environment.GetEnvironmentVariable("DB_RU"),
            ["EU"] = Environment.GetEnvironmentVariable("DB_EU"),
            ["ASIA"] = Environment.GetEnvironmentVariable("DB_ASIA"),
        };

        // 2. Инициализируем Redis с поддержкой регионов
        builder.Services.AddSingleton<IRedis>(new Redis(
            Environment.GetEnvironmentVariable("DB_MAIN"),
            redisConfig
        ));

        // Add services to the container.
        builder.Services.AddRazorPages();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }
}
