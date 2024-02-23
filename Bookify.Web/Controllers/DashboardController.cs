using Microsoft.AspNetCore.Mvc;

namespace Bookify.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
