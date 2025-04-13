using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarPath.API.Core.Interfaces.Services;

public interface IBookingStatusService
{
    Task<int> GetReservedStatusIdAsync();
    Task<int> GetPaidStatusIdAsync();
    Task<int> GetCompletedStatusIdAsync();
    Task<int> GetCancelledStatusIdAsync();
    Task<int> GetExpiredStatusIdAsync();
    Task<string> GetStatusNameByIdAsync(int statusId);
}
