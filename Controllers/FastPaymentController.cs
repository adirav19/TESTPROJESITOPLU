using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;
using TESTPROJESI.Models;

namespace TESTPROJESI.Controllers
{
    public class FastPaymentController : BaseController
    {
        public FastPaymentController(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IConfiguration config,
            ILogger<FastPaymentController> logger)
            : base(httpClientFactory, cache, config, logger) { }

        public IActionResult Index() => View();
        public IActionResult List()  => View();

        [HttpGet("FastPayment/GetAll")]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
        {
            try
            {
                var http = await CreateApiClientAsync();
                var response = await http.GetAsync($"{ApiBaseUrl}/MixedReceiptsMain?limit={pageSize * 2}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var root = JsonSerializer.Deserialize<JsonElement>(json);

                JsonElement dataArray;
                if (root.ValueKind == JsonValueKind.Object &&
                    root.TryGetProperty("Data", out var inner) && inner.ValueKind == JsonValueKind.Array)
                    dataArray = inner;
                else if (root.ValueKind == JsonValueKind.Array)
                    dataArray = root;
                else
                    return Json(new { data = Array.Empty<object>() });

                var list = new List<FastPaymentListDto>();
                foreach (var item in dataArray.EnumerateArray())
                {
                    list.Add(new FastPaymentListDto
                    {
                        KasaKod            = GetString(item, "KasaKod"),
                        BelgeNo            = GetString(item, "BelgeNo"),
                        CariKod            = GetString(item, "CariKod"),
                        IslemTarihi        = GetString(item, "IslemTarihi"),
                        DOVTIP             = (int)GetDecimal(item, "DOVTIP"),
                        TahsilatKalemAdedi = (int)GetDecimal(item, "TahsilatKalemAdedi")
                    });
                }

                return Json(new { data = list });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FastPayment listesi alinamadi");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("FastPayment/GetDetails")]
        public async Task<IActionResult> GetDetails([FromBody] FastPaymentHeaderInputDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.KasaKod) ||
                string.IsNullOrWhiteSpace(dto.CariKod) || string.IsNullOrWhiteSpace(dto.BelgeNo) ||
                string.IsNullOrWhiteSpace(dto.IslemTarihi))
                return BadRequest(new { success = false, message = "Kasa, Cari, Belge No ve Tarih zorunludur." });

            try
            {
                var http = await CreateApiClientAsync();
                var param = new { dto.BelgeNo, dto.CariKod, IslemTarihi = FormatDateTime(dto.IslemTarihi), dto.KasaKod };
                var content = new StringContent(JsonSerializer.Serialize(param), Encoding.UTF8, "application/json");

                var response = await http.PostAsync($"{ApiBaseUrl}/MixedReceiptsMain/Details", content);
                if (!response.IsSuccessStatusCode)
                    return BadRequest(new { success = false, message = "Belge getirilemedi." });

                var json = await response.Content.ReadAsStringAsync();
                var root = JsonSerializer.Deserialize<JsonElement>(json);
                var data = root.ValueKind == JsonValueKind.Object && root.TryGetProperty("Data", out var d) ? d : root;

                if (data.ValueKind != JsonValueKind.Object)
                    return NotFound(new { success = false, message = "Belge bulunamadı." });

                var viewDto = new FastPaymentViewDto
                {
                    KasaKod     = GetString(data, "KasaKod"),
                    CariKod     = GetString(data, "CariKod"),
                    BelgeNo     = GetString(data, "BelgeNo"),
                    IslemTarihi = GetString(data, "IslemTarihi"),
                    DOVTIP      = (int)GetDecimal(data, "DOVTIP"),
                };

                if (data.TryGetProperty("Tahsilats", out var tahArray) && tahArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in tahArray.EnumerateArray())
                    {
                        viewDto.Tahsilats.Add(new FastPaymentLineDto
                        {
                            SozKodu       = GetString(item, "SozKodu"),
                            TaksitSay     = (int)GetDecimal(item, "TaksitSay"),
                            DovTutar      = GetDecimal(item, "DovTutar"),
                            Kur           = GetDecimal(item, "Kur"),
                            Tutar         = GetDecimal(item, "Tutar"),
                            KartNo        = GetString(item, "KartNo"),
                            CRapKod1      = GetString(item, "CRapKod1"),
                            CRapKod2      = GetString(item, "CRapKod2"),
                            PLA_KODU      = GetString(item, "PLA_KODU"),
                            Proje_Kodu    = GetString(item, "Proje_Kodu"),
                            Referans_Kodu = GetString(item, "Referans_Kodu"),
                            Entegrefkey   = GetString(item, "Entegrefkey"),
                            Aciklama      = GetString(item, "Aciklama"),
                            KasaKod       = GetString(item, "KasaKod"),
                            TahsilatTipi  = (int)GetDecimal(item, "TahsilatTipi")
                        });
                    }
                }

                return Json(new { success = true, data = viewDto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FastPayment GetDetails hatasi");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("FastPayment/Detail/{kasaKod}/{belgeNo}/{cariKod}/{islemTarihi}")]
        public async Task<IActionResult> Detail(string kasaKod, string belgeNo, string cariKod, string islemTarihi)
            => await GetDetails(new FastPaymentHeaderInputDto
            {
                KasaKod = kasaKod, BelgeNo = belgeNo, CariKod = cariKod, IslemTarihi = islemTarihi
            });

        [HttpPost("FastPayment/Create")]
        public async Task<IActionResult> Create([FromBody] FastPaymentSaveDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.KasaKod) ||
                string.IsNullOrWhiteSpace(dto.CariKod) || string.IsNullOrWhiteSpace(dto.BelgeNo) ||
                string.IsNullOrWhiteSpace(dto.IslemTarihi))
                return BadRequest(new { success = false, message = "Kasa, Cari, Belge No ve Tarih zorunludur." });

            if (dto.Tahsilats == null || dto.Tahsilats.Count == 0)
                return BadRequest(new { success = false, message = "En az bir tahsilat satırı girilmelidir." });

            try
            {
                var http = await CreateApiClientAsync();
                var payload = new
                {
                    TransactSupport      = true,
                    MuhasebelesmisBelge  = false,
                    dto.KasaKod,
                    IslemTarihi          = FormatDateTime(dto.IslemTarihi),
                    dto.CariKod,
                    dto.BelgeNo,
                    dto.DOVTIP,
                    TahsilatKalemAdedi   = dto.Tahsilats.Count,
                    BaglantiNo           = 0,
                    Tahsilats = dto.Tahsilats.Select(x => new
                    {
                        SozKodu       = string.IsNullOrWhiteSpace(x.SozKodu) ? "NAKİT" : x.SozKodu,
                        x.TaksitSay, x.DovTutar, x.Kur, x.Tutar,
                        KartNo        = x.KartNo ?? "",
                        CRapKod1      = x.CRapKod1 ?? "",
                        CRapKod2      = x.CRapKod2 ?? "",
                        PLA_KODU      = x.PLA_KODU ?? "",
                        Proje_Kodu    = x.Proje_Kodu ?? "",
                        Referans_Kodu = x.Referans_Kodu ?? "",
                        Entegrefkey   = x.Entegrefkey ?? "",
                        Aciklama      = x.Aciklama ?? "",
                        KasaKod       = string.IsNullOrWhiteSpace(x.KasaKod) ? dto.KasaKod : x.KasaKod,
                        x.TahsilatTipi
                    }).ToList()
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await http.PostAsync($"{ApiBaseUrl}/MixedReceiptsMain", content);
                if (!response.IsSuccessStatusCode)
                    return BadRequest(new { success = false, message = "Kayıt oluşturulamadı." });

                return Ok(new { success = true, message = "Kayıt başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FastPayment Create hatasi");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("FastPayment/Delete")]
        public async Task<IActionResult> Delete([FromBody] FastPaymentHeaderInputDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.KasaKod) ||
                string.IsNullOrWhiteSpace(dto.CariKod) || string.IsNullOrWhiteSpace(dto.BelgeNo) ||
                string.IsNullOrWhiteSpace(dto.IslemTarihi))
                return BadRequest(new { success = false, message = "Kasa, Cari, Belge No ve Tarih zorunludur." });

            try
            {
                var http = await CreateApiClientAsync();
                var param = new { dto.BelgeNo, dto.CariKod, IslemTarihi = FormatDateTime(dto.IslemTarihi), dto.KasaKod };
                var content = new StringContent(JsonSerializer.Serialize(param), Encoding.UTF8, "application/json");
                var response = await http.PostAsync($"{ApiBaseUrl}/MixedReceiptsMain/Delete", content);
                if (!response.IsSuccessStatusCode)
                    return BadRequest(new { success = false, message = "Belge silinemedi." });

                return Ok(new { success = true, message = "Belge başarıyla silindi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FastPayment Delete hatasi");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("FastPayment/UpdateLine")]
        public async Task<IActionResult> UpdateLine([FromBody] FastPaymentUpdateLineDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.KasaKod) ||
                string.IsNullOrWhiteSpace(dto.CariKod) || string.IsNullOrWhiteSpace(dto.BelgeNo) ||
                string.IsNullOrWhiteSpace(dto.IslemTarihi) || string.IsNullOrWhiteSpace(dto.Referans_Kodu))
                return BadRequest(new { success = false, message = "Header alanları ve Referans Kodu zorunludur." });

            try
            {
                var http = await CreateApiClientAsync();
                var param = new { dto.BelgeNo, dto.CariKod, IslemTarihi = FormatDateTime(dto.IslemTarihi), dto.KasaKod };

                // 1. Mevcut belgeyi oku
                var detailsContent = new StringContent(JsonSerializer.Serialize(param), Encoding.UTF8, "application/json");
                var detailsResp = await http.PostAsync($"{ApiBaseUrl}/MixedReceiptsMain/Details", detailsContent);
                if (!detailsResp.IsSuccessStatusCode)
                    return BadRequest(new { success = false, message = "Belge okunamadı." });

                var json = await detailsResp.Content.ReadAsStringAsync();
                var root = JsonSerializer.Deserialize<JsonElement>(json);
                var data = root.ValueKind == JsonValueKind.Object && root.TryGetProperty("Data", out var d) ? d : root;
                if (data.ValueKind != JsonValueKind.Object)
                    return NotFound(new { success = false, message = "Belge bulunamadı." });

                // Satırları güncelle
                var tahsilats = new List<FastPaymentLineDto>();
                if (data.TryGetProperty("Tahsilats", out var tahArray) && tahArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in tahArray.EnumerateArray())
                    {
                        var line = new FastPaymentLineDto
                        {
                            SozKodu       = GetString(item, "SozKodu"),
                            TaksitSay     = (int)GetDecimal(item, "TaksitSay"),
                            DovTutar      = GetDecimal(item, "DovTutar"),
                            Kur           = GetDecimal(item, "Kur"),
                            Tutar         = GetDecimal(item, "Tutar"),
                            KartNo        = GetString(item, "KartNo"),
                            CRapKod1      = GetString(item, "CRapKod1"),
                            CRapKod2      = GetString(item, "CRapKod2"),
                            PLA_KODU      = GetString(item, "PLA_KODU"),
                            Proje_Kodu    = GetString(item, "Proje_Kodu"),
                            Referans_Kodu = GetString(item, "Referans_Kodu"),
                            Entegrefkey   = GetString(item, "Entegrefkey"),
                            Aciklama      = GetString(item, "Aciklama"),
                            KasaKod       = GetString(item, "KasaKod"),
                            TahsilatTipi  = (int)GetDecimal(item, "TahsilatTipi")
                        };
                        if (line.Referans_Kodu == dto.Referans_Kodu && line.Tutar == dto.EskiTutar)
                            line.Tutar = dto.YeniTutar;
                        tahsilats.Add(line);
                    }
                }

                // 2. Eski belgeyi sil
                var deleteContent = new StringContent(JsonSerializer.Serialize(param), Encoding.UTF8, "application/json");
                var deleteResp = await http.PostAsync($"{ApiBaseUrl}/MixedReceiptsMain/Delete", deleteContent);
                if (!deleteResp.IsSuccessStatusCode)
                    return BadRequest(new { success = false, message = "Eski belge silinemedi." });

                // 3. Güncellenmiş belgeyi yeniden oluştur
                var payload = new
                {
                    TransactSupport     = GetBool(data, "TransactSupport"),
                    MuhasebelesmisBelge = GetBool(data, "MuhasebelesmisBelge"),
                    KasaKod             = GetString(data, "KasaKod"),
                    IslemTarihi         = GetString(data, "IslemTarihi"),
                    CariKod             = GetString(data, "CariKod"),
                    BelgeNo             = GetString(data, "BelgeNo"),
                    DOVTIP              = (int)GetDecimal(data, "DOVTIP"),
                    TahsilatKalemAdedi  = tahsilats.Count,
                    BaglantiNo          = (int)GetDecimal(data, "BaglantiNo"),
                    Tahsilats           = tahsilats
                };

                var createContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var createResp = await http.PostAsync($"{ApiBaseUrl}/MixedReceiptsMain", createContent);
                if (!createResp.IsSuccessStatusCode)
                    return BadRequest(new { success = false, message = "Güncellenmiş belge oluşturulamadı." });

                return Ok(new { success = true, message = "Tutar başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FastPayment UpdateLine hatasi");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
