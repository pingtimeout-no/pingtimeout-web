using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PingTimeout.Web.Data;

namespace PingTimeout.Web.Hubs {

    public class SeatmapHub : Hub
    {
        private ApplicationDbContext _context;

        public SeatmapHub(ApplicationDbContext context)
        {
           _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var ticketsWithSeats = _context.Tickets.Include(t => t.Seat).Where(t => t.Seat != null);
            foreach (var ticket in ticketsWithSeats) {
                await Clients.Caller.SendAsync("ClaimedSeat", ticket.Id, ticket.Seat.Id, false);
            }
            var speciallyReservedSeats = _context.Seats.Where(s => !string.IsNullOrWhiteSpace(s.ReservationOverride));
            foreach (var seat in speciallyReservedSeats)
            {
                await Clients.Caller.SendAsync("ClaimedSeat", 999, seat.Id, false);
            }
            await base.OnConnectedAsync();
        }

        public async Task ClaimSeat(int ticketId, string token, int seatId)
        {
            var ticket = _context.Tickets.Include(t => t.Seat).Where(t => t.Id == ticketId).FirstOrDefault();
            var previousSeat = ticket.Seat;

            var seat = _context.Seats.Where(s => s.Id == seatId).FirstOrDefault();

            if (ticket == null || seat == null)
            {
                // Ticket or seat not found
                await Clients.Caller.SendAsync("ClaimFailed", seat.Id, "Ukjent billett eller plass");
                return;
            }

            if (ticket.SeatMapToken != token)
            {
                // Token is invalid
                await Clients.Caller.SendAsync("ClaimFailed", seatId, "Du har ikke tilgang til denne billetten");
                return;
            }

            if (_context.Tickets.Any(t => t.Seat.Id == seat.Id && t.Id != ticket.Id)) {
                // Claimed by someone else
                await Clients.Caller.SendAsync("ClaimFailed", seat.Id, "Plassen er opptatt");
                return;
            }

            // Release previous seat
            if (previousSeat != null)
                await Clients.All.SendAsync("ReleasedSeat", previousSeat.Id);

            // Save seat claim, notify everyone
            ticket.Seat = seat;
            await _context.SaveChangesAsync();
            await Clients.All.SendAsync("ClaimedSeat", ticket.Id, seat.Id, true);
        }
    }
}