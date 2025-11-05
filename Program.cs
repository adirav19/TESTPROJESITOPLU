using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using System;
using System.Net.Http;
using TESTPROJESI.Middlewares;
using TESTPROJESI.Services.Implementations;
using TESTPROJESI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

//
// 🧩 1️⃣ SERILOG yapılandırması (console + dosya)
//
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("Logs/app_log_.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 10, // 10 günlük log tut
        shared: true,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

//
// 🌐 2️⃣ MVC + MemoryCache
//
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

//
// ⚙️ 3️⃣ Polly (Retry + Timeout)
//
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() =>
    HttpPolicyExtensions
        .HandleTransientHttpError()
        .Or<TaskCanceledException>()
        .WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(Math.Pow(2, retry)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Log.Warning($"🌐 Retry {retryCount} - {timespan.TotalSeconds}s sonra tekrar denenecek...");
            });

//
// 🧠 4️⃣ HttpClient servisleri
//
builder.Services.AddHttpClient<NetOpenXService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(GetRetryPolicy());

builder.Services.AddHttpClient<BaseApiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(GetRetryPolicy());

//
// 💡 5️⃣ Interface bazlı bağımlılıklar
//
builder.Services.AddScoped<ITokenManager, TokenManager>();
builder.Services.AddScoped<IBaseApiService, BaseApiService>();
builder.Services.AddScoped<INetOpenXService, NetOpenXService>();
builder.Services.AddScoped<ICarilerService, CarilerService>();
builder.Services.AddScoped<IFinishedGoodsService, FinishedGoodsService>();
var app = builder.Build();

//
// 🚦 6️⃣ Ortam ayarları
//
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//
// 💥 7️⃣ Global hata yakalama middleware’i
//
app.UseMiddleware<ErrorHandlingMiddleware>();

//
// 🌍 8️⃣ Pipeline
//
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

//
// 🧭 9️⃣ Varsayılan rota
//
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

try
{
    Log.Information("🚀 Uygulama başlatılıyor...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Uygulama başlatılamadı!");
}
finally
{
    Log.CloseAndFlush();
}
