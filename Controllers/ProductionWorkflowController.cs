using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using TESTPROJESI.Models;

namespace TESTPROJESI.Controllers
{
    public class ProductionWorkflowController : BaseController
    {
        public ProductionWorkflowController(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IConfiguration config,
            ILogger<ProductionWorkflowController> logger)
            : base(httpClientFactory, cache, config, logger) { }

        public IActionResult Index() => View();

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
    }
}
