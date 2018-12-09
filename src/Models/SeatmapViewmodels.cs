using System;
using System.Collections.Generic;

namespace PingTimeout.Web.Models
{
    public class SeatmapIndexViewModel
    {
        public Event Event { get; set; }
        public Ticket Ticket { get; set; }
        public IEnumerable<int> Rows { get; set; }
        public IEnumerable<Seat> Seats { get; set; }
    }

    public class SeatmapTicketViewModel
    {
        public Ticket Ticket { get; set; }
        public Seat Seat { get; set; }
    }
}