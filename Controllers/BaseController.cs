using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text.Json;

namespace TESTPROJESI.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly IMemoryCache _cache;
        protected readonly IConfiguration _config;
        protected readonly ILogger _logger;

        protected BaseController(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            IConfiguration config,
            ILogger logger)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _config = config;
            _logger = logger;
        }

        protected string ApiBaseUrl => _config["NetOpenX:BaseUrl"]!.TrimEnd('/');

        protected async Task<string> GetTokenAsync()
        {
            if (_cache.TryGetValue("Token", out string? token) && token != null)
                return token;

            var http = _httpClientFactory.CreateClient();
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("branchcode", _config["NetOpenX:BranchCode"]!),
                new KeyValuePair<string, string>("username",   _config["NetOpenX:Username"]!),
                new KeyValuePair<string, string>("password",   _config["NetOpenX:Password"]!),
                new KeyValuePair<string, string>("dbname",     _config["NetOpenX:DbName"]!),
                new KeyValuePair<string, string>("dbuser",     _config["NetOpenX:DbUser"] ?? ""),
                new KeyValuePair<string, string>("dbpassword", _config["NetOpenX:DbPassword"] ?? ""),
                new KeyValuePair<string, string>("dbtype",     _config["NetOpenX:DbType"]!)
            });

            var resp = await http.PostAsync($"{ApiBaseUrl}/token", form);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();
            dynamic obj = JsonConvert.DeserializeObject(json)!;
            token = (string)obj.access_token;

            _cache.Set("Token", token, TimeSpan.FromMinutes(20));
            _logger.LogInformation("Token alindi ve cache'lendi");
            return token;
        }

        // Token ile hazir HttpClient doner
        protected async Task<HttpClient> CreateApiClientAsync()
        {
            var token = await GetTokenAsync();
            var http = _httpClientFactory.CreateClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return http;
        }

        // System.Text.Json JsonElement yardimci metodlari
        protected static string GetString(JsonElement item, string propName)
        {
            if (item.TryGetProperty(propName, out var val))
            {
                return val.ValueKind switch
                {
                    JsonValueKind.String => val.GetString() ?? "",
                    JsonValueKind.Number => val.GetRawText(),
                    JsonValueKind.True   => "true",
                    JsonValueKind.False  => "false",
                    _                   => ""
                };
            }
            return "";
        }

        protected static decimal GetDecimal(JsonElement item, string propName)
        {
            if (item.TryGetProperty(propName, out var val))
            {
                if (val.ValueKind == JsonValueKind.Number && val.TryGetDecimal(out var num))
                    return num;
                if (val.ValueKind == JsonValueKind.String &&
                    decimal.TryParse(val.GetString(), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var strNum))
                    return strNum;
            }
            return 0;
        }

        protected static bool GetBool(JsonElement item, string propName)
        {
            if (item.TryGetProperty(propName, out var val))
            {
                if (val.ValueKind == JsonValueKind.True) return true;
                if (val.ValueKind == JsonValueKind.False) return false;
                if (val.ValueKind == JsonValueKind.String && bool.TryParse(val.GetString(), out var b))
                    return b;
            }
            return false;
        }

        protected static string FormatDateTime(string input)
        {
            if (DateTime.TryParse(input, out var dt))
                return dt.ToString("yyyy-MM-dd HH:mm:ss");
            return input;
        }
    }
}
