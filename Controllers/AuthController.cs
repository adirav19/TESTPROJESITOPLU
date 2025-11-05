using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TESTPROJESI.Services.Interfaces; // ✅ interface ekle

namespace TESTPROJESI.Controllers
{
    public class AuthController : Controller
    {
        private readonly ITokenManager _tokenManager; // ✅ interface tipi olmalı

        // ✅ Interface üzerinden dependency injection
        public AuthController(ITokenManager tokenManager)
        {
            _tokenManager = tokenManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string? actionType)
        {
            try
            {
                var token = await _tokenManager.GetTokenAsync();
                ViewBag.Token = token;
            }
            catch (Exception ex)
            {
                ViewBag.Hata = ex.Message;
            }

            return View();
        }
    }
}
