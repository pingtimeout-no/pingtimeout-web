using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
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

        [Authorize(Roles = "Admin")]
        public IActionResult Print(int eventId)
        {
            var tmpFile = Path.GetTempFileName();

            var activeEvent = eventId > 0 ? _context.Events.Find(eventId) : _context.Events.FirstOrDefault();

            PdfDocument document = new PdfDocument();
            document.Info.Title = $"Seatmap {activeEvent.Name}";
            var seats = _context.Seats.OrderBy(m => m.RowNumber).ThenBy(m => m.SeatNumber);
            foreach (var seat in seats)
            {
                PdfPage page = document.AddPage();
                page.Size = PdfSharp.PageSize.A4;
                page.Orientation = PdfSharp.PageOrientation.Landscape;

                XGraphics gfx = XGraphics.FromPdfPage(page);
                XFont boldfont = new XFont("Courier New", 60, XFontStyle.Bold);
                XFont font = new XFont("Courier New", 48, XFontStyle.Regular);
                gfx.DrawString($"Rad {seat.RowNumber} | Sete {seat.SeatNumber}", boldfont, XBrushes.Black, new XRect(0, 0, page.Width, page.Height / 2), XStringFormats.BottomCenter);


                var bottomRect = new XRect(50, page.Height / 2, page.Width-100, page.Height / 2);

                string text = "(ikke reservert)";
                var ticket = _context.Tickets.FirstOrDefault(t => t.Seat.Id == seat.Id);
                if (ticket != null)
                    text = $"{ticket.UserName}";
                else if (seat.Disabled)
                    text = $"Reservert for CREW";

                gfx.DrawString(text, font, XBrushes.Gray, bottomRect, XStringFormats.TopCenter);

            }

            document.Save(tmpFile);

            var stream = new FileStream(tmpFile, FileMode.Open);

            if (stream == null)
                return NotFound();

            var date = DateTime.Now.ToShortDateString();
            return File(stream, "application/pdf", $"{activeEvent.Name} seatmap {DateTime.Now.ToString("yyyy-MM-dd HHmmss")}");
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
