using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using TESTPROJESI.Models;

namespace TESTPROJESI.Controllers
{
    public class ProductionFlowController : BaseController
    {
        public ProductionFlowController(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IConfiguration config,
            ILogger<ProductionFlowController> logger)
            : base(httpClientFactory, cache, config, logger) { }

        public async Task<IActionResult> Index(string isEmriNo)
        {
            if (string.IsNullOrWhiteSpace(isEmriNo))
                return View(null);

            try
            {
                var http = await CreateApiClientAsync();
                var resp = await http.GetAsync($"{ApiBaseUrl}/ProductionFlow?q=ISEMRINO='{isEmriNo}'");
                var json = await resp.Content.ReadAsStringAsync();

                dynamic root = JsonConvert.DeserializeObject(json)!;
                if (root == null || root.IsSuccessful != true)
                    return View(null);

                var list = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(root.Data));
                return View(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProductionFlow Index hatasi");
                return View(null);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUAK(string isEmriNo, string conf)
        {
            try
            {
                var http = await CreateApiClientAsync();
                var resp = await http.GetAsync($"{ApiBaseUrl}/ProductionFlow/{isEmriNo};{conf}");
                return Content(await resp.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] JObject fullJson)
        {
            try
            {
                fullJson["ShrinkageDetailList"] ??= new JArray();
                fullJson["UAKKaynakLists"]      ??= new JArray();

                var http = await CreateApiClientAsync();
                var content = new StringContent(fullJson.ToString(), Encoding.UTF8, "application/json");
                var resp = await http.PostAsync($"{ApiBaseUrl}/ProductionFlow", content);
                return Content(await resp.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string isEmriNo, string conf)
        {
            try
            {
                var http = await CreateApiClientAsync();
                var resp = await http.DeleteAsync($"{ApiBaseUrl}/ProductionFlow/{isEmriNo};{conf}");
                return Content(await resp.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // Typed UAK kayıt endpoint — mobil app bu endpoint'i kullanır
        [HttpPost]
        public async Task<IActionResult> SaveUAK([FromBody] UakKayitDto dto)
        {
            try
            {
                var http = await CreateApiClientAsync();
                var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
                var resp = await http.PostAsync($"{ApiBaseUrl}/ProductionFlow", content);
                return Content(await resp.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UAK kaydedilemedi");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConvertToUSK(string isEmriNo)
        {
            try
            {
                var http = await CreateApiClientAsync();
                var payload = new JObject
                {
                    ["IsEmriNoAralikAlt"]                    = isEmriNo,
                    ["IsEmriNoAralikUst"]                    = isEmriNo,
                    ["TarihAralikAlt"]                       = "2000-01-01",
                    ["TarihAralikUst"]                       = DateTime.Now.ToString("yyyy-MM-dd"),
                    ["FislerIsEmriBazindaTekTekOlusturulsun"] = true,
                    ["FisNoSerisi"]                          = "U",
                    ["KayitTarihi"]                          = DateTime.Now.ToString("yyyy-MM-dd"),
                    ["GirisDepo"]                            = 1,
                    ["CikisDepo"]                            = 1,
                    ["FislerOtomatikUretilsin"]              = true
                };

                var content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");
                var resp = await http.PostAsync($"{ApiBaseUrl}/ProductionFlow/ProductionFlowToFinishedGoodsReceipt", content);
                return Content(await resp.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
