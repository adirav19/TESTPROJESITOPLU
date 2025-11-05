using System.Text.Json;
using System.Threading.Tasks;
using TESTPROJESI.Business.DTOs;

namespace TESTPROJESI.Services.Interfaces
{
    public interface ICarilerService
    {
        Task<JsonElement> GetCarilerAsync();
        Task<JsonElement> CreateCariAsync(CariCreateDto dto);
        Task<JsonElement> UpdateCariAsync(CariUpdateDto dto);
        Task DeleteCariAsync(string cariKodu);
    }
}
