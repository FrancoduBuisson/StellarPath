using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces.Services;
using StellarPath.API.Core.Models;

namespace StelarPath.API.Infrastructure.Services;
public class CruiseStatusService(ICruiseStatusRepository cruiseStatusRepository, IUnitOfWork unitOfWork) : ICruiseStatusService
{
    private const string STATUS_SCHEDULED = "Scheduled";
    private const string STATUS_IN_PROGRESS = "In Progress";
    private const string STATUS_COMPLETED = "Completed";
    private const string STATUS_CANCELLED = "Cancelled";

    private int? _scheduledStatusId;
    private int? _inProgressStatusId;
    private int? _completedStatusId;
    private int? _cancelledStatusId;

    public async Task<IEnumerable<CruiseStatus>> GetAllStatusesAsync()
    {
        return await cruiseStatusRepository.GetAllAsync();
    }

    public async Task<CruiseStatus?> GetStatusByIdAsync(int id)
    {
        return await cruiseStatusRepository.GetByIdAsync(id);
    }

    public async Task<CruiseStatus?> GetStatusByNameAsync(string name)
    {
        return await cruiseStatusRepository.GetByNameAsync(name);
    }

    public async Task<int> GetScheduledStatusIdAsync()
    {
        if (_scheduledStatusId.HasValue)
            return _scheduledStatusId.Value;

        var status = await cruiseStatusRepository.GetByNameAsync(STATUS_SCHEDULED);
        _scheduledStatusId = status?.CruiseStatusId ?? throw new Exception($"Status '{STATUS_SCHEDULED}' not found");
        return _scheduledStatusId.Value;
    }

    public async Task<int> GetInProgressStatusIdAsync()
    {
        if (_inProgressStatusId.HasValue)
            return _inProgressStatusId.Value;

        var status = await cruiseStatusRepository.GetByNameAsync(STATUS_IN_PROGRESS);
        _inProgressStatusId = status?.CruiseStatusId ?? throw new Exception($"Status '{STATUS_IN_PROGRESS}' not found");
        return _inProgressStatusId.Value;
    }

    public async Task<int> GetCompletedStatusIdAsync()
    {
        if (_completedStatusId.HasValue)
            return _completedStatusId.Value;

        var status = await cruiseStatusRepository.GetByNameAsync(STATUS_COMPLETED);
        _completedStatusId = status?.CruiseStatusId ?? throw new Exception($"Status '{STATUS_COMPLETED}' not found");
        return _completedStatusId.Value;
    }

    public async Task<int> GetCancelledStatusIdAsync()
    {
        if (_cancelledStatusId.HasValue)
            return _cancelledStatusId.Value;

        var status = await cruiseStatusRepository.GetByNameAsync(STATUS_CANCELLED);
        _cancelledStatusId = status?.CruiseStatusId ?? throw new Exception($"Status '{STATUS_CANCELLED}' not found");
        return _cancelledStatusId.Value;
    }
}