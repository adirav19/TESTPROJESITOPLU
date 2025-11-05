using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TESTPROJESI.Business.DTOs;
using TESTPROJESI.Services.Interfaces;

namespace TESTPROJESI.Services.Implementations
{
    /// <summary>
    /// 🧩 Cariler modülü servisidir.
    /// BaseModuleService’den türetilir, böylece ortak HTTP işlemleri devralınır.
    /// </summary>
    public class CarilerService : BaseModuleService<CarilerService>, ICarilerService
    {
        public CarilerService(
            IBaseApiService apiService,
            ITokenManager tokenManager,
            ILogger<CarilerService> logger)
            : base(apiService, tokenManager, logger)
        {
        }

        /// <summary>
        /// 📋 Tüm carileri getirir.
        /// </summary>
        public async Task<JsonElement> GetCarilerAsync()
        {
            return await SafeGetAsync<JsonElement>("ARPs?limit=50&sort=CARI_KOD ASC");
        }

        /// <summary>
        /// ➕ Yeni cari oluşturur.
        /// </summary>
        public async Task<JsonElement> CreateCariAsync(CariCreateDto dto)
        {
            var body = new
            {
                CariTemelBilgi = new
                {
                    CARI_KOD = dto.CARI_KOD,
                    CARI_ISIM = dto.CARI_ISIM,
                    CARI_TEL = dto.CARI_TEL,
                    CARI_IL = dto.CARI_IL,
                    EMAIL = dto.EMAIL,
                    ISLETME_KODU = 1,
                    CARI_TIP = "A"
                },
                CariEkBilgi = new { CARI_KOD = dto.CARI_KOD },
                SubelerdeOrtak = true,
                IsletmelerdeOrtak = true,
                TransactSupport = true,
                MuhasebelesmisBelge = true
            };

            return await SafePostAsync<JsonElement>("ARPs", body);
        }

        public async Task<JsonElement> UpdateCariAsync(CariUpdateDto dto)
        {
            var body = new
            {
                CariTemelBilgi = new
                {
                    dto.CARI_KOD,
                    dto.CARI_ISIM,
                    dto.CARI_TEL,
                    dto.CARI_IL,
                    dto.EMAIL
                },
                CariEkBilgi = new { CARI_KOD = dto.CARI_KOD },
                SubelerdeOrtak = true,
                IsletmelerdeOrtak = true,
                TransactSupport = true,
                MuhasebelesmisBelge = true
            };

            return await SafePutAsync<JsonElement>($"ARPs/{dto.CARI_KOD}", body);
        }

        public async Task DeleteCariAsync(string cariKodu)
        {
            await SafeDeleteAsync($"ARPs/{cariKodu}");
        }
    }
}
