using StelarPath.API.Infrastructure.Data.Repositories;
using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces.Services;
using StellarPath.API.Core.Models;

namespace StelarPath.API.Infrastructure.Services;

public class SpaceshipService(
    ISpaceshipRepository spaceshipRepository,
    IShipModelRepository shipModelRepository,
    ICruiseStatusService cruiseStatusService,
    ICruiseRepository cruiseRepository,
    IUnitOfWork unitOfWork) : ISpaceshipService
{
    public async Task<int> CreateSpaceshipAsync(SpaceshipDto spaceshipDto)
    {
        try
        {
            unitOfWork.BeginTransaction();
            var spaceship = MapToEntity(spaceshipDto);
            var result = await spaceshipRepository.AddAsync(spaceship);
            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<bool> DeleteSpaceshipAsync(int id)
    {
        try
        {
            var spaceship = await spaceshipRepository.GetByIdAsync(id);
            if (spaceship == null)
            {
                return false;
            }

            unitOfWork.BeginTransaction();
            var result = await spaceshipRepository.DeleteAsync(id);
            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<bool> DeactivateSpaceshipAsync(int id)
    {
        try
        {
            var spaceship = await spaceshipRepository.GetByIdAsync(id);
            if (spaceship == null)
            {
                return false;
            }

            unitOfWork.BeginTransaction();

            spaceship.IsActive = false;
            var result = await spaceshipRepository.UpdateAsync(spaceship);

            unitOfWork.Commit();

            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<bool> ActivateSpaceshipAsync(int id)
    {
        try
        {
            var spaceship = await spaceshipRepository.GetByIdAsync(id);
            if (spaceship == null)
            {
                return false;
            }

            unitOfWork.BeginTransaction();

            spaceship.IsActive = true;
            var result = await spaceshipRepository.UpdateAsync(spaceship);

            unitOfWork.Commit();

            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<SpaceshipDto>> GetAllSpaceshipsAsync()
    {
        var spaceships = await spaceshipRepository.GetAllAsync();
        var spaceshipDtos = new List<SpaceshipDto>();

        foreach (var spaceship in spaceships)
        {
            var dto = await MapToDtoWithModelDetailsAsync(spaceship);
            spaceshipDtos.Add(dto);
        }

        return spaceshipDtos;
    }

    public async Task<IEnumerable<SpaceshipDto>> GetActiveSpaceshipsAsync()
    {
        var activeSpaceships = await spaceshipRepository.GetActiveSpaceshipsAsync();
        var spaceshipDtos = new List<SpaceshipDto>();

        foreach (var spaceship in activeSpaceships)
        {
            var dto = await MapToDtoWithModelDetailsAsync(spaceship);
            spaceshipDtos.Add(dto);
        }

        return spaceshipDtos;
    }

    public async Task<SpaceshipDto?> GetSpaceshipByIdAsync(int id)
    {
        var spaceship = await spaceshipRepository.GetByIdAsync(id);
        return spaceship != null ? await MapToDtoWithModelDetailsAsync(spaceship) : null;
    }

    public async Task<IEnumerable<SpaceshipDto>> GetSpaceshipsByModelIdAsync(int modelId)
    {
        var spaceships = await spaceshipRepository.GetSpaceshipsByModelIdAsync(modelId);
        var spaceshipDtos = new List<SpaceshipDto>();

        foreach (var spaceship in spaceships)
        {
            var dto = await MapToDtoWithModelDetailsAsync(spaceship);
            spaceshipDtos.Add(dto);
        }

        return spaceshipDtos;
    }

    public async Task<bool> UpdateSpaceshipAsync(SpaceshipDto spaceshipDto)
    {
        try
        {
            unitOfWork.BeginTransaction();
            var spaceship = MapToEntity(spaceshipDto);
            var result = await spaceshipRepository.UpdateAsync(spaceship);
            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<SpaceshipDto>> SearchSpaceshipsAsync(
        int? modelId, string? modelName, bool? isActive)
    {
        var spaceships = await spaceshipRepository.SearchSpaceshipsAsync(
            modelId, modelName, isActive);

        var spaceshipDtos = new List<SpaceshipDto>();
        foreach (var spaceship in spaceships)
        {
            var dto = await MapToDtoWithModelDetailsAsync(spaceship);
            spaceshipDtos.Add(dto);
        }

        return spaceshipDtos;
    }

    public async Task<IEnumerable<SpaceshipAvailabilityDto>> GetAvailableSpaceshipsForTimeRangeAsync(
       DateTime startTime, DateTime endTime)
    {
        var activeSpaceships = await spaceshipRepository.GetActiveSpaceshipsAsync();
        var availabilityDtos = new List<SpaceshipAvailabilityDto>();

        int cancelledStatusId = await cruiseStatusService.GetCancelledStatusIdAsync();

        foreach (var spaceship in activeSpaceships)
        {
            var model = await shipModelRepository.GetByIdAsync(spaceship.ModelId);
            if (model == null) continue;

            // all cruises for this spaceship that overlap with the requested time range + not cancelled
            var overlappingCruises = await cruiseRepository.GetOverlappingCruisesForSpaceshipAsync(
                spaceship.SpaceshipId, startTime, endTime);

            // spaceship is fully available
            if (!overlappingCruises.Any())
            {
                availabilityDtos.Add(new SpaceshipAvailabilityDto
                {
                    SpaceshipId = spaceship.SpaceshipId,
                    ModelId = model.ModelId,
                    ModelName = model.ModelName,
                    Capacity = model.Capacity,
                    CruiseSpeedKmph = model.CruiseSpeedKmph,
                    IsActive = spaceship.IsActive,
                    AvailableTimeSlots = new List<TimeSlot>
                    {
                        new TimeSlot { StartTime = startTime, EndTime = endTime }
                    }
                });
                continue;
            }

            // can calculate available time slots
            var timeSlots = CalculateAvailableTimeSlots(startTime, endTime, overlappingCruises);

            if (timeSlots.Any())
            {
                availabilityDtos.Add(new SpaceshipAvailabilityDto
                {
                    SpaceshipId = spaceship.SpaceshipId,
                    ModelId = model.ModelId,
                    ModelName = model.ModelName,
                    Capacity = model.Capacity,
                    CruiseSpeedKmph = model.CruiseSpeedKmph,
                    IsActive = spaceship.IsActive,
                    AvailableTimeSlots = timeSlots
                });
            }
        }

        return availabilityDtos;
    }

    // actually do the overlap calc
    private List<TimeSlot> CalculateAvailableTimeSlots(DateTime rangeStart, DateTime rangeEnd, IEnumerable<Cruise> overlappingCruises)
    {
        var timeSlots = new List<TimeSlot>();
        var busySlots = new List<(DateTime Start, DateTime End)>();

        // convert all cruises to busy time slots
        foreach (var cruise in overlappingCruises)
        {
            var cruiseEnd = cruise.LocalDepartureTime.AddMinutes(cruise.DurationMinutes);
            busySlots.Add((cruise.LocalDepartureTime, cruiseEnd));
        }

        // sort by start time
        busySlots = busySlots.OrderBy(s => s.Start).ToList();

        // find available slots
        DateTime currentStart = rangeStart;

        foreach (var (busyStart, busyEnd) in busySlots)
        {
            // if there's a gap before this busy slot, add it as available
            if (currentStart < busyStart)
            {
                timeSlots.Add(new TimeSlot
                {
                    StartTime = currentStart,
                    EndTime = busyStart
                });
            }

            // move on to next
            currentStart = busyEnd > currentStart ? busyEnd : currentStart;
        }

        // add finfal if need
        if (currentStart < rangeEnd)
        {
            timeSlots.Add(new TimeSlot
            {
                StartTime = currentStart,
                EndTime = rangeEnd
            });
        }

        return timeSlots;
    }


    private async Task<SpaceshipDto> MapToDtoWithModelDetailsAsync(Spaceship spaceship)
    {
        var model = await shipModelRepository.GetByIdAsync(spaceship.ModelId);

        return new SpaceshipDto
        {
            SpaceshipId = spaceship.SpaceshipId,
            ModelId = spaceship.ModelId,
            ModelName = model?.ModelName,
            Capacity = model?.Capacity,
            CruiseSpeedKmph = model?.CruiseSpeedKmph,
            IsActive = spaceship.IsActive
        };
    }

    private static Spaceship MapToEntity(SpaceshipDto dto)
    {
        return new Spaceship
        {
            SpaceshipId = dto.SpaceshipId,
            ModelId = dto.ModelId,
            IsActive = dto.IsActive
        };
    }
}