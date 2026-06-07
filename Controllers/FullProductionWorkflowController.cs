using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Text;
using TESTPROJESI.Models;

namespace TESTPROJESI.Controllers
{
    public class FullProductionWorkflowController : BaseController
    {
        public FullProductionWorkflowController(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IConfiguration config,
            ILogger<FullProductionWorkflowController> logger)
            : base(httpClientFactory, cache, config, logger) { }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetCustomerOrders(int page = 1, int pageSize = 50)
        {
            try
            {
                int offset = (page - 1) * pageSize;
                var http = await CreateApiClientAsync();
                var resp = await http.GetAsync($"{ApiBaseUrl}/ItemSlips?limit={pageSize}&offset={offset}&docType=7");
                var json = await resp.Content.ReadAsStringAsync();

                dynamic root = JsonConvert.DeserializeObject(json)!;
                if (root == null || root.IsSuccessful != true)
                    return Json(new { success = false, data = new List<dynamic>() });

                var list = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(root.Data));
                return Json(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCustomerOrders ERROR");
                return Json(new { success = false, data = new List<dynamic>(), error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerOrderDetail(string fisNo)
        {
            if (string.IsNullOrWhiteSpace(fisNo))
                return BadRequest(new { error = "Fiş numarası boş olamaz." });
            try
            {
                var http = await CreateApiClientAsync();
                var resp = await http.GetAsync($"{ApiBaseUrl}/ItemSlips/7;{fisNo}");
                return Content(await resp.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCustomerOrderDetail ERROR");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductionOrders(int page = 1, int pageSize = 200)
        {
            try
            {
                int offset = (page - 1) * pageSize;
                var http = await CreateApiClientAsync();
                var resp = await http.GetAsync($"{ApiBaseUrl}/ProductionOrder?limit={pageSize}&offset={offset}&sort=IsEmriNo ASC");
                var json = await resp.Content.ReadAsStringAsync();

                dynamic root = JsonConvert.DeserializeObject(json)!;
                var list = JsonConvert.DeserializeObject<List<ProductionOrderVm>>(JsonConvert.SerializeObject(root.Data));

                return Json(new { success = true, data = list });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetProductionOrders ERROR");
                return Json(new { success = false, data = new List<ProductionOrderVm>(), error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProductionOrder([FromBody] CreateProductionOrderDto dto)
        {
            try
            {
                var http = await CreateApiClientAsync();
                var payload = new
                {
                    TransactSupport     = true,
                    MuhasebelesmisBelge = false,
                    dto.IsEmriNo,
                    dto.Tarih,
                    dto.StokKodu,
                    dto.Miktar,
                    dto.TeslimTarihi,
                    SiparisNo           = dto.SiparisNo ?? "",
                    SipKont             = string.IsNullOrWhiteSpace(dto.SiparisNo) ? 0 : 1,
                    dto.DepoKodu,
                    dto.CikisDepoKodu,
                    Aciklama            = dto.Aciklama ?? "",
                    Kapali              = false,
                    Olcubr              = 1,
                    SubeKodu            = 0
                };

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                var resp = await http.PostAsync($"{ApiBaseUrl}/ProductionOrder", content);
                return Content(await resp.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateProductionOrder ERROR");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult CreateProductionReceiptForm(string isEmriNo)
        {
            if (string.IsNullOrWhiteSpace(isEmriNo))
                return BadRequest("İş emri no boş olamaz");

            return RedirectToAction("CreateFromProductionOrder", "FinishedGoodsReceiptWChanges", new { isEmriNo });
        }

        [HttpGet]
        public async Task<IActionResult> GetFinishedGoods()
        {
            try
            {
                var http = await CreateApiClientAsync();
                var resp = await http.GetAsync($"{ApiBaseUrl}/FinishedGoods?limit=100&offset=0");
                return Content(await resp.Content.ReadAsStringAsync(), "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetFinishedGoods ERROR");
                return Json(new { success = false, data = new List<dynamic>(), error = ex.Message });
            }
        }
    }
}
