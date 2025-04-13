using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarPath.API.Core.DTOs;

public class SearchBookingHistoryDto
{
    public int? BookingId { get; set; }
    public int? PreviousStatusId { get; set; }
    public int? NewStatusId { get; set; }
    public string? PreviousStatusName { get; set; }
    public string? NewStatusName { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}