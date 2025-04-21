# StellarPath

StellarPath is a project that provides a CLI application for intergalactic travel management, with features to book a trip, see available trips, cancel a booked trip, and to list all destinations that can be visited

## Getting Started

## Docker related commands

Ensure target exists
dotnet run -- build

- run docker-compose file

### Prerequisites

- .NET 9.0.203+

### Running the Server

1. Open a terminal or command prompt.
2. Navigate to the `api` directory:
    ```sh
    cd API\src\StellarPath.API

    dotnet run
    ```
You can access the API from port 5291 on your local browser:
http://localhost:5291/swagger/index.html

### Running the CLI Application

1. Open a terminal or command prompt.
2. Navigate to the `cli` directory:

    ```sh
    cd CLi

    cd src
    ```

3. Run the CLI application:

    ```sh
    dotnet run
    ```

### Features

- Google OAuth2 authentication
- Intergalactic Travel Management

### Project Structure

- `api/`: Contains the backend API for the project.
- `cli/`: Contains the CLI application for the project.


### Booking Related Diagrams
## Create Booking
```mermaid
sequenceDiagram
    participant Client
    participant BookingEndpoints as BookingEndpoints
    participant BookingService
    participant IUnitOfWork
    participant BookingRepository
    participant CruiseService
    participant BookingStatusService
    participant BookingHistoryRepository
    
    Client->>BookingEndpoints: POST /api/bookings (CreateBookingDto)
    Note over BookingEndpoints: Extract GoogleId from JWT token
    BookingEndpoints->>BookingService: CreateBookingAsync(bookingDto, googleId)
    
    BookingService->>CruiseService: GetCruiseByIdAsync(cruiseId)
    CruiseService-->>BookingService: CruiseDto
    
    Note over BookingService: Validate cruise exists
    Note over BookingService: Validate cruise status (Scheduled)
    Note over BookingService: Validate seat number range
    
    BookingService->>BookingService: GetAvailableSeatsForCruiseAsync(cruiseId)
    BookingService->>BookingRepository: GetActiveBookingsForCruiseAsync(cruiseId)
    BookingRepository-->>BookingService: List<Booking>
    
    Note over BookingService: Verify seat is available
    
    BookingService->>BookingStatusService: GetReservedStatusIdAsync()
    BookingStatusService-->>BookingService: statusId
    
    BookingService->>IUnitOfWork: BeginTransaction()
    
    Note over BookingService: Create new Booking object
    
    BookingService->>BookingRepository: AddAsync(booking)
    BookingRepository-->>BookingService: bookingId
    
    Note over BookingService: Create booking history record
    
    BookingService->>BookingHistoryRepository: AddAsync(bookingHistory)
    BookingHistoryRepository-->>BookingService: historyId
    
    BookingService->>IUnitOfWork: Commit()
    
    BookingService-->>BookingEndpoints: bookingId
    BookingEndpoints-->>Client: 201 Created (bookingId)
    
    Note over Client, BookingEndpoints: Error paths:
    Note over BookingEndpoints, BookingService: 400 Bad Request - Invalid seat, already taken seat, etc.
    Note over BookingEndpoints, BookingService: 401 Unauthorized - Invalid JWT
    Note over BookingEndpoints, BookingService: 500 Internal Server Error - Unexpected errors

```
## Pay For Booking
```mermaid
sequenceDiagram
    participant Client
    participant BookingEndpoints as BookingEndpoints
    participant BookingService
    participant IUnitOfWork
    participant BookingRepository
    participant BookingStatusService
    participant BookingHistoryRepository
    
    Client->>BookingEndpoints: PATCH /api/bookings/{id}/pay
    Note over BookingEndpoints: Extract GoogleId from JWT token
    BookingEndpoints->>BookingService: PayForBookingAsync(id, googleId)
    
    BookingService->>BookingRepository: GetByIdAsync(id)
    BookingRepository-->>BookingService: booking
    
    Note over BookingService: Verify booking exists
    Note over BookingService: Verify booking belongs to user
    
    BookingService->>BookingStatusService: GetReservedStatusIdAsync()
    BookingStatusService-->>BookingService: reservedStatusId
    
    Note over BookingService: Verify booking has 'Reserved' status
    
    BookingService->>BookingService: UpdateBookingIfExpiredAsync(booking)
    
    Note over BookingService: Check if booking expired 
    
    BookingService->>BookingStatusService: GetReservedStatusIdAsync()
    BookingStatusService-->>BookingService: reservedStatusId
    
    Note over BookingService: Verify booking still in 'Reserved' status after expiration check
    
    BookingService->>IUnitOfWork: BeginTransaction()
    
    BookingService->>BookingStatusService: GetPaidStatusIdAsync()
    BookingStatusService-->>BookingService: paidStatusId
    
    Note over BookingService: Create booking history record
    
    BookingService->>BookingHistoryRepository: AddAsync(bookingHistory)
    BookingHistoryRepository-->>BookingService: historyId
    
    BookingService->>BookingRepository: UpdateBookingStatusAsync(id, paidStatusId)
    BookingRepository-->>BookingService: success (true)
    
    BookingService->>IUnitOfWork: Commit()
    
    BookingService-->>BookingEndpoints: success (true)
    BookingEndpoints-->>Client: 204 No Content
    
    Note over Client, BookingEndpoints: Error paths:
    Note over BookingEndpoints, BookingService: 400 Bad Request - Booking expired or invalid state
    Note over BookingEndpoints, BookingService: 401 Unauthorized - Invalid JWT or not owner
    Note over BookingEndpoints, BookingService: 404 Not Found - Booking doesn't exist
    Note over BookingEndpoints, BookingService: 500 Internal Server Error - Unexpected errors
```
## Cancel Booking
```mermaid
sequenceDiagram
    participant Client
    participant BookingEndpoints as BookingEndpoints
    participant BookingService
    participant IUnitOfWork
    participant BookingRepository
    participant BookingStatusService
    participant BookingHistoryRepository
    
    Client->>BookingEndpoints: PATCH /api/bookings/{id}/cancel
    Note over BookingEndpoints: Extract GoogleId from JWT token
    BookingEndpoints->>BookingService: CancelBookingAsync(id, googleId)
    
    BookingService->>BookingRepository: GetByIdAsync(id)
    BookingRepository-->>BookingService: booking
    
    Note over BookingService: Verify booking exists
    
    Note over BookingService: Verify booking belongs to user
    Note over BookingService: Otherwise, throw UnauthorizedAccessException
    
    BookingService->>BookingStatusService: GetCancelledStatusIdAsync()
    BookingStatusService-->>BookingService: cancelledStatusId
    
    BookingService->>BookingStatusService: GetCompletedStatusIdAsync()
    BookingStatusService-->>BookingService: completedStatusId
    
    Note over BookingService: Verify booking is not already cancelled or completed
    Note over BookingService: Otherwise, throw InvalidOperationException
    
    BookingService->>IUnitOfWork: BeginTransaction()
    
    Note over BookingService: Create booking history record
    
    BookingService->>BookingHistoryRepository: AddAsync(bookingHistory)
    BookingHistoryRepository-->>BookingService: historyId
    
    BookingService->>BookingRepository: UpdateBookingStatusAsync(id, cancelledStatusId)
    BookingRepository-->>BookingService: success (true)
    
    BookingService->>IUnitOfWork: Commit()
    
    BookingService-->>BookingEndpoints: success (true)
    BookingEndpoints-->>Client: 204 No Content
    
    Note over Client, BookingEndpoints: Error paths:
    Note over BookingEndpoints, BookingService: 400 Bad Request - Already cancelled/completed
    Note over BookingEndpoints, BookingService: 401 Unauthorized - Invalid JWT or not owner
    Note over BookingEndpoints, BookingService: 404 Not Found - Booking doesn't exist
    Note over BookingEndpoints, BookingService: 500 Internal Server Error - Unexpected errors
```
## Get Available Seats
```mermaid
sequenceDiagram
    participant Client
    participant BookingEndpoints as BookingEndpoints
    participant BookingService
    participant CruiseService
    participant BookingRepository
    
    Client->>BookingEndpoints: GET /api/bookings/cruise/{cruiseId}/seats
    BookingEndpoints->>BookingService: GetAvailableSeatsForCruiseAsync(cruiseId)
    
    BookingService->>CruiseService: GetCruiseByIdAsync(cruiseId)
    CruiseService-->>BookingService: cruise
    
    Note over BookingService: Verify cruise exists
    Note over BookingService: Otherwise, throw ArgumentException
    
    Note over BookingService: Extract ship capacity from cruise
    
    BookingService->>BookingRepository: GetActiveBookingsForCruiseAsync(cruiseId)
    BookingRepository-->>BookingService: List<Booking>
    
    Note over BookingService: Calculate available seats by comparing
    Note over BookingService: all seats (1 to capacity) with booked seats
    
    BookingService-->>BookingEndpoints: List<int> (available seat numbers)
    BookingEndpoints-->>Client: 200 OK (List of available seat numbers)
    
    Note over Client, BookingEndpoints: Error paths:
    Note over BookingEndpoints, BookingService: 400 Bad Request - Invalid cruise ID
    Note over BookingEndpoints, BookingService: 404 Not Found - Cruise doesn't exist
    Note over BookingEndpoints, BookingService: 500 Internal Server Error - Unexpected errors
```
## System Architecture
```mermaid
flowchart TD
    subgraph "Client Applications"
        CLI["StellarPath CLI\n(Console App)"]
    end
    
    subgraph "API Layer"
        API["ASP.NET API\n(Minimal API Endpoints)"]
        Auth["Authentication\n(JWT + Google OAuth)"]
        
        subgraph "Endpoints" 
            GalaxyEP["Galaxy Endpoints"]
            StarSystemEP["Star System Endpoints"]
            DestinationEP["Destination Endpoints"]
            ShipModelEP["Ship Model Endpoints"]
            SpaceshipEP["Spaceship Endpoints"]
            CruiseEP["Cruise Endpoints"]
            BookingEP["Booking Endpoints"]
            UserEP["User Endpoints"]
            NasaEP["NASA APOD Endpoints"]
            PlanetEP["Planets API Endpoints"]
        end
    end
    
    subgraph "Core Layer"
        Interfaces["Core Interfaces"]
        Models["Domain Models"]
        DTOs["Data Transfer Objects"]
        Config["Configuration Models"]
    end
    
    subgraph "Infrastructure Layer"
        Services["Service Implementations"]
        Repositories["Repository Implementations"]
        DbContext["Database Access\n(Dapper)"]
        ExternalAPIs["External API Clients\n(NASA, Planets)"]
    end
    
    subgraph "Database"
        PostgreSQL["PostgreSQL Database"]
    end
    
    subgraph "External Services"
        GoogleAuth["Google OAuth"]
        NasaAPI["NASA APOD API"]
        PlanetsAPI["Planets API"]
    end
    
    %% Client to API connections
    CLI -->|HTTP| API
    WebUI -->|HTTP| API
    
    %% API to Core and Infrastructure connections
    API --> Auth
    API --> Endpoints
    
    GalaxyEP & StarSystemEP & DestinationEP & ShipModelEP & SpaceshipEP & CruiseEP & BookingEP & UserEP & NasaEP & PlanetEP --> Services
    
    %% Core and Infrastructure connections
    Services --> Interfaces
    Services --> Models
    Services --> DTOs
    Services --> Repositories
    Repositories --> DbContext
    Services --> ExternalAPIs
    
    %% Database and External Services connections
    DbContext --> PostgreSQL
    Auth -->|OAuth| GoogleAuth
    ExternalAPIs -->|HTTP| NasaAPI
    ExternalAPIs -->|HTTP| PlanetsAPI
```

## Domain Model Diagram
```mermaid
classDiagram
    class Booking {
        +int BookingId
        +string GoogleId
        +int CruiseId
        +int SeatNumber
        +DateTime BookingDate
        +DateTime BookingExpiration
        +int BookingStatusId
    }
    
    class BookingStatus {
        +int BookingStatusId
        +string StatusName
    }
    
    class BookingHistory {
        +int HistoryId
        +int BookingId
        +int PreviousBookingStatusId
        +int NewBookingStatusId
        +DateTime ChangedAt
    }
    
    class User {
        +string GoogleId
        +string Email
        +string FirstName
        +string LastName
        +int RoleId
        +bool IsActive
    }
    
    class Cruise {
        +int CruiseId
        +int SpaceshipId
        +int DepartureDestinationId
        +int ArrivalDestinationId
        +DateTime LocalDepartureTime
        +int DurationMinutes
        +decimal CruiseSeatPrice
        +int CruiseStatusId
        +string CreatedByGoogleId
    }
    
    class CruiseStatus {
        +int CruiseStatusId
        +string StatusName
    }
    
    class Spaceship {
        +int SpaceshipId
        +int ModelId
        +bool IsActive
    }
    
    class ShipModel {
        +int ModelId
        +string ModelName
        +int Capacity
        +int CruiseSpeedKmph
    }
    
    class Destination {
        +int DestinationId
        +string Name
        +int SystemId
        +long DistanceFromEarth
        +bool IsActive
    }
    
    class StarSystem {
        +int SystemId
        +string SystemName
        +int GalaxyId
        +bool IsActive
    }
    
    class Galaxy {
        +int GalaxyId
        +string GalaxyName
        +bool IsActive
    }
    
    Booking "many" --> "1" BookingStatus : has status
    Booking "many" --> "1" User : belongs to
    Booking "many" --> "1" Cruise : books
    BookingHistory "many" --> "1" Booking : tracks changes for
    BookingHistory "many" --> "1" BookingStatus : previous status
    BookingHistory "many" --> "1" BookingStatus : new status
    
    Cruise "many" --> "1" Spaceship : uses
    Cruise "many" --> "1" Destination : departs from
    Cruise "many" --> "1" Destination : arrives at
    Cruise "many" --> "1" CruiseStatus : has status
    Cruise "many" --> "1" User : created by
    
    Spaceship "many" --> "1" ShipModel : has model
    
    Destination "many" --> "1" StarSystem : belongs to
    StarSystem "many" --> "1" Galaxy : belongs to
```

