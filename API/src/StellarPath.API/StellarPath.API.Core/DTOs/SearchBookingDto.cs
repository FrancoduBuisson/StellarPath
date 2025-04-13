using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarPath.API.Core.DTOs;

public class SearchBookingsDto
{
    public string? GoogleId { get; set; }
    public int? CruiseId { get; set; }
    public int? BookingStatusId { get; set; }
    public string? StatusName { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? SeatNumber { get; set; }
}