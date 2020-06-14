using Microsoft.AspNetCore.Mvc;
using OnChrome.Core.Helpers;
using OnChrome.Web.Models;

namespace OnChrome.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index(string? extensionVersion, bool? missingExtension)
        {
            if (extensionVersion == null && !missingExtension.HasValue)
            {
                return View("GetExtensionVersion");
            }
            
            var model = new IndexViewModel
            {
                NativeMessagingState = OsDependentTasks.GetNativeMessagingState(),
                ExtensionVersion = extensionVersion,
                CompatibilityStatus = ExtensionVersionHelper.GetCompatibiltyStatus(extensionVersion, out var appVersion),
                AppVersion = appVersion,
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

        [Route("shutdown")]
        public IActionResult Shutdown()
        {
            Program.CancellationTokenSource.Cancel();
            return Content("ok");
        }
    }
}
