using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;

namespace TESTPROJESI.Controllers
{
    public class DepoTransferController : BaseController
    {
        public DepoTransferController(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IConfiguration config,
            ILogger<DepoTransferController> logger)
            : base(httpClientFactory, cache, config, logger) { }

        public async Task<IActionResult> List(int page = 1, int pageSize = 20)
        {
            try
            {
                int offset = (page - 1) * pageSize;
                var http = await CreateApiClientAsync();
                var response = await http.GetAsync($"{ApiBaseUrl}/ItemSlips?limit={pageSize}&offset={offset}&docType=5");
                var json = await response.Content.ReadAsStringAsync();

                dynamic root = JsonConvert.DeserializeObject(json)!;

                ViewBag.TotalCount = (root == null || root.IsSuccessful != true) ? 0 : (int)(root.TotalCount ?? 0);
                ViewBag.Page       = page;
                ViewBag.PageSize   = pageSize;

                if (root == null || root.IsSuccessful != true)
                    return View(new List<dynamic>());

                var list = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(root.Data));
                return View(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DepoTransfer LIST hatasi");
                ViewBag.TotalCount = 0; ViewBag.Page = page; ViewBag.PageSize = pageSize;
                return View(new List<dynamic>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFull(string fisNo)
        {
            if (string.IsNullOrWhiteSpace(fisNo))
                return BadRequest("Fiş numarası boş olamaz.");
            try
            {
                var http = await CreateApiClientAsync();
                var resp = await http.GetAsync($"{ApiBaseUrl}/ItemSlips/5;{fisNo}");
                return Content(await resp.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DepoTransfer GetFull hatasi");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult New() => View();

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JsonElement rawJson)
        {
            try
            {
                JObject data = JObject.Parse(rawJson.GetRawText());

                data["FaturaTip"] = 5;
                data["FatUst"] ??= new JObject();
                var fatUst = (JObject)data["FatUst"]!;

                fatUst["Tip"]         = 5;
                fatUst["TIPI"]        = 0;
                fatUst["AMBHARTUR"]   = 0;
                fatUst["KDV_DAHILMI"] = true;

                if (fatUst["Tarih"] == null || string.IsNullOrWhiteSpace((string?)fatUst["Tarih"]))
                    fatUst["Tarih"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                if (fatUst["Sube_Kodu"] == null || string.IsNullOrWhiteSpace(fatUst["Sube_Kodu"]?.ToString()))
                    fatUst["Sube_Kodu"] = 0;

                data["Kalems"] ??= new JArray();
                var kalems = (JArray)data["Kalems"]!;
                if (!kalems.Any()) return BadRequest(new { error = "En az bir kalem girmelisiniz." });

                foreach (JObject k in kalems)
                {
                    if (fatUst["FATIRS_NO"] != null) k["STra_FATIRSNO"] = fatUst["FATIRS_NO"];
                    k["STra_GC"]  ??= "C";
                    k["STra_TAR"] ??= fatUst["Tarih"];
                    k["STra_GCMIK"] ??= 0;
                    k["STra_BF"]    ??= 0;
                    if (k["DEPO_KODU"] != null && int.TryParse(k["DEPO_KODU"]?.ToString(), out int d)) k["DEPO_KODU"] = d;
                    if (k["Gir_Depo_Kodu"] != null && int.TryParse(k["Gir_Depo_Kodu"]?.ToString(), out int gd)) k["Gir_Depo_Kodu"] = gd;
                }

                var http = await CreateApiClientAsync();
                var content = new StringContent(data.ToString(), Encoding.UTF8, "application/json");
                var response = await http.PostAsync($"{ApiBaseUrl}/ItemSlips", content);
                return Content(await response.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DepoTransfer Create hatasi");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] JsonElement rawJson)
        {
            try
            {
                JObject fullJson = JObject.Parse(rawJson.GetRawText());
                string? fisNo = fullJson["FatUst"]?["FATIRS_NO"]?.ToString();
                if (string.IsNullOrWhiteSpace(fisNo))
                    return BadRequest(new { error = "Fiş numarası bulunamadı" });

                var http = await CreateApiClientAsync();

                // Eski fişi sil
                var delResp = await http.DeleteAsync($"{ApiBaseUrl}/ItemSlips/5;{fisNo}");
                var delJson = await delResp.Content.ReadAsStringAsync();
                dynamic delResult = JsonConvert.DeserializeObject(delJson)!;
                if (delResult?.IsSuccessful != true)
                    return Content(delJson, "application/json");

                // Zorunlu alanları ekle, yeniden oluştur
                fullJson["FaturaTip"] = 5;
                fullJson["FatUst"]    ??= new JObject();
                var fatUst = (JObject)fullJson["FatUst"]!;
                fatUst["Tip"] = 5; fatUst["TIPI"] = 0; fatUst["AMBHARTUR"] = 0; fatUst["KDV_DAHILMI"] = true;
                fullJson["Kalems"] ??= new JArray();

                var content = new StringContent(fullJson.ToString(), Encoding.UTF8, "application/json");
                var createResp = await http.PostAsync($"{ApiBaseUrl}/ItemSlips", content);
                return Content(await createResp.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DepoTransfer Save hatasi");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string fisNo)
        {
            if (string.IsNullOrWhiteSpace(fisNo))
                return BadRequest(new { error = "Fiş numarası boş olamaz." });
            try
            {
                var http = await CreateApiClientAsync();
                var response = await http.DeleteAsync($"{ApiBaseUrl}/ItemSlips/5;{fisNo}");
                return Content(await response.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DepoTransfer Delete hatasi");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
