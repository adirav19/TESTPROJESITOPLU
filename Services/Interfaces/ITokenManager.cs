using System.Threading.Tasks;

namespace TESTPROJESI.Services.Interfaces
{
    /// <summary>
    /// 🔐 Token yönetimi için arayüz.
    /// Token alma, yenileme ve cache işlemleri burada tanımlanır.
    /// </summary>
    public interface ITokenManager
    {
        Task<string> GetTokenAsync();
        Task<string> RefreshTokenAsync();
    }
}
 