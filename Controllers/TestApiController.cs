using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;
using TESTPROJESI.Business;
using TESTPROJESI.Business.DTOs;
using TESTPROJESI.Services.Interfaces;

namespace TESTPROJESI.Controllers
{
    public class TestApiController : Controller
    {
        private readonly ICarilerService _carilerService;
        private readonly ILogger<TestApiController> _logger;

        public TestApiController(ICarilerService carilerService, ILogger<TestApiController> logger)
        {
            _carilerService = carilerService;
            _logger = logger;
        }

        // 🔹 Ana sayfa (liste görüntüleme)
        public async Task<IActionResult> Index()
        {
            try
            {
                var result = await _carilerService.GetCarilerAsync();
                ViewBag.Result = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Hata oluştu: {Message}", ex.Message);
                ViewBag.Hata = ex.Message;
            }
            return View();
        }

        // 🔹 Cari oluşturma (AJAX)
        [HttpPost]
        public async Task<IActionResult> CreateCari([FromBody] CariCreateDto dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Cari bilgisi alınamadı." });

            try
            {
                var result = await _carilerService.CreateCariAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = "✅ Cari başarıyla oluşturuldu.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Cari oluşturma hatası: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Cari oluşturulurken hata: {ex.Message}"
                });
            }
        }

        // 🔹 Cari güncelleme (AJAX)
        [HttpPost]
        public async Task<IActionResult> UpdateCari([FromBody] CariUpdateDto dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Cari bilgisi alınamadı." });

            try
            {
                var result = await _carilerService.UpdateCariAsync(dto);
                return Ok(new
                {
                    success = true,
                    message = $"✏️ {dto.CARI_KOD} kodlu cari güncellendi.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Cari güncelleme hatası: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Cari güncellenirken hata: {ex.Message}"
                });
            }
        }

        // 🔹 Cari silme (AJAX)
        [HttpPost]
        public async Task<IActionResult> DeleteCari([FromBody] CariSilDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.cariKodu))
                return BadRequest(new { success = false, message = "Cari kodu boş olamaz." });

            try
            {
                await _carilerService.DeleteCariAsync(dto.cariKodu);
                return Ok(new
                {
                    success = true,
                    message = $"🗑️ {dto.cariKodu} kodlu cari başarıyla silindi."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Cari silme hatası: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Cari silinirken hata: {ex.Message}"
                });
            }
        }

        // 🔹 Cari listesi (tabloyu doldurmak için AJAX)
        [HttpGet]
        public async Task<IActionResult> ListCariler()
        {
            try
            {
                var result = await _carilerService.GetCarilerAsync();

                // Eğer API yanıtı obje içeriyorsa (örneğin Data, items vb.)
                if (result.ValueKind == JsonValueKind.Object)
                {
                    if (result.TryGetProperty("Data", out var dataArray))
                        return Json(dataArray);

                    if (result.TryGetProperty("items", out var itemsArray))
                        return Json(itemsArray);

                    if (result.TryGetProperty("value", out var valueArray))
                        return Json(valueArray);
                }

                // Zaten dizi formatındaysa direkt döndür
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Cari listesi alınırken hata: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
