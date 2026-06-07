using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;
using TESTPROJESI.Models;

namespace TESTPROJESI.Controllers
{
    public class BOMController : BaseController
    {
        public BOMController(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IConfiguration config,
            ILogger<BOMController> logger)
            : base(httpClientFactory, cache, config, logger) { }

        public IActionResult Index() => View();
        public IActionResult New()   => View();

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var http = await CreateApiClientAsync();
                var response = await http.GetAsync($"{ApiBaseUrl}/BOM?limit=50");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var root = JsonSerializer.Deserialize<JsonElement>(json);

                var list = new List<BOMDto>();
                if (root.TryGetProperty("Data", out var dataArray) && dataArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in dataArray.EnumerateArray())
                    {
                        list.Add(new BOMDto
                        {
                            MamulKodu     = GetString(item, "PrmMamulKodu"),
                            ReceteToplami = GetDecimal(item, "ReceteToplami"),
                            OlcuBirimi    = GetString(item, "OlcuBirimi")
                        });
                    }
                }

                return Json(new { data = list });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BOM listesi alinamadi");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("BOM/Detail/{mamulKodu}")]
        public async Task<IActionResult> Detail(string mamulKodu)
        {
            try
            {
                var http = await CreateApiClientAsync();
                var response = await http.GetAsync($"{ApiBaseUrl}/BOM/{mamulKodu}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var root = JsonSerializer.Deserialize<JsonElement>(json);

                if (!root.TryGetProperty("Data", out var data))
                    return NotFound(new { success = false, message = "Reçete bulunamadı" });

                var dto = new BOMDetailDto
                {
                    PrmMamulKodu  = GetString(data, "PrmMamulKodu"),
                    MamulYapKod   = GetString(data, "MamulYapKod"),
                    ReceteToplami = GetDecimal(data, "ReceteToplami"),
                    OlcuBirimi    = GetString(data, "OlcuBirimi"),
                    ReceteSayisi  = (int)GetDecimal(data, "ReceteSayisi"),
                    PrmOprBil     = GetString(data, "PrmOprBil"),
                };

                if (data.TryGetProperty("BOMItemList", out var itemArray) && itemArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in itemArray.EnumerateArray())
                    {
                        dto.BOMItemList.Add(new BOMItemDto
                        {
                            Mamul_Kodu = GetString(item, "Mamul_Kodu"),
                            Ham_Kodu   = GetString(item, "Ham_Kodu"),
                            Miktar     = GetDecimal(item, "Miktar"),
                            OpNo       = GetString(item, "OpNo"),
                            Opr_Bil    = GetString(item, "Opr_Bil"),
                            H_Stok_Adi = GetString(item, "H_Stok_Adi"),
                            H_Olcu_Br1 = GetString(item, "H_Olcu_Br1"),
                            FireMik    = GetDecimal(item, "FireMik"),
                            Aciklama   = GetString(item, "Aciklama")
                        });
                    }
                }

                return Json(new { success = true, data = dto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BOM detayi alinamadi");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("BOM/Create")]
        public async Task<IActionResult> Create([FromBody] BOMCreateDto dto)
        {
            if (string.IsNullOrEmpty(dto.PrmMamulKodu))
                return BadRequest(new { success = false, message = "Mamul kodu gerekli" });
            if (dto.BOMItemList == null || dto.BOMItemList.Count == 0)
                return BadRequest(new { success = false, message = "En az 1 kalem eklemelisiniz" });

            try
            {
                var http = await CreateApiClientAsync();

                var itemList = dto.BOMItemList.Select((item, i) => new
                {
                    item.OpNo,
                    item.Opr_Bil,
                    item.Ham_Kodu,
                    item.Miktar,
                    SonOperasyon = (i == dto.BOMItemList.Count - 1)
                }).ToList<object>();

                var payload = new
                {
                    PrmMamulKodu  = dto.PrmMamulKodu,
                    ReceteToplami = dto.ReceteToplami,
                    BOMItemList   = itemList
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await http.PostAsync($"{ApiBaseUrl}/BOM", content);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Recete olusturuldu: {MamulKodu}", dto.PrmMamulKodu);
                return Ok(new { success = true, message = "Reçete başarıyla oluşturuldu" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Recete olusturulamadi");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("BOM/UpdateBOM")]
        public async Task<IActionResult> UpdateBOM([FromBody] BOMUpdateFullDto dto)
        {
            try
            {
                var http = await CreateApiClientAsync();
                var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                var response = await http.PutAsync($"{ApiBaseUrl}/BOM/{dto.PrmMamulKodu}", content);
                response.EnsureSuccessStatusCode();

                return Ok(new { success = true, message = "BOM güncellendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BOM guncellenemedi");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("BOM/UpdateReceteToplami")]
        public async Task<IActionResult> UpdateReceteToplami([FromBody] BOMUpdateReceteDto dto)
        {
            try
            {
                var http = await CreateApiClientAsync();

                // Mevcut reçeteyi al
                var getResponse = await http.GetAsync($"{ApiBaseUrl}/BOM/{dto.MamulKodu}");
                getResponse.EnsureSuccessStatusCode();
                var getJson = await getResponse.Content.ReadAsStringAsync();
                var current = JsonSerializer.Deserialize<JsonElement>(getJson);

                if (!current.TryGetProperty("Data", out var data))
                    return NotFound(new { success = false, message = "Reçete bulunamadı" });

                // Kalemleri topla
                var itemList = new List<object>();
                if (data.TryGetProperty("BOMItemList", out var itemArray) && itemArray.ValueKind == JsonValueKind.Array)
                {
                    int total = itemArray.GetArrayLength(), idx = 0;
                    foreach (var item in itemArray.EnumerateArray())
                    {
                        idx++;
                        itemList.Add(new
                        {
                            OpNo         = GetString(item, "OpNo"),
                            Opr_Bil      = GetString(item, "Opr_Bil"),
                            Ham_Kodu     = GetString(item, "Ham_Kodu"),
                            Miktar       = GetDecimal(item, "Miktar"),
                            SonOperasyon = (idx == total)
                        });
                    }
                }

                // Sil + yeniden oluştur
                await http.DeleteAsync($"{ApiBaseUrl}/BOM/{dto.MamulKodu}");

                var payload = new
                {
                    PrmMamulKodu  = dto.MamulKodu,
                    ReceteToplami = dto.YeniReceteToplami,
                    BOMItemList   = itemList
                };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var postResponse = await http.PostAsync($"{ApiBaseUrl}/BOM", content);
                postResponse.EnsureSuccessStatusCode();

                return Ok(new { success = true, message = "Reçete toplamı güncellendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReceteToplami guncellenemedi");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("BOM/UpdateQuantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] BOMItemUpdateDto dto)
        {
            try
            {
                var http = await CreateApiClientAsync();

                var getResponse = await http.GetAsync($"{ApiBaseUrl}/BOM/{dto.MamulKodu}");
                getResponse.EnsureSuccessStatusCode();
                var getJson = await getResponse.Content.ReadAsStringAsync();
                var current = JsonSerializer.Deserialize<JsonElement>(getJson);

                if (!current.TryGetProperty("Data", out var data))
                    return NotFound(new { success = false, message = "Reçete bulunamadı" });

                var itemList = new List<object>();
                if (data.TryGetProperty("BOMItemList", out var itemArray) && itemArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in itemArray.EnumerateArray())
                    {
                        var hamKodu = GetString(item, "Ham_Kodu");
                        var miktar  = hamKodu == dto.Ham_Kodu ? dto.YeniMiktar : GetDecimal(item, "Miktar");

                        itemList.Add(new
                        {
                            Mamul_Kodu = GetString(item, "Mamul_Kodu"),
                            Ham_Kodu   = hamKodu,
                            Miktar     = miktar,
                            FireMik    = GetDecimal(item, "FireMik"),
                            OpNo       = GetString(item, "OpNo"),
                            Opr_Bil    = GetString(item, "Opr_Bil"),
                            IncKeyNo   = (int)GetDecimal(item, "IncKeyNo")
                        });
                    }
                }

                var payload = new
                {
                    PrmMamulKodu  = GetString(data, "PrmMamulKodu"),
                    ReceteToplami = GetDecimal(data, "ReceteToplami"),
                    OlcuBirimi    = GetString(data, "OlcuBirimi"),
                    BOMItemList   = itemList
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await http.PutAsync($"{ApiBaseUrl}/BOM/{dto.MamulKodu}", content);
                response.EnsureSuccessStatusCode();

                return Ok(new { success = true, message = "Miktar güncellendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BOM kalem miktari guncellenemedi");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("BOM/DeleteItem")]
        public async Task<IActionResult> DeleteItem([FromBody] DeleteBOMItemDto dto)
        {
            try
            {
                var http = await CreateApiClientAsync();
                var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                var response = await http.PostAsync($"{ApiBaseUrl}/BOM/DeleteBOMItem", content);
                response.EnsureSuccessStatusCode();

                return Ok(new { success = true, message = "Kalem silindi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BOM kalemi silinemedi");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
