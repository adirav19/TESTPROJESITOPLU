using Microsoft.AspNetCore.Mvc;

namespace MinimalProject.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
