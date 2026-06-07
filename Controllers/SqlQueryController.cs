using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using TESTPROJESI.Models;

namespace TESTPROJESI.Controllers
{
    public class SqlQueryController : BaseController
    {
        // Sadece harf, rakam ve alt çizgiye izin ver (SQL injection koruması)
        private static readonly Regex SafeTableName = new(@"^[a-zA-Z0-9_]+$", RegexOptions.Compiled);

        public SqlQueryController(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IConfiguration config,
            ILogger<SqlQueryController> logger)
            : base(httpClientFactory, cache, config, logger) { }

        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> Execute([FromBody] SqlQueryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.TSql))
                return BadRequest(new { success = false, message = "SQL sorgusu boş olamaz" });

            try
            {
                var http = await CreateApiClientAsync();
                var body = new { TSql = request.TSql.Trim() };
                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

                var response = await http.PostAsync($"{ApiBaseUrl}/Queries", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, new { success = false, message = responseJson });

                var data = JsonSerializer.Deserialize<JsonElement>(responseJson);
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQL sorgusu calistirulamadi");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTable(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { success = false, message = "Tablo adı gerekli" });

            if (!SafeTableName.IsMatch(tableName))
                return BadRequest(new { success = false, message = "Geçersiz tablo adı." });

            try
            {
                var http = await CreateApiClientAsync();
                var body = new { TSql = $"SELECT TOP 100 * FROM {tableName}" };
                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

                var response = await http.PostAsync($"{ApiBaseUrl}/Queries", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, new { success = false, message = responseJson });

                var data = JsonSerializer.Deserialize<JsonElement>(responseJson);
                return Ok(new { success = true, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tablo verisi alinamadi");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
