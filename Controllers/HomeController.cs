using Microsoft.AspNetCore.Mvc;

namespace TESTPROJESI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
