using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MinimalProject.Controllers
{
    public class FinishedGoodsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;
        private readonly ILogger<FinishedGoodsController> _logger;

        public FinishedGoodsController(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IConfiguration config,
            ILogger<FinishedGoodsController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _config = config;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Token al
        private async Task<string> GetTokenAsync()
        {
            if (_cache.TryGetValue("Token", out string token))
            {
                _logger.LogInformation("🔁 Cache'den token alındı");
                return token;
            }

            _logger.LogInformation("🔐 Yeni token alınıyor...");
            
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

        // Liste getir
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var token = await GetTokenAsync();
                var httpClient = _httpClientFactory.CreateClient();
                var baseUrl = _config["NetOpenX:BaseUrl"];
                
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var url = $"{baseUrl.TrimEnd('/')}/FinishedGoodsReceiptWChanges?limit=20";
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var responseJson = JsonSerializer.Deserialize<JsonElement>(json);

                var list = new List<FinishedGoodsDto>();

                JsonElement dataArray;
                if (responseJson.ValueKind == JsonValueKind.Object &&
                    responseJson.TryGetProperty("Data", out var innerData) && innerData.ValueKind == JsonValueKind.Array)
                {
                    dataArray = innerData;
                }
                else if (responseJson.ValueKind == JsonValueKind.Array)
                {
                    dataArray = responseJson;
                }
                else
                {
                    return Json(new { data = new List<FinishedGoodsDto>() });
                }

                foreach (var item in dataArray.EnumerateArray())
                {
                    list.Add(new FinishedGoodsDto
                    {
                        FisNo = GetString(item, "UretSon_FisNo"),
                        Tarih = GetString(item, "UretSon_Tarih"),
                        Depo = GetString(item, "UretSon_Depo"),
                        Malzeme = GetString(item, "UretSon_Mamul"),
                        Miktar = GetDecimal(item, "UretSon_Miktar"),
                        Birim = "Adet"
                    });
                }

                return Json(new { data = list });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Liste alınamadı");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Detay getir
        [HttpGet("FinishedGoods/Detail/{fisNo}")]
        public async Task<IActionResult> Detail(string fisNo)
        {
            try
            {
                var token = await GetTokenAsync();
                var httpClient = _httpClientFactory.CreateClient();
                var baseUrl = _config["NetOpenX:BaseUrl"];
                
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var url = $"{baseUrl.TrimEnd('/')}/FinishedGoodsReceiptWChanges/{fisNo}";
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var responseJson = JsonSerializer.Deserialize<JsonElement>(json);

                if (responseJson.ValueKind != JsonValueKind.Object ||
                    !responseJson.TryGetProperty("Data", out var data))
                {
                    return NotFound(new { success = false, message = "Fiş bulunamadı" });
                }

                var dto = new FinishedGoodsDetailDto
                {
                    UretSon_FisNo = GetString(data, "UretSon_FisNo"),
                    UretSon_Tarih = GetString(data, "UretSon_Tarih"),
                    UretSon_SipNo = GetString(data, "UretSon_SipNo"),
                    UretSon_Mamul = GetString(data, "UretSon_Mamul"),
                    UretSon_Miktar = GetDecimal(data, "UretSon_Miktar"),
                    UretSon_Depo = (int)GetDecimal(data, "UretSon_Depo"),
                    Aciklama = GetString(data, "Aciklama"),
                    KayitYapanKul = GetString(data, "KayitYapanKul"),
                    Kalem = new List<KalemDto>()
                };

                if (data.TryGetProperty("Kalem", out var kalemArray) && kalemArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in kalemArray.EnumerateArray())
                    {
                        dto.Kalem.Add(new KalemDto
                        {
                            Index = (int)GetDecimal(item, "Index"),
                            IncKeyNo = (int)GetDecimal(item, "IncKeyNo"),
                            StokKodu = GetString(item, "StokKodu"),
                            DepoKodu = (int)GetDecimal(item, "DepoKodu"),
                            Miktar = (double)GetDecimal(item, "Miktar"),
                            Aciklama = GetString(item, "Aciklama"),
                            SeriVarMi = GetBool(item, "SeriVarMi"),
                            BGTIP = GetString(item, "BGTIP")
                        });
                    }
                }

                return Json(new { success = true, data = dto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Detay alınamadı");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Miktar güncelle
        [HttpPost("FinishedGoods/UpdateQuantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] KalemDto dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.FisNo))
                    return BadRequest(new { success = false, message = "Fiş numarası gerekli" });

                var token = await GetTokenAsync();
                var httpClient = _httpClientFactory.CreateClient();
                var baseUrl = _config["NetOpenX:BaseUrl"];
                
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Önce mevcut fişi al
                var getUrl = $"{baseUrl.TrimEnd('/')}/FinishedGoodsReceiptWChanges/{dto.FisNo}";
                var getResponse = await httpClient.GetAsync(getUrl);
                getResponse.EnsureSuccessStatusCode();

                var getJson = await getResponse.Content.ReadAsStringAsync();
                var current = JsonSerializer.Deserialize<JsonElement>(getJson);

                if (!current.TryGetProperty("Data", out var data))
                    return NotFound(new { success = false, message = "Fiş bulunamadı" });

                // Kalem listesini güncelle
                var kalemList = new List<KalemDto>();
                if (data.TryGetProperty("Kalem", out var kalemArray) && kalemArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in kalemArray.EnumerateArray())
                    {
                        var kalem = new KalemDto
                        {
                            Index = (int)GetDecimal(item, "Index"),
                            IncKeyNo = (int)GetDecimal(item, "IncKeyNo"),
                            StokKodu = GetString(item, "StokKodu"),
                            DepoKodu = (int)GetDecimal(item, "DepoKodu"),
                            Miktar = (double)GetDecimal(item, "Miktar"),
                            Aciklama = GetString(item, "Aciklama"),
                            SeriVarMi = GetBool(item, "SeriVarMi"),
                            BGTIP = GetString(item, "BGTIP")
                        };

                        // Eşleşen kalemi güncelle
                        if (kalem.StokKodu == dto.StokKodu)
                            kalem.Miktar = dto.Miktar;

                        kalemList.Add(kalem);
                    }
                }

                // Save endpoint'ine gönder
                var payload = new
                {
                    UretSon_FisNo = GetString(data, "UretSon_FisNo"),
                    UretSon_Tarih = GetString(data, "UretSon_Tarih"),
                    UretSon_Depo = (int)GetDecimal(data, "UretSon_Depo"),
                    UretSon_Mamul = GetString(data, "UretSon_Mamul"),
                    UretSon_Miktar = GetDecimal(data, "UretSon_Miktar"),
                    Aciklama = GetString(data, "Aciklama"),
                    KayitYapanKul = GetString(data, "KayitYapanKul"),
                    Kalem = kalemList
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var saveUrl = $"{baseUrl.TrimEnd('/')}/FinishedGoodsReceiptWChanges/Save";
                var response = await httpClient.PostAsync(saveUrl, content);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("✅ Miktar güncellendi: {StokKodu}", dto.StokKodu);
                return Ok(new { success = true, message = "Miktar güncellendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Miktar güncellenemedi");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Yardımcı metodlar
        private string GetString(JsonElement item, string propName)
        {
            if (item.TryGetProperty(propName, out var val))
            {
                return val.ValueKind switch
                {
                    JsonValueKind.String => val.GetString(),
                    JsonValueKind.Number => val.GetRawText(),
                    _ => ""
                };
            }
            return "";
        }

        private decimal GetDecimal(JsonElement item, string propName)
        {
            if (item.TryGetProperty(propName, out var val))
            {
                if (val.ValueKind == JsonValueKind.Number && val.TryGetDecimal(out var num))
                    return num;

                if (val.ValueKind == JsonValueKind.String && decimal.TryParse(val.GetString(), out var strNum))
                    return strNum;
            }
            return 0;
        }

        private bool GetBool(JsonElement item, string propName)
        {
            if (item.TryGetProperty(propName, out var val))
            {
                if (val.ValueKind == JsonValueKind.True) return true;
                if (val.ValueKind == JsonValueKind.False) return false;
            }
            return false;
        }
    }

    // DTOs
    public class FinishedGoodsDto
    {
        public string FisNo { get; set; }
        public string Tarih { get; set; }
        public string Depo { get; set; }
        public string Malzeme { get; set; }
        public decimal Miktar { get; set; }
        public string Birim { get; set; }
    }

    public class FinishedGoodsDetailDto
    {
        public string UretSon_FisNo { get; set; }
        public string UretSon_Tarih { get; set; }
        public string UretSon_SipNo { get; set; }
        public string UretSon_Mamul { get; set; }
        public decimal UretSon_Miktar { get; set; }
        public int UretSon_Depo { get; set; }
        public string Aciklama { get; set; }
        public string KayitYapanKul { get; set; }
        public List<KalemDto> Kalem { get; set; }
    }

    public class KalemDto
    {
        public string FisNo { get; set; }
        public int Index { get; set; }
        public int IncKeyNo { get; set; }
        public string StokKodu { get; set; }
        public int DepoKodu { get; set; }
        public double Miktar { get; set; }
        public string Aciklama { get; set; }
        public bool SeriVarMi { get; set; }
        public string BGTIP { get; set; }
    }
}
