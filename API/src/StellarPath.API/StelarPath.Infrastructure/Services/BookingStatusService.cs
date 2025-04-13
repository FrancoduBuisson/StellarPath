using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces.Services;

namespace StelarPath.API.Infrastructure.Services;

public class BookingStatusService(IBookingStatusRepository bookingStatusRepository) : IBookingStatusService
{
    private const string STATUS_RESERVED = "Reserved";
    private const string STATUS_PAID = "Paid";
    private const string STATUS_COMPLETED = "Completed";
    private const string STATUS_CANCELLED = "Cancelled";
    private const string STATUS_EXPIRED = "Expired";

    private int? _reservedStatusId;
    private int? _paidStatusId;
    private int? _completedStatusId;
    private int? _cancelledStatusId;
    private int? _expiredStatusId;

    public async Task<int> GetReservedStatusIdAsync()
    {
        if (_reservedStatusId.HasValue)
            return _reservedStatusId.Value;

        var status = await bookingStatusRepository.GetByNameAsync(STATUS_RESERVED);
        _reservedStatusId = status?.BookingStatusId ?? throw new Exception($"Status '{STATUS_RESERVED}' not found");
        return _reservedStatusId.Value;
    }

    public async Task<int> GetPaidStatusIdAsync()
    {
        if (_paidStatusId.HasValue)
            return _paidStatusId.Value;

        var status = await bookingStatusRepository.GetByNameAsync(STATUS_PAID);
        _paidStatusId = status?.BookingStatusId ?? throw new Exception($"Status '{STATUS_PAID}' not found");
        return _paidStatusId.Value;
    }

    public async Task<int> GetCompletedStatusIdAsync()
    {
        if (_completedStatusId.HasValue)
            return _completedStatusId.Value;

        var status = await bookingStatusRepository.GetByNameAsync(STATUS_COMPLETED);
        _completedStatusId = status?.BookingStatusId ?? throw new Exception($"Status '{STATUS_COMPLETED}' not found");
        return _completedStatusId.Value;
    }

    public async Task<int> GetCancelledStatusIdAsync()
    {
        if (_cancelledStatusId.HasValue)
            return _cancelledStatusId.Value;

        var status = await bookingStatusRepository.GetByNameAsync(STATUS_CANCELLED);
        _cancelledStatusId = status?.BookingStatusId ?? throw new Exception($"Status '{STATUS_CANCELLED}' not found");
        return _cancelledStatusId.Value;
    }

    public async Task<int> GetExpiredStatusIdAsync()
    {
        if (_expiredStatusId.HasValue)
            return _expiredStatusId.Value;

        var status = await bookingStatusRepository.GetByNameAsync(STATUS_EXPIRED);
        _expiredStatusId = status?.BookingStatusId ?? throw new Exception($"Status '{STATUS_EXPIRED}' not found");
        return _expiredStatusId.Value;
    }

    public async Task<string> GetStatusNameByIdAsync(int statusId)
    {
        var status = await bookingStatusRepository.GetByIdAsync(statusId);
        return status?.StatusName ?? "Unknown";
    }
}
