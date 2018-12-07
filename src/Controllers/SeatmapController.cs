using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PingTimeout.Web.Data;
using PingTimeout.Web.Models;

namespace PingTimeout.Web.Controllers
{
    public class SeatmapController : Controller
    {
        private ApplicationDbContext _context;
 
        public SeatmapController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var model = new SeatmapIndexViewModel();
            var evt = _context.Events.FirstOrDefault();

            if (evt == null)
                return NotFound();

            var seats = _context.Seats.Where(s => s.Event.Id == evt.Id);
            model.Event = evt;
            model.Seats = seats;

            return View(model);
        }
    }
}
