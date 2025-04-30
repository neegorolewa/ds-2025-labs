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
            ["RU"] = builder.Configuration["ConnectionStrings:DB_RU"] ?? "localhost:6001",
            ["EU"] = builder.Configuration["ConnectionStrings:DB_EU"] ?? "localhost:6002",
            ["ASIA"] = builder.Configuration["ConnectionStrings:DB_ASIA"] ?? "localhost:6003"
        };

        // 2. Инициализируем Redis с поддержкой регионов
        builder.Services.AddSingleton<IRedis>(new Redis(
            builder.Configuration["ConnectionStrings:DB_MAIN"] ?? "localhost:6000",
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
