using System;

namespace PingTimeout.Web.Models
{
    public class Seat
    {
        public int Id { get; set; }
        public virtual Event Event { get; set; }

        public int RowNumber { get; set; }
        public int SeatNumber { get; set; }
        public bool Disabled { get; set; }
        public string ReservationOverride { get; set; }
    }
}