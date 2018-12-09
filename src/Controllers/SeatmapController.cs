using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var activeEvent = _context.Events.FirstOrDefault();

            if (activeEvent == null)
                return NotFound();

            var seats = _context.Seats.Where(s => s.Event.Id == activeEvent.Id).OrderBy(s => s.SeatNumber);
            var rows = seats.DistinctBy(s => s.RowNumber).OrderBy(s => s.RowNumber).Select(s => s.RowNumber);
            model.Event = activeEvent;
            model.Rows = rows;
            model.Seats = seats;

            // Get authenticated ticket
            var ticketId = HttpContext.Session.GetInt32("_PingTimeoutTicketId");
            var token = HttpContext.Session.GetString("_PingTimeoutSeatmapToken");
            if (ticketId != null && !string.IsNullOrWhiteSpace(token))
            {
                var ticket = _context.Tickets.Include(t => t.Seat).Where(t => t.Id == ticketId && t.Event.Id == activeEvent.Id && t.SeatMapToken == token).FirstOrDefault();
                if (ticket != null)
                    model.Ticket = ticket;
            }   
                        
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Generate()
        {
            var activeEvent = _context.Events.FirstOrDefault();

            if (activeEvent == null)
                return NotFound();

            var disabled = false;
            for (int row = 1; row < 11; row++) {
                disabled = false;
                if (row > 6)
                    disabled = true;

                for (int seat = 1; seat < 21; seat++) {
                    
                    if (!_context.Seats.Any(s => s.Event.Id == activeEvent.Id && s.RowNumber == row && s.SeatNumber == seat))
                    {
                        _context.Seats.Add(new Seat() {
                            Event = activeEvent,
                            RowNumber = row,
                            SeatNumber = seat,
                            Disabled = ((row == 1 || row == 2) && seat < 11) ? true : disabled
                        });
                    }
                }
            }
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Auth(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return Forbid();

            var ticket = _context.Tickets.FirstOrDefault(t => t.SeatMapToken == token);
            if (ticket == null)
                return Forbid();

            HttpContext.Session.SetInt32("_PingTimeoutTicketId", ticket.Id);
            HttpContext.Session.SetString("_PingTimeoutSeatmapToken", token);

            return RedirectToAction("Index");
        }

        public IActionResult Logout() {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
