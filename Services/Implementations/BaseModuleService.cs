using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TESTPROJESI.Services.Interfaces;

namespace TESTPROJESI.Services.Implementations
{
    /// <summary>
    /// 🌐 BaseModuleService:
    /// Tüm modül servislerinin ortak temeli.
    /// Token, hata yönetimi ve loglama burada kapsüllenmiştir.
    /// </summary>
    public abstract class BaseModuleService<T> where T : class
    {
        protected readonly IBaseApiService _apiService;
        protected readonly ITokenManager _tokenManager;
        protected readonly ILogger<T> _logger;

        protected BaseModuleService(
            IBaseApiService apiService,
            ITokenManager tokenManager,
            ILogger<T> logger)
        {
            _apiService = apiService;
            _tokenManager = tokenManager;
            _logger = logger;
        }

        // ✅ Ortak güvenli GET
        protected async Task<TResult?> SafeGetAsync<TResult>(string endpoint)
        {
            try
            {
                var token = await _tokenManager.GetTokenAsync();
                return await _apiService.GetAsync<TResult>(endpoint, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ GET isteği başarısız: {Endpoint}", endpoint);
                throw;
            }
        }

        // ✅ Ortak güvenli POST
        protected async Task<TResult?> SafePostAsync<TResult>(string endpoint, object data)
        {
            try
            {
                var token = await _tokenManager.GetTokenAsync();
                return await _apiService.PostAsync<TResult>(endpoint, data, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ POST isteği başarısız: {Endpoint}", endpoint);
                throw;
            }
        }

        // ✅ Ortak güvenli PUT
        protected async Task<TResult?> SafePutAsync<TResult>(string endpoint, object data)
        {
            try
            {
                var token = await _tokenManager.GetTokenAsync();
                return await _apiService.PutAsync<TResult>(endpoint, data, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ PUT isteği başarısız: {Endpoint}", endpoint);
                throw;
            }
        }

        // ✅ Ortak güvenli DELETE
        protected async Task<bool> SafeDeleteAsync(string endpoint)
        {
            try
            { 
                var token = await _tokenManager.GetTokenAsync();
                return await _apiService.DeleteAsync(endpoint, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ DELETE isteği başarısız: {Endpoint}", endpoint);
                throw;
            }
        }
    }
}
