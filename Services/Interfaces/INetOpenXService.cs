using System.Threading.Tasks;
using TESTPROJESI.Models;

namespace TESTPROJESI.Services.Interfaces
{
    public interface INetOpenXService
    {
        Task<TokenResponse?> GetTokenAsync(LoginRequest request);

        Task<bool> DeleteCariAsync(string cariKodu);


    }
}
