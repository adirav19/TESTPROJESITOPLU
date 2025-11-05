using System.Threading.Tasks;

namespace TESTPROJESI.Services.Interfaces
{
    /// <summary>
    /// 🌐 Tüm API istekleri için temel servis arayüzü.
    /// GET, POST, PUT, DELETE işlemleri burada tanımlanır.
    /// </summary>
    public interface IBaseApiService
    {
        Task<T?> GetAsync<T>(string endpoint, string token = null);
        Task<T?> PostAsync<T>(string endpoint, object data, string token = null);
        Task<T?> PutAsync<T>(string endpoint, object data, string token = null);
        Task<bool> DeleteAsync(string endpoint, string token = null);
    }
}
