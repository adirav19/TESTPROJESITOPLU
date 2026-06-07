using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;
using TESTPROJESI.Models;

namespace TESTPROJESI.Controllers
{
    public class FinishedGoodsController : BaseController
    {
        public FinishedGoodsController(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IConfiguration config,
            ILogger<FinishedGoodsController> logger)
            : base(httpClientFactory, cache, config, logger) { }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var http = await CreateApiClientAsync();
                var response = await http.GetAsync($"{ApiBaseUrl}/FinishedGoodsReceiptWChanges?limit=50");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var root = JsonSerializer.Deserialize<JsonElement>(json);

                JsonElement dataArray;
                if (root.ValueKind == JsonValueKind.Object &&
                    root.TryGetProperty("Data", out var inner) &&
                    inner.ValueKind == JsonValueKind.Array)
                    dataArray = inner;
                else if (root.ValueKind == JsonValueKind.Array)
                    dataArray = root;
                else
                    return Json(new { data = Array.Empty<object>() });

                var list = new List<FinishedGoodsDto>();
                foreach (var item in dataArray.EnumerateArray())
                {
                    list.Add(new FinishedGoodsDto
                    {
                        FisNo   = GetString(item, "UretSon_FisNo"),
                        Tarih   = GetString(item, "UretSon_Tarih"),
                        Depo    = GetString(item, "UretSon_Depo"),
                        Malzeme = GetString(item, "UretSon_Mamul"),
                        Miktar  = GetDecimal(item, "UretSon_Miktar")
                    });
                }

                return Json(new { data = list });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FinishedGoods listesi alinamadi");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("FinishedGoods/Detail/{fisNo}")]
        public async Task<IActionResult> Detail(string fisNo)
        {
            try
            {
                var http = await CreateApiClientAsync();
                var response = await http.GetAsync($"{ApiBaseUrl}/FinishedGoodsReceiptWChanges/{fisNo}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var root = JsonSerializer.Deserialize<JsonElement>(json);

                if (!root.TryGetProperty("Data", out var data))
                    return NotFound(new { success = false, message = "Fiş bulunamadı" });

                var dto = new FinishedGoodsDetailDto
                {
                    UretSon_FisNo  = GetString(data, "UretSon_FisNo"),
                    UretSon_Tarih  = GetString(data, "UretSon_Tarih"),
                    UretSon_SipNo  = GetString(data, "UretSon_SipNo"),
                    UretSon_Mamul  = GetString(data, "UretSon_Mamul"),
                    UretSon_Miktar = GetDecimal(data, "UretSon_Miktar"),
                    UretSon_Depo   = (int)GetDecimal(data, "UretSon_Depo"),
                    Aciklama       = GetString(data, "Aciklama"),
                    KayitYapanKul  = GetString(data, "KayitYapanKul"),
                };

                if (data.TryGetProperty("Kalem", out var kalemArray) && kalemArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in kalemArray.EnumerateArray())
                    {
                        dto.Kalem.Add(new KalemDto
                        {
                            Index     = (int)GetDecimal(item, "Index"),
                            IncKeyNo  = (int)GetDecimal(item, "IncKeyNo"),
                            StokKodu  = GetString(item, "StokKodu"),
                            DepoKodu  = (int)GetDecimal(item, "DepoKodu"),
                            Miktar    = (double)GetDecimal(item, "Miktar"),
                            Aciklama  = GetString(item, "Aciklama"),
                            SeriVarMi = GetBool(item, "SeriVarMi"),
                            BGTIP     = GetString(item, "BGTIP")
                        });
                    }
                }

                return Json(new { success = true, data = dto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FinishedGoods detayi alinamadi");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("FinishedGoods/UpdateQuantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] KalemDto dto)
        {
            if (string.IsNullOrEmpty(dto.FisNo))
                return BadRequest(new { success = false, message = "Fiş numarası gerekli" });

            try
            {
                var http = await CreateApiClientAsync();

                // Mevcut fişi al
                var getResponse = await http.GetAsync($"{ApiBaseUrl}/FinishedGoodsReceiptWChanges/{dto.FisNo}");
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
                            Index     = (int)GetDecimal(item, "Index"),
                            IncKeyNo  = (int)GetDecimal(item, "IncKeyNo"),
                            StokKodu  = GetString(item, "StokKodu"),
                            DepoKodu  = (int)GetDecimal(item, "DepoKodu"),
                            Miktar    = (double)GetDecimal(item, "Miktar"),
                            Aciklama  = GetString(item, "Aciklama"),
                            SeriVarMi = GetBool(item, "SeriVarMi"),
                            BGTIP     = GetString(item, "BGTIP")
                        };
                        if (kalem.StokKodu == dto.StokKodu)
                            kalem.Miktar = dto.Miktar;
                        kalemList.Add(kalem);
                    }
                }

                var payload = new
                {
                    UretSon_FisNo  = GetString(data, "UretSon_FisNo"),
                    UretSon_Tarih  = GetString(data, "UretSon_Tarih"),
                    UretSon_Depo   = (int)GetDecimal(data, "UretSon_Depo"),
                    UretSon_Mamul  = GetString(data, "UretSon_Mamul"),
                    UretSon_Miktar = GetDecimal(data, "UretSon_Miktar"),
                    Aciklama       = GetString(data, "Aciklama"),
                    KayitYapanKul  = GetString(data, "KayitYapanKul"),
                    Kalem          = kalemList
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var saveResponse = await http.PostAsync($"{ApiBaseUrl}/FinishedGoodsReceiptWChanges/Save", content);
                saveResponse.EnsureSuccessStatusCode();

                _logger.LogInformation("Miktar guncellendi: {StokKodu}", dto.StokKodu);
                return Ok(new { success = true, message = "Miktar güncellendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Miktar guncellenemedi");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
