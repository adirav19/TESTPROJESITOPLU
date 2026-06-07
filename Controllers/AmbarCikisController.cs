using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;

namespace TESTPROJESI.Controllers
{
    public class AmbarCikisController : BaseController
    {
        public AmbarCikisController(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IConfiguration config,
            ILogger<AmbarCikisController> logger)
            : base(httpClientFactory, cache, config, logger) { }

        public async Task<IActionResult> List(int page = 1, int pageSize = 20)
        {
            try
            {
                int offset = (page - 1) * pageSize;
                var http = await CreateApiClientAsync();
                var response = await http.GetAsync($"{ApiBaseUrl}/ItemSlips?limit={pageSize}&offset={offset}&docType=9");
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
                _logger.LogError(ex, "AmbarCikis LIST hatasi");
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
                var resp = await http.GetAsync($"{ApiBaseUrl}/ItemSlips/9;{fisNo}");
                return Content(await resp.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AmbarCikis GetFull hatasi");
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

                data["FaturaTip"]          = 9;
                data["Use64BitService"]    = true;
                data["EIrsEkBilgi"]        = null;
                data["HalFaturaMasraflari"] = null;

                data["FatUst"] ??= new JObject();
                var fatUst = (JObject)data["FatUst"]!;

                fatUst["Tip"]         = 9;
                fatUst["TIPI"]        = 0;
                fatUst["AMBHARTUR"]   = 2;
                fatUst["CikisYeri"]   = 4;
                fatUst["KDV_DAHILMI"] = true;

                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                if (fatUst["Tarih"] == null || string.IsNullOrWhiteSpace((string?)fatUst["Tarih"]))
                    fatUst["Tarih"] = now;

                fatUst["FiiliTarih"]  = fatUst["Tarih"];
                fatUst["ODEMETARIHI"] ??= now;
                fatUst["Sube_Kodu"]   ??= 0;

                data["Kalems"] ??= new JArray();
                var kalems = (JArray)data["Kalems"]!;
                if (!kalems.Any()) return BadRequest(new { error = "En az bir kalem gerekli." });

                foreach (JObject k in kalems)
                {
                    k["DovizAdi"]           = "";
                    k["STra_DovizAdi"]      = "";
                    k["KalemSeri"]          = new JArray();
                    k["Asorti"]             = new JArray();
                    k["KosulMalFazlasiIsle"] = false;
                    k["STra_GC"]            = "C";
                    k["STra_TAR"]           = fatUst["Tarih"];
                    if (fatUst["FATIRS_NO"] != null) k["STra_FATIRSNO"] = fatUst["FATIRS_NO"];
                    if (k["DEPO_KODU"] != null && int.TryParse(k["DEPO_KODU"]!.ToString(), out int depo))
                        k["DEPO_KODU"] = depo;
                }

                var http = await CreateApiClientAsync();
                var content = new StringContent(data.ToString(), Encoding.UTF8, "application/json");
                var response = await http.PostAsync($"{ApiBaseUrl}/ItemSlips", content);
                return Content(await response.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AmbarCikis Create hatasi");
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
                    return BadRequest(new { error = "Fiş numarası yok." });

                var http = await CreateApiClientAsync();

                // Eski fişi sil
                var delResp = await http.DeleteAsync($"{ApiBaseUrl}/ItemSlips/9;{fisNo}");
                var delJson = await delResp.Content.ReadAsStringAsync();
                dynamic delObj = JsonConvert.DeserializeObject(delJson)!;
                if (delObj?.IsSuccessful != true)
                    return Content(delJson, "application/json");

                // Zorunlu alanları ekle, yeniden oluştur
                fullJson["FaturaTip"]          = 9;
                fullJson["Use64BitService"]    = true;
                fullJson["EIrsEkBilgi"]        = null;
                fullJson["HalFaturaMasraflari"] = null;

                var fatUst = (JObject)fullJson["FatUst"]!;
                fatUst["Tip"]         = 9;
                fatUst["TIPI"]        = 0;
                fatUst["KDV_DAHILMI"] = true;
                fatUst["AMBHARTUR"]   = 2;
                fatUst["CikisYeri"]   = 4;
                fatUst["FiiliTarih"]  = fatUst["Tarih"];

                fullJson["Kalems"] ??= new JArray();
                foreach (JObject k in (JArray)fullJson["Kalems"]!)
                {
                    k["DovizAdi"] = ""; k["STra_DovizAdi"] = "";
                    k["KalemSeri"] = new JArray(); k["Asorti"] = new JArray();
                    k["KosulMalFazlasiIsle"] = false;
                    k["STra_GC"] = "C"; k["STra_TAR"] = fatUst["Tarih"];
                }

                var content = new StringContent(fullJson.ToString(), Encoding.UTF8, "application/json");
                var createResp = await http.PostAsync($"{ApiBaseUrl}/ItemSlips", content);
                return Content(await createResp.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AmbarCikis Save hatasi");
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
                var response = await http.DeleteAsync($"{ApiBaseUrl}/ItemSlips/9;{fisNo}");
                return Content(await response.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AmbarCikis Delete hatasi");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
