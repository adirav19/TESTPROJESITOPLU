using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Text;
using TESTPROJESI.Models;

namespace TESTPROJESI.Controllers
{
    public class ProductionOrderController : BaseController
    {
        public ProductionOrderController(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IConfiguration config,
            ILogger<ProductionOrderController> logger)
            : base(httpClientFactory, cache, config, logger) { }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            try
            {
                var http = await CreateApiClientAsync();
                var response = await http.GetAsync($"{ApiBaseUrl}/ProductionOrder?page={page}&pageSize={pageSize}");
                var json = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                dynamic result = JsonConvert.DeserializeObject(json)!;
                int totalCount = result.TotalCount ?? 0;

                var orders = new List<dynamic>();
                if (result.Data != null)
                    foreach (var item in result.Data) orders.Add(item);

                ViewBag.Page       = page;
                ViewBag.PageSize   = pageSize;
                ViewBag.TotalCount = totalCount;

                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Is emirleri listelenemedi");
                TempData["Error"] = "İş emirleri yüklenirken hata: " + ex.Message;
                return View(new List<dynamic>());
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(
            string IsEmriNo, DateTime Tarih, string StokKodu, decimal Miktar,
            int DepoKodu, int CikisDepoKodu, DateTime TeslimTarihi, string SiparisNo = "")
        {
            try
            {
                var http = await CreateApiClientAsync();
                var sipNo = string.IsNullOrWhiteSpace(Request.Form["SiparisNo"]) ? "" : Request.Form["SiparisNo"].ToString();

                var body = new
                {
                    TransactSupport         = true,
                    IsEmriNo,
                    Tarih,
                    StokKodu,
                    Miktar,
                    TeslimTarihi,
                    SiparisNo               = sipNo,
                    SipKont                 = string.IsNullOrWhiteSpace(sipNo) ? 0 : 1,
                    DepoKodu,
                    CikisDepoKodu,
                    Kapali                  = false,
                    Oncelik                 = 0,
                    UskDurumu               = 0,
                    RezervasyonDurumu       = 0
                };

                var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                var resp = await http.PostAsync($"{ApiBaseUrl}/ProductionOrder", content);
                var respJson = await resp.Content.ReadAsStringAsync();

                dynamic root = JsonConvert.DeserializeObject(respJson)!;
                if (root.IsSuccessful != true)
                    return Json(new { success = false, message = (string?)root.ErrorDesc });

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Is emri olusturulamadi");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
