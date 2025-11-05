using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using TESTPROJESI.Services.Interfaces;

namespace TESTPROJESI.Services.Implementations
{
    /// <summary>
    /// 🌐 Tüm HTTP işlemlerini yöneten temel servis.
    /// GET, POST, PUT, DELETE işlemlerini güvenli şekilde yapar.
    /// </summary>
    public class BaseApiService : IBaseApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BaseApiService> _logger;
        private readonly string _baseUrl;

        public BaseApiService(HttpClient httpClient, IConfiguration config, ILogger<BaseApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // appsettings.json içinden BaseUrl okunur
            _baseUrl = config["NetOpenX:BaseUrl"] ?? throw new Exception("❌ BaseUrl appsettings içinde bulunamadı!");
        }

        // ---------------------------------------------------
        // 🔸 Genel hata yönetimi (tüm HTTP istekleri buradan geçer)
        // ---------------------------------------------------
        private async Task<T?> SafeExecuteAsync<T>(Func<Task<HttpResponseMessage>> action, string endpoint)
        {
            try
            {
                // 🔗 URL temizlenir (çift slash engellenir)
                var fullUrl = $"{_baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
                _logger.LogInformation("🌍 İstek atılıyor: {Url}", fullUrl);

                var response = await action.Invoke();

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ HTTP Hatası ({Status}): {Error}", response.StatusCode, err);
                    throw new HttpRequestException($"İstek başarısız: {response.StatusCode}");
                }

                var json = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(json))
                    return default;

                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "🌐 HTTP isteği başarısız: {Message}", ex.Message);
                throw new Exception($"İstek başarısız: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Beklenmeyen hata: {Message}", ex.Message);
                throw;
            }
        }

        // ---------------------------------------------------
        // 🔹 GET
        // ---------------------------------------------------
        public async Task<T?> GetAsync<T>(string endpoint, string token = null)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await SafeExecuteAsync<T>(() => _httpClient.GetAsync($"{_baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}"), endpoint);
        }

        // ---------------------------------------------------
        // 🔹 POST
        // ---------------------------------------------------
        public async Task<T?> PostAsync<T>(string endpoint, object data, string token = null)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await SafeExecuteAsync<T>(() => _httpClient.PostAsync($"{_baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}", content), endpoint);
        }

        // ---------------------------------------------------
        // 🔹 PUT
        // ---------------------------------------------------
        public async Task<T?> PutAsync<T>(string endpoint, object data, string token = null)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await SafeExecuteAsync<T>(() => _httpClient.PutAsync($"{_baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}", content), endpoint);
        }

        // ---------------------------------------------------
        // 🔹 DELETE
        // ---------------------------------------------------
        public async Task<bool> DeleteAsync(string endpoint, string token = null)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var result = await SafeExecuteAsync<HttpResponseMessage>(() => _httpClient.DeleteAsync($"{_baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}"), endpoint);
            return result?.IsSuccessStatusCode ?? false;
        }
    }
}
