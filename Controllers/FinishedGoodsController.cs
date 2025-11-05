using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TESTPROJESI.Business.DTOs;
using TESTPROJESI.Services.Interfaces;

namespace TESTPROJESI.Controllers
{
    public class FinishedGoodsController : Controller
    {
        private readonly IFinishedGoodsService _service;

        public FinishedGoodsController(IFinishedGoodsService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Json(new { Data = result }); // 🔹 Data sarmalı eklendi
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FinishedGoodsCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return Json(result);
        }

        [HttpDelete("FinishedGoods/Delete/{fisNo}")]
        public async Task<IActionResult> Delete(string fisNo)
        {
            // Burada silme işlemi ileride API'den yapılacak
            return Ok(new { success = true, message = $"{fisNo} silindi." }); 
        }

        [HttpGet("FinishedGoods/Detail/{fisNo}")]
        public async Task<IActionResult> Detail(string fisNo)
        {
            var result = await _service.GetByIdAsync(fisNo);
            if (result == null)
                return NotFound(new { success = false, message = "Fiş bulunamadı." });

            return Json(new { success = true, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInline([FromBody] dynamic dto)
        {
            // Inline düzenleme ileride aktif edilecek
            return Ok(new { success = true, message = "Güncellendi." });
        }

        [HttpPost("FinishedGoods/UpdateQuantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] KalemDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.StokKodu))
                return BadRequest(new { success = false, message = "Geçersiz veri gönderildi." });

            var result = await _service.UpdateQuantityAsync(dto);
            return Json(result);
        }

    }
}
