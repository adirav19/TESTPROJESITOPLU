using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using TESTPROJESI.Business.DTOs;

namespace TESTPROJESI.Services.Interfaces
{
    public interface IFinishedGoodsService
    {
        Task<List<FinishedGoodsCreateDto>> GetAllAsync(string queryParams = null);

        // 🔹 DTO tipi olarak değiştirildi
        Task<FinishedGoodsDetailDto?> GetByIdAsync(string fisNo);

        Task<JsonElement> CreateAsync(FinishedGoodsCreateDto dto);
        Task<JsonElement> UpdateQuantityAsync(KalemDto dto);
    }
}
