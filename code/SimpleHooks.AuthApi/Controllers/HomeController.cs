using Microsoft.AspNetCore.Mvc;

namespace SimpleTools.SimpleHooks.AuthApi.Controllers
{
    public class HomeController(ILogger<HomeController> logger) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;

        public IActionResult Index()
        {
            return Ok("simple-hooks auth api running normally");
        }
    }
}
