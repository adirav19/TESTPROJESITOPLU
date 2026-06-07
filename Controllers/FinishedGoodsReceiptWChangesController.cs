using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;

namespace TESTPROJESI.Controllers
{
    public class FinishedGoodsReceiptWChangesController : BaseController
    {
        public FinishedGoodsReceiptWChangesController(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IConfiguration config,
            ILogger<FinishedGoodsReceiptWChangesController> logger)
            : base(httpClientFactory, cache, config, logger) { }

        public async Task<IActionResult> List(int page = 1, int pageSize = 20)
        {
            var http = await CreateApiClientAsync();
            int offset = (page - 1) * pageSize;
            var response = await http.GetAsync($"{ApiBaseUrl}/FinishedGoodsReceiptWChanges?limit={pageSize}&offset={offset}");
            var json = await response.Content.ReadAsStringAsync();
            dynamic root = JsonConvert.DeserializeObject(json)!;

            ViewBag.TotalCount = (int)root.TotalCount;
            ViewBag.Page       = page;
            ViewBag.PageSize   = pageSize;

            var list = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(root.Data));
            return View(list);
        }

        public async Task<IActionResult> Index(string fisNo)
        {
            if (string.IsNullOrWhiteSpace(fisNo))
                return View(null);
            try
            {
                var http = await CreateApiClientAsync();
                var response = await http.GetAsync($"{ApiBaseUrl}/FinishedGoodsReceiptWChanges/{fisNo}");
                var json = await response.Content.ReadAsStringAsync();
                dynamic root = JsonConvert.DeserializeObject(json)!;

                if (root.IsSuccessful != true) return View(null);
                return View(root.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "USK okunamadi");
                return View(null);
            }
        }

        public async Task<IActionResult> CreateFromProductionOrder(string isEmriNo)
        {
            if (string.IsNullOrWhiteSpace(isEmriNo))
                return View(null);
            try
            {
                var http = await CreateApiClientAsync();
                var response = await http.GetAsync($"{ApiBaseUrl}/FinishedGoodsReceiptWChanges/GetFromProductionOrder/{isEmriNo}");
                var json = await response.Content.ReadAsStringAsync();
                dynamic root = JsonConvert.DeserializeObject(json)!;

                if (root.IsSuccessful != true) return View(null);
                return View(root.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Is emrinden USK verisi alinamadi");
                return View(null);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] JsonElement rawJson)
        {
            try
            {
                JObject fullJson = JObject.Parse(rawJson.GetRawText());

                // Netsis boş array gerektiriyorsa tamamla
                fullJson["Seri"]               ??= new JArray();
                fullJson["ShrinkageDetailList"] ??= new JArray();
                fullJson["MamulFireSeri"]       ??= new JArray();
                fullJson["Kalem"]               ??= new JArray();

                foreach (var k in (JArray)fullJson["Kalem"]!)
                {
                    k["ShrinkageDetailList"] ??= new JArray();
                    k["KalemSeri"]           ??= new JArray();
                }

                var http = await CreateApiClientAsync();
                var content = new StringContent(fullJson.ToString(), Encoding.UTF8, "application/json");
                var response = await http.PostAsync($"{ApiBaseUrl}/FinishedGoodsReceiptWChanges/Save", content);
                return Content(await response.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "USK Save hatasi");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Produce([FromBody] JsonElement rawJson)
        {
            try
            {
                JObject data = JObject.Parse(rawJson.GetRawText());
                var http = await CreateApiClientAsync();
                var content = new StringContent(data.ToString(), Encoding.UTF8, "application/json");
                var response = await http.PostAsync($"{ApiBaseUrl}/FinishedGoodsReceiptWChanges/ReceiptProduce", content);
                return Content(await response.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "USK Produce hatasi");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetKalemler(string fisNo)
        {
            var http = await CreateApiClientAsync();
            var response = await http.GetAsync($"{ApiBaseUrl}/FinishedGoodsReceiptWChanges/{fisNo}");
            var json = await response.Content.ReadAsStringAsync();
            dynamic root = JsonConvert.DeserializeObject(json)!;

            if (root.IsSuccessful != true)
                return BadRequest("Fiş bulunamadı");

            return Json(root.Data.Kalem);
        }

        [HttpGet("FinishedGoodsReceiptWChanges/GetFull/{fisNo}")]
        public async Task<IActionResult> GetFull(string fisNo)
        {
            var http = await CreateApiClientAsync();
            var resp = await http.GetAsync($"{ApiBaseUrl}/FinishedGoodsReceiptWChanges/{fisNo}");
            return Content(await resp.Content.ReadAsStringAsync(), "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> GetFromProductionOrderJson(string isEmriNo)
        {
            if (string.IsNullOrWhiteSpace(isEmriNo))
                return BadRequest(new { success = false, message = "İş emri no boş olamaz" });

            try
            {
                var http = await CreateApiClientAsync();
                var response = await http.GetAsync($"{ApiBaseUrl}/FinishedGoodsReceiptWChanges/GetFromProductionOrder/{isEmriNo}");
                var json = await response.Content.ReadAsStringAsync();

                var root = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (root != null && root.ContainsKey("IsSuccessful") && root["IsSuccessful"].ToString() == "True")
                    return Content(json, "application/json");

                return Json(new { success = false, message = "İş emrinden veri alınamadı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetFromProductionOrderJson ERROR");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductionOrders()
        {
            try
            {
                var http = await CreateApiClientAsync();
                var response = await http.GetAsync($"{ApiBaseUrl}/ProductionOrder?limit=200&sort=IsEmriNo ASC");
                var json = await response.Content.ReadAsStringAsync();

                dynamic root = JsonConvert.DeserializeObject(json)!;
                var items = root.Data ?? root.data;
                if (items == null) return Json(new List<dynamic>());

                var columnarList = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(items));
                if (columnarList == null || columnarList.Count == 0) return Json(new List<dynamic>());

                var firstRow = columnarList[0] as JObject;
                if (firstRow == null) return Json(new List<dynamic>());

                var properties = firstRow.Properties().ToList();
                int rowCount = 0;
                var firstProp = properties.FirstOrDefault();
                if (firstProp?.Value is JArray firstArray) rowCount = firstArray.Count;
                if (rowCount == 0) return Json(new List<dynamic>());

                // Columnar → row format
                var rowList = new List<dynamic>();
                for (int i = 0; i < rowCount; i++)
                {
                    var row = new Dictionary<string, object>();
                    foreach (var col in columnarList)
                    {
                        var jObj = col as JObject;
                        if (jObj == null) continue;
                        foreach (var prop in jObj.Properties())
                            if (prop.Value is JArray arr && arr.Count > i)
                                row[prop.Name] = arr[i];
                    }
                    rowList.Add(row);
                }

                return Json(rowList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Is emirleri listesi alinamadi");
                return Json(new List<dynamic>());
            }
        }
    }
}
