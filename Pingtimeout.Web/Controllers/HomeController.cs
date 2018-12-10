using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PingTimeout.Web.Data;
using PingTimeout.Web.Models;

namespace PingTimeout.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOptions<PingTimeoutConfig> config;
        private ApplicationDbContext context;
 
        public HomeController(ApplicationDbContext context, IOptions<PingTimeoutConfig> config)
        {
            this.context = context;
            this.config = config;
        }
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Seatmap");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
