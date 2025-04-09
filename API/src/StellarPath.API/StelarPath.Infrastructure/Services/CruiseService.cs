using StellarPath.API.Core.DTOs;
using StellarPath.API.Core.Interfaces.Repositories;
using StellarPath.API.Core.Interfaces.Services;
using StellarPath.API.Core.Interfaces;
using StellarPath.API.Core.Models;
using StelarPath.API.Infrastructure.Data.Repositories;

public class CruiseService(
   ICruiseRepository cruiseRepository,
   ISpaceshipRepository spaceshipRepository,
   IShipModelRepository shipModelRepository,
   IDestinationRepository destinationRepository,
   IUserRepository userRepository,
   ICruiseStatusService cruiseStatusService,
   IUnitOfWork unitOfWork) : ICruiseService
{
    public async Task<int> CreateCruiseAsync(CreateCruiseDto cruiseDto, string createdByGoogleId)
    {
        try
        {
            // first very ship active ++ found
            var spaceship = await spaceshipRepository.GetByIdAsync(cruiseDto.SpaceshipId);
            if (spaceship == null || !spaceship.IsActive)
            {
                throw new ArgumentException($"Spaceship with ID {cruiseDto.SpaceshipId} not found or is inactive");
            }

            // ensure dep active
            var departureDestination = await destinationRepository.GetByIdAsync(cruiseDto.DepartureDestinationId);
            if (departureDestination == null || !departureDestination.IsActive)
            {
                throw new ArgumentException($"Departure destination with ID {cruiseDto.DepartureDestinationId} not found or is inactive");
            }

            // ensure dest active
            var arrivalDestination = await destinationRepository.GetByIdAsync(cruiseDto.ArrivalDestinationId);
            if (arrivalDestination == null || !arrivalDestination.IsActive)
            {
                throw new ArgumentException($"Arrival destination with ID {cruiseDto.ArrivalDestinationId} not found or is inactive");
            }

            // cant go to the same place
            if (cruiseDto.DepartureDestinationId == cruiseDto.ArrivalDestinationId)
            {
                throw new ArgumentException("Departure and arrival destinations cannot be the same");
            }

            // beed ship for upcoming calculations
            var shipModel = await shipModelRepository.GetByIdAsync(spaceship.ModelId);
            if (shipModel == null)
            {
                throw new ArgumentException($"Ship model for spaceship with ID {cruiseDto.SpaceshipId} not found");
            }

            // calc the distance between the two destinations, we just assume straight line
            long distanceToTravel = Math.Abs(arrivalDestination.DistanceFromEarth - departureDestination.DistanceFromEarth);

            // calc the duration based on distance and cruise speed
            int durationMinutes = (int)Math.Ceiling((double)distanceToTravel / shipModel.CruiseSpeedKmph * 60);

            // check for overlapping cruises
            var estimatedArrivalTime = cruiseDto.LocalDepartureTime.AddMinutes(durationMinutes);
            var overlappingCruises = await cruiseRepository.GetOverlappingCruisesForSpaceshipAsync(
                cruiseDto.SpaceshipId,
                cruiseDto.LocalDepartureTime,
                estimatedArrivalTime);

            if (overlappingCruises.Any())
            {
                throw new InvalidOperationException("The spaceship is already scheduled for a cruise during this time period");
            }

            // dt the initial status
            int statusId = await cruiseStatusService.GetScheduledStatusIdAsync();
            var now = DateTime.UtcNow;

            if (cruiseDto.LocalDepartureTime <= now)
            {
                statusId = await cruiseStatusService.GetInProgressStatusIdAsync();

                if (estimatedArrivalTime <= now)
                {
                    statusId = await cruiseStatusService.GetCompletedStatusIdAsync();
                }
            }

            unitOfWork.BeginTransaction();

            var cruise = new Cruise
            {
                SpaceshipId = cruiseDto.SpaceshipId,
                DepartureDestinationId = cruiseDto.DepartureDestinationId,
                ArrivalDestinationId = cruiseDto.ArrivalDestinationId,
                LocalDepartureTime = cruiseDto.LocalDepartureTime,
                DurationMinutes = durationMinutes,
                CruiseSeatPrice = cruiseDto.CruiseSeatPrice,
                CruiseStatusId = statusId,
                CreatedByGoogleId = createdByGoogleId
            };

            var result = await cruiseRepository.AddAsync(cruise);
            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }
    
    public async Task<IEnumerable<CruiseDto>> GetCruisesByStatusAsync(int statusId)
    {
        var cruises = await cruiseRepository.GetCruisesByStatusAsync(statusId);
        var cruiseDtos = new List<CruiseDto>();

        foreach (var cruise in cruises)
        {
            var dto = await MapToDtoWithDetailsAsync(cruise);
            cruiseDtos.Add(dto);
        }

        return cruiseDtos;
    }
    
    public async Task<bool> CancelCruiseAsync(int id)
    {
        try
        {
            var cruise = await cruiseRepository.GetByIdAsync(id);
            if (cruise == null)
            {
                return false;
            }

            // only cancel Scheduled cruises
            int scheduledStatusId = await cruiseStatusService.GetScheduledStatusIdAsync();
            if (cruise.CruiseStatusId != scheduledStatusId)
            {
                throw new InvalidOperationException($"Only scheduled cruises can be cancelled. Current status: {cruise.CruiseStatusId}");
            }

            unitOfWork.BeginTransaction();

            int cancelledStatusId = await cruiseStatusService.GetCancelledStatusIdAsync();
            var result = await cruiseRepository.UpdateCruiseStatusAsync(id, cancelledStatusId);

            // TODO: Need to canceld assoc bkings

            unitOfWork.Commit();
            return result;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<IEnumerable<CruiseDto>> GetAllCruisesAsync()
    {
        var cruises = await cruiseRepository.GetAllAsync();
        var cruiseDtos = new List<CruiseDto>();

        foreach (var cruise in cruises)
        {
            var dto = await MapToDtoWithDetailsAsync(cruise);
            cruiseDtos.Add(dto);
        }

        return cruiseDtos;
    }

    public async Task<CruiseDto?> GetCruiseByIdAsync(int id)
    {
        var cruise = await cruiseRepository.GetByIdAsync(id);
        return cruise != null ? await MapToDtoWithDetailsAsync(cruise) : null;
    }

    public async Task<IEnumerable<CruiseDto>> GetCruisesBySpaceshipIdAsync(int spaceshipId)
    {
        var cruises = await cruiseRepository.GetCruisesBySpaceshipIdAsync(spaceshipId);
        var cruiseDtos = new List<CruiseDto>();

        foreach (var cruise in cruises)
        {
            var dto = await MapToDtoWithDetailsAsync(cruise);
            cruiseDtos.Add(dto);
        }

        return cruiseDtos;
    }

    public async Task<IEnumerable<CruiseDto>> SearchCruisesAsync(
        int? spaceshipId,
        int? departureDestinationId,
        int? arrivalDestinationId,
        DateTime? startDate,
        DateTime? endDate,
        int? statusId,
        decimal? minPrice,
        decimal? maxPrice)
    {
        var cruises = await cruiseRepository.SearchCruisesAsync(
            spaceshipId, departureDestinationId, arrivalDestinationId,
            startDate, endDate, statusId, minPrice, maxPrice);

        var cruiseDtos = new List<CruiseDto>();
        foreach (var cruise in cruises)
        {
            var dto = await MapToDtoWithDetailsAsync(cruise);
            cruiseDtos.Add(dto);
        }

        return cruiseDtos;
    }

    public async Task<bool> UpdateCruiseStatusesAsync()
    {
        try
        {
            var now = DateTime.UtcNow;

            int scheduledStatusId = await cruiseStatusService.GetScheduledStatusIdAsync();
            int inProgressStatusId = await cruiseStatusService.GetInProgressStatusIdAsync();
            int completedStatusId = await cruiseStatusService.GetCompletedStatusIdAsync();

            var scheduledCruises = await cruiseRepository.GetCruisesByStatusAsync(scheduledStatusId);
            var inProgressCruises = await cruiseRepository.GetCruisesByStatusAsync(inProgressStatusId);

            unitOfWork.BeginTransaction();

            // update scheduled cruises that should now be in progress
            foreach (var cruise in scheduledCruises)
            {
                if (cruise.LocalDepartureTime <= now)
                {
                    await cruiseRepository.UpdateCruiseStatusAsync(cruise.CruiseId, inProgressStatusId);
                }
            }

            // update in-progress cruises that should now be completed
            foreach (var cruise in inProgressCruises)
            {
                var estimatedArrivalTime = cruise.LocalDepartureTime.AddMinutes(cruise.DurationMinutes);
                if (estimatedArrivalTime <= now)
                {
                    await cruiseRepository.UpdateCruiseStatusAsync(cruise.CruiseId, completedStatusId);
                }
            }

            unitOfWork.Commit();
            return true;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    private async Task<CruiseDto> MapToDtoWithDetailsAsync(Cruise cruise)
    {
        // Rrlated things
        var spaceship = await spaceshipRepository.GetByIdAsync(cruise.SpaceshipId);
        var departureDestination = await destinationRepository.GetByIdAsync(cruise.DepartureDestinationId);
        var arrivalDestination = await destinationRepository.GetByIdAsync(cruise.ArrivalDestinationId);
        var createdByUser = await userRepository.GetByGoogleIdAsync(cruise.CreatedByGoogleId);

        // get the ship model details from the spaceship
        ShipModel? shipModel = null;
        if (spaceship != null)
        {
            shipModel = await shipModelRepository.GetByIdAsync(spaceship.ModelId);
        }

        var allStatuses = await cruiseStatusService.GetAllStatusesAsync();
        var cruiseStatus = allStatuses.FirstOrDefault(s => s.CruiseStatusId == cruise.CruiseStatusId);

        return new CruiseDto
        {
            CruiseId = cruise.CruiseId,
            SpaceshipId = cruise.SpaceshipId,
            SpaceshipName = shipModel?.ModelName,
            Capacity = shipModel?.Capacity,
            CruiseSpeedKmph = shipModel?.CruiseSpeedKmph,
            DepartureDestinationId = cruise.DepartureDestinationId,
            DepartureDestinationName = departureDestination?.Name,
            ArrivalDestinationId = cruise.ArrivalDestinationId,
            ArrivalDestinationName = arrivalDestination?.Name,
            LocalDepartureTime = cruise.LocalDepartureTime,
            DurationMinutes = cruise.DurationMinutes,
            CruiseSeatPrice = cruise.CruiseSeatPrice,
            CruiseStatusId = cruise.CruiseStatusId,
            CruiseStatusName = cruiseStatus?.StatusName,
            CreatedByGoogleId = cruise.CreatedByGoogleId,
            CreatedByName = createdByUser != null ? $"{createdByUser.FirstName} {createdByUser.LastName}" : null
        };
    }

    public async Task<IEnumerable<CruiseDto>> GetCruisesByDepartureDestinationAsync(int destinationId)
    {
        var cruises = await cruiseRepository.GetCruisesByDepartureDestinationAsync(destinationId);
        var cruiseDtos = new List<CruiseDto>();

        foreach (var cruise in cruises)
        {
            var dto = await MapToDtoWithDetailsAsync(cruise);
            cruiseDtos.Add(dto);
        }

        return cruiseDtos;
    }

    public async Task<IEnumerable<CruiseDto>> GetCruisesByArrivalDestinationAsync(int destinationId)
    {
        var cruises = await cruiseRepository.GetCruisesByArrivalDestinationAsync(destinationId);
        var cruiseDtos = new List<CruiseDto>();

        foreach (var cruise in cruises)
        {
            var dto = await MapToDtoWithDetailsAsync(cruise);
            cruiseDtos.Add(dto);
        }

        return cruiseDtos;
    }

    public async Task<IEnumerable<CruiseDto>> GetCruisesBetweenDatesAsync(DateTime startDate, DateTime endDate)
    {
        var cruises = await cruiseRepository.GetCruisesBetweenDatesAsync(startDate, endDate);
        var cruiseDtos = new List<CruiseDto>();

        foreach (var cruise in cruises)
        {
            var dto = await MapToDtoWithDetailsAsync(cruise);
            cruiseDtos.Add(dto);
        }

        return cruiseDtos;
    }

    public async Task<IEnumerable<CruiseDto>> GetCruisesCreatedByUserAsync(string googleId)
    {
        var cruises = await cruiseRepository.GetCruisesCreatedByUserAsync(googleId);
        var cruiseDtos = new List<CruiseDto>();

        foreach (var cruise in cruises)
        {
            var dto = await MapToDtoWithDetailsAsync(cruise);
            cruiseDtos.Add(dto);
        }

        return cruiseDtos;
    }
}