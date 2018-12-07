using System;

namespace PingTimeout.Web.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public virtual Event Event { get; set; }

        public string TicketNumber { get; set; }
        public string PickupCode { get; set; }
        public string PurchaseDate { get; set; }
        
        public string PurchaserName { get; set; }
        public string PurchaserEmail { get; set; }

        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime UserBirthdate { get; set; }

        public string SeatMapToken { get; set; }
        public virtual Seat Seat { get; set; }
    }
}