using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarPath.API.Core.Models
{
    public class BookingHistory
    {
        public int HistoryId { get; set; }
        public int BookingId { get; set; }
        public int PreviousBookingStatusId { get; set; }
        public int NewBookingStatusId { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
