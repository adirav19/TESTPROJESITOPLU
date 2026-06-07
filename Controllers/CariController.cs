using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;
using TESTPROJESI.Models;

namespace TESTPROJESI.Controllers
{
    public class CariController : BaseController
    {
        public CariController(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IConfiguration config,
            ILogger<CariController> logger)
            : base(httpClientFactory, cache, config, logger) { }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                var http = await CreateApiClientAsync();
                var response = await http.GetAsync($"{ApiBaseUrl}/ARPs?limit=50&sort=CARI_KOD ASC");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(json);

                if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("Data", out var dataArray))
                    return Json(dataArray);

                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari listesi alinamadi");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CariDto dto)
        {
            try
            {
                var http = await CreateApiClientAsync();
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

                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var response = await http.PostAsync($"{ApiBaseUrl}/ARPs", content);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Cari olusturuldu: {Kod}", dto.CARI_KOD);
                return Ok(new { success = true, message = "Cari oluşturuldu" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari olusturulamadi");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] CariDto dto)
        {
            try
            {
                var http = await CreateApiClientAsync();
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

                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var response = await http.PutAsync($"{ApiBaseUrl}/ARPs/{dto.CARI_KOD}", content);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Cari guncellendi: {Kod}", dto.CARI_KOD);
                return Ok(new { success = true, message = "Cari güncellendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari guncellenemedi");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] DeleteDto dto)
        {
            try
            {
                var http = await CreateApiClientAsync();
                var response = await http.DeleteAsync($"{ApiBaseUrl}/ARPs/{dto.cariKodu}");
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Cari silindi: {Kod}", dto.cariKodu);
                return Ok(new { success = true, message = "Cari silindi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cari silinemedi");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
