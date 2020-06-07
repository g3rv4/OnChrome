using Microsoft.AspNetCore.Mvc;
using OnChrome.Core.Helpers;
using OnChrome.Web.Models;

namespace OnChrome.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var model = new IndexViewModel
            {
                NativeMessagingState = OsDependentTasks.GetNativeMessagingState(),
            };
            return View(model);
        }

        public IActionResult Unregister()
        {
            OsDependentTasks.UnregisterNativeMessaging();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Register()
        {
            OsDependentTasks.SetupNativeMessaging();
            return RedirectToAction(nameof(Index));
        }
    }
}
