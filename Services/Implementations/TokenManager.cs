using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TESTPROJESI.Models;
using TESTPROJESI.Services.Interfaces;

namespace TESTPROJESI.Services.Implementations
{
    /// <summary>
    /// 🔐 TokenManager
    /// Token’ı cache’te saklar, süresi dolunca otomatik yeniler.
    /// </summary>
    public class TokenManager : ITokenManager
    {
        private readonly ILogger<TokenManager> _logger;
        private readonly IMemoryCache _cache;
        private readonly INetOpenXService _netOpenXService;
        private readonly IConfiguration _configuration;

        private const string CacheKey = "NetOpenXAccessToken";
        private const string ExpireKey = "NetOpenXExpireTime";

        public TokenManager(
            IMemoryCache cache,
            INetOpenXService netOpenXService,
            IConfiguration configuration,
            ILogger<TokenManager> logger)
        {
            _cache = cache;
            _netOpenXService = netOpenXService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetTokenAsync()
        {
            // ✅ Önce cache’te varsa dön
            if (_cache.TryGetValue(CacheKey, out string existingToken) &&
                _cache.TryGetValue(ExpireKey, out DateTime expireTime) &&
                expireTime > DateTime.Now.AddMinutes(1))
            {
                _logger.LogInformation("🔁 Cache'ten geçerli token döndü (expire: {Expire})", expireTime);
                return existingToken;
            }

            // ❌ Token yok veya süresi bitmiş
            _logger.LogInformation("🔐 Yeni token alınıyor...");
            var token = await RequestNewTokenAsync();

            var expiresIn = token.expires_in > 0 ? token.expires_in : 1200; // fallback 20 dk
            var expireAt = DateTime.Now.AddSeconds(expiresIn - 30); // 30 sn önceden yenile

            _cache.Set(CacheKey, token.access_token, TimeSpan.FromSeconds(expiresIn - 10));
            _cache.Set(ExpireKey, expireAt, TimeSpan.FromSeconds(expiresIn - 10));

            _logger.LogInformation("✅ Yeni token cache’e kaydedildi (geçerlilik: {Minute} dk)", expiresIn / 60);
            return token.access_token;
        }

        private async Task<TokenResponse> RequestNewTokenAsync()
        {
            var settings = _configuration.GetSection("NetOpenX");

            var loginRequest = new LoginRequest
            {
                grant_type = "password",
                branchcode = settings["BranchCode"],
                password = settings["Password"],
                username = settings["Username"],
                dbname = settings["DbName"],
                dbuser = settings["DbUser"],
                dbpassword = settings["DbPassword"],
                dbtype = settings["DbType"]
            };

            var tokenResponse = await _netOpenXService.GetTokenAsync(loginRequest);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.access_token))
                throw new Exception("Token alınamadı!");

            return tokenResponse;
        }

        public async Task<string> RefreshTokenAsync()
        {
            _cache.Remove(CacheKey);
            _cache.Remove(ExpireKey);
            _logger.LogInformation("♻️ Token manuel olarak yenileniyor...");
            return await GetTokenAsync();
        }
    }
}
