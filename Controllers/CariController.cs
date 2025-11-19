using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MinimalProject.Controllers
{
    public class CariController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;
        private readonly ILogger<CariController> _logger;

        public CariController(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IConfiguration config,
            ILogger<CariController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _config = config;
            _logger = logger;
        }

        // Ana sayfa
        public IActionResult Index()
        {
            return View();
        }

        // Token al ve cache'le
        private async Task<string> GetTokenAsync()
        {
            if (_cache.TryGetValue("Token", out string token))
            {
                _logger.LogInformation("🔁 Cache'den token alındı");
                return token;
            }

            _logger.LogInformation("🔐 Yeni token alınıyor...");
            
            // HttpClient'ı baseUrl olmadan oluştur
            var httpClient = _httpClientFactory.CreateClient();
            var baseUrl = _config["NetOpenX:BaseUrl"];

            var loginData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("branchcode", _config["NetOpenX:BranchCode"]),
                new KeyValuePair<string, string>("username", _config["NetOpenX:Username"]),
                new KeyValuePair<string, string>("password", _config["NetOpenX:Password"]),
                new KeyValuePair<string, string>("dbname", _config["NetOpenX:DbName"]),
                new KeyValuePair<string, string>("dbuser", _config["NetOpenX:DbUser"]),
                new KeyValuePair<string, string>("dbpassword", _config["NetOpenX:DbPassword"]),
                new KeyValuePair<string, string>("dbtype", _config["NetOpenX:DbType"])
            });

            var tokenUrl = $"{baseUrl.TrimEnd('/')}/token";
            _logger.LogInformation("📍 Token URL: {Url}", tokenUrl);
            
            var response = await httpClient.PostAsync(tokenUrl, loginData);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("❌ Token alınamadı: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new Exception($"Token alınamadı: {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenObj = JsonSerializer.Deserialize<JsonElement>(json);
            token = tokenObj.GetProperty("access_token").GetString();

            _cache.Set("Token", token, TimeSpan.FromMinutes(20));
            _logger.LogInformation("✅ Token alındı ve cache'lendi");

            return token;
        }

        // Cari listesi (AJAX)
        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                var token = await GetTokenAsync();
                var httpClient = _httpClientFactory.CreateClient();
                var baseUrl = _config["NetOpenX:BaseUrl"];
                
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var url = $"{baseUrl.TrimEnd('/')}/ARPs?limit=50&sort=CARI_KOD ASC";
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(json);

                // Data array'ini çıkar
                if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("Data", out var dataArray))
                    return Json(dataArray);

                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Cari listesi alınamadı");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Cari oluştur (AJAX)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CariDto dto)
        {
            try
            {
                var token = await GetTokenAsync();
                var httpClient = _httpClientFactory.CreateClient();
                var baseUrl = _config["NetOpenX:BaseUrl"];
                
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var body = new
                {
                    CariTemelBilgi = new
                    {
                        dto.CARI_KOD,
                        dto.CARI_ISIM,
                        dto.CARI_TEL,
                        dto.CARI_IL,
                        dto.EMAIL,
                        ISLETME_KODU = 1,
                        CARI_TIP = "A"
                    },
                    CariEkBilgi = new { CARI_KOD = dto.CARI_KOD },
                    SubelerdeOrtak = true,
                    IsletmelerdeOrtak = true,
                    TransactSupport = true,
                    MuhasebelesmisBelge = true
                };

                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{baseUrl.TrimEnd('/')}/ARPs";
                var response = await httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("✅ Cari oluşturuldu: {Kod}", dto.CARI_KOD);
                return Ok(new { success = true, message = "Cari oluşturuldu" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Cari oluşturulamadı");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Cari güncelle (AJAX)
        [HttpPost]
        public async Task<IActionResult> Update([FromBody] CariDto dto)
        {
            try
            {
                var token = await GetTokenAsync();
                var httpClient = _httpClientFactory.CreateClient();
                var baseUrl = _config["NetOpenX:BaseUrl"];
                
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var body = new
                {
                    CariTemelBilgi = new
                    {
                        dto.CARI_KOD,
                        dto.CARI_ISIM,
                        dto.CARI_TEL,
                        dto.CARI_IL,
                        dto.EMAIL
                    },
                    CariEkBilgi = new { CARI_KOD = dto.CARI_KOD },
                    SubelerdeOrtak = true,
                    IsletmelerdeOrtak = true,
                    TransactSupport = true,
                    MuhasebelesmisBelge = true
                };

                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{baseUrl.TrimEnd('/')}/ARPs/{dto.CARI_KOD}";
                var response = await httpClient.PutAsync(url, content);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("✅ Cari güncellendi: {Kod}", dto.CARI_KOD);
                return Ok(new { success = true, message = "Cari güncellendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Cari güncellenemedi");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Cari sil (AJAX)
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] DeleteDto dto)
        {
            try
            {
                var token = await GetTokenAsync();
                var httpClient = _httpClientFactory.CreateClient();
                var baseUrl = _config["NetOpenX:BaseUrl"];
                
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var url = $"{baseUrl.TrimEnd('/')}/ARPs/{dto.cariKodu}";
                var response = await httpClient.DeleteAsync(url);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("✅ Cari silindi: {Kod}", dto.cariKodu);
                return Ok(new { success = true, message = "Cari silindi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Cari silinemedi");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    // DTOs
    public class CariDto
    {
        public string CARI_KOD { get; set; }
        public string CARI_ISIM { get; set; }
        public string CARI_TEL { get; set; }
        public string CARI_IL { get; set; }
        public string EMAIL { get; set; }
    }

    public class DeleteDto
    {
        public string cariKodu { get; set; }
    }
}
