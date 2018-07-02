using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HB.Framework.AuthorizationServer.Web.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;

namespace HB.Framework.AuthorizationServer.Web.Controllers
{
    public class HomeController : Controller
    {
        private ILogger _logger;

        public HomeController(ILogger<HomeController> logger, IDataProtectionProvider dataProtectionProvider)
        {
            _logger = logger;

            IDataProtector dataProtector = dataProtectionProvider.CreateProtector("HomeController");

            string secrets = dataProtector.Protect("xxxxxxxxxxxxxxxxxxxxx");

            _logger.LogInformation($"Start In HomeController:{secrets}");
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Display Index");
            _logger.LogDebug("Display index debug");
            
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
