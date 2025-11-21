# Event Booking System - Technical Implementation

A comprehensive event booking system built with C# and ASP.NET Core, demonstrating object-oriented design, payment integration, and advanced data access patterns.

## Table of Contents
- [Architecture Overview](#architecture-overview)
- [Object-Oriented Design](#object-oriented-design)
- [Payment Gateway Integration](#payment-gateway-integration)
- [Data Access Layer](#data-access-layer)
- [Advanced SQL Queries](#advanced-sql-queries)
- [API Endpoints](#api-endpoints)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)

## Architecture Overview

The system follows the ASP.NET MVC pattern with a clean separation of concerns:

```
BookingSystem/
├── Models/                    # Domain entities
│   ├── User.cs
│   ├── Venue.cs
│   ├── Event.cs
│   ├── Booking.cs
│   └── Seating/              # Seating type abstractions
│       ├── ISeatingType.cs
│       ├── FullReservedSeating.cs
│       ├── SectionReservedSeating.cs
│       └── OpenSeating.cs
├── Services/                  # Business logic
│   ├── Booking/
│   │   ├── IBookingService.cs
│   │   ├── BookingService.cs
│   │   ├── CreateBookingRequest.cs
│   │   └── BookingResult.cs
│   └── Payment/
│       ├── IPaymentGateway.cs
│       ├── PaymentGateway.cs
│       ├── SimulatedPaymentGateway.cs
│       ├── PaymentRequest.cs
│       └── PaymentResponse.cs
├── DataAccess/               # Data access layer
│   ├── Interfaces
│   │   ├── IUserRepository.cs
│   │   ├── IVenueRepository.cs
│   │   ├── IEventRepository.cs
│   │   └── IBookingRepository.cs
│   ├── InMemory/            # In-memory implementations
│   │   ├── InMemoryUserRepository.cs
│   │   ├── InMemoryVenueRepository.cs
│   │   ├── InMemoryEventRepository.cs
│   │   └── InMemoryBookingRepository.cs
│   └── Sql/                 # SQL implementations
│       ├── SqlUserRepository.cs
│       ├── SqlVenueRepository.cs
│       ├── SqlEventRepository.cs
│       └── SqlBookingRepository.cs
├── Controllers/              # API controllers
│   ├── BookingsController.cs
│   ├── EventsController.cs
│   ├── VenuesController.cs
│   └── UsersController.cs
├── Database/
│   └── schema.sql           # Complete database schema
├── Program.cs               # DI configuration
└── appsettings.json        # Application configuration
```

## Object-Oriented Design

### Core Entities

#### 1. User (Models/User.cs:1)
Represents a customer in the system.
```csharp
- Id: int
- FirstName: string
- LastName: string
- Email: string (unique)
- CreatedAt: DateTime
```

#### 2. Venue (Models/Venue.cs:1)
Represents a location where events are held.
```csharp
- Id: int
- Name: string
- Location: string
- TotalCapacity: int (constraint: must be > 0)
- CreatedAt: DateTime
```

#### 3. Event (Models/Event.cs:1)
Represents an event with seating type logic.
```csharp
- Id: int
- Name: string
- Description: string
- VenueId: int
- EventDate: DateTime
- EventType: string
- SeatingType: ISeatingType
- CreatedAt: DateTime
```

#### 4. Booking (Models/Booking.cs:1)
Tracks user bookings with payment status.
```csharp
- Id: int
- UserId: int
- EventId: int
- VenueId: int
- NumberOfSeats: int
- SectionIdentifier: string (nullable)
- PaymentStatus: enum (Pending, Paid, Failed, Refunded)
- PaymentId: string
- TotalAmount: decimal
- BookingDate: DateTime
- CreatedAt: DateTime
```

### Event Seating Type Abstraction

The system implements a polymorphic seating type system using the Strategy pattern:

#### Interface: ISeatingType (Models/Seating/ISeatingType.cs:1)
```csharp
string SeatingTypeName { get; }
int GetAvailableCapacity(int totalCapacity, int currentBookings)
bool CanAccommodateBooking(int requestedSeats, int availableCapacity, string sectionIdentifier)
string GetSectionInfo()
```

#### Three Concrete Implementations:

1. **FullReservedSeating** (Models/Seating/FullReservedSeating.cs:1)
   - For fully seated concerts
   - Each booking has a specific seat number
   - Example: Traditional theater or concert hall

2. **SectionReservedSeating** (Models/Seating/SectionReservedSeating.cs:1)
   - For events with multiple sections (e.g., Golden Circle, Balcony)
   - Each section has its own capacity
   - Bookings must specify a section identifier

3. **OpenSeating** (Models/Seating/OpenSeating.cs:1)
   - For open-air festivals or general admission
   - No specific seat assignments
   - Only overall capacity limits apply

## Payment Gateway Integration

### Payment API Contract (Services/Payment/PaymentGateway.cs:1)

**Request:**
```json
POST /payments/
{
  "creditCardNumber": "string"
}
```

**Response:**
```json
{
  "isValid": true|false,
  "paymentId": "string"
}
```

### Implementation

The system provides two payment gateway implementations:

1. **PaymentGateway** (Services/Payment/PaymentGateway.cs:1)
   - Integrates with external payment API via HTTP
   - Uses HttpClient for API communication
   - Handles JSON serialization/deserialization

2. **SimulatedPaymentGateway** (Services/Payment/SimulatedPaymentGateway.cs:1)
   - For testing and development
   - Validates credit card format locally
   - Generates payment IDs without external calls

### Booking Flow with Payment (Services/Booking/BookingService.cs:1)

The `CreateBookingAsync` method implements the following logic:

1. **Validate Input**: Check user, event, and venue existence
2. **Capacity Check**: Verify available seats against venue capacity
3. **Seating Type Validation**: Use polymorphic seating type logic
4. **Payment Processing**: Call payment gateway
5. **Booking Creation**: Only create booking if payment is valid
6. **Failure Handling**: Reject booking if payment fails

```csharp
// Simplified flow
var paymentResponse = await _paymentGateway.ProcessPaymentAsync(paymentRequest);

if (!paymentResponse.IsValid)
{
    return BookingResult.FailureResult("Payment was rejected");
}

// Only reach here if payment succeeds
await _bookingRepository.AddAsync(booking);
```

## Data Access Layer

### Repository Pattern

All data access is abstracted through repository interfaces, allowing for easy switching between implementations:

- `IUserRepository`
- `IVenueRepository`
- `IEventRepository`
- `IBookingRepository`

### Dual Implementation

#### 1. In-Memory Repositories (DataAccess/InMemory/)
- Perfect for development and testing
- Uses Dictionary<int, T> for storage
- Pre-seeded with sample data
- Thread-safe for single-instance scenarios

#### 2. SQL Repositories (DataAccess/Sql/)
- Production-ready SQL Server implementation
- Uses ADO.NET for direct SQL access
- Parameterized queries to prevent SQL injection
- Efficient with proper indexing

### Configuration (appsettings.json:1)

Switch between implementations via configuration:

```json
{
  "DataAccessMode": "InMemory",  // or "SQL"
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BookingSystem;..."
  }
}
```

## Advanced SQL Queries

### Database Schema (Database/schema.sql:1)

The schema includes:
- 4 main tables (Users, Venues, Events, Bookings)
- Foreign key constraints
- Check constraints for data validation
- 10+ indexes for query optimization
- 2 views for common queries

**Key Indexes:**
```sql
-- Composite indexes for complex queries
CREATE INDEX IX_Bookings_VenueId_PaymentStatus ON Bookings(VenueId, PaymentStatus);
CREATE INDEX IX_Bookings_UserId_VenueId ON Bookings(UserId, VenueId);
```

### Advanced Query Methods

#### 1. FindBookingsForPaidUsersAtVenue (DataAccess/Sql/SqlBookingRepository.cs:267)

**Purpose:** Retrieve bookings only for users who have at least one paid booking at the venue.

**SQL Query:**
```sql
SELECT b.*
FROM Bookings b
WHERE b.VenueId = @VenueId
  AND b.UserId IN (
    SELECT DISTINCT UserId
    FROM Bookings
    WHERE VenueId = @VenueId
      AND PaymentStatus = 'Paid'
  )
```

**Use Case:** Marketing campaigns targeting users who have successfully paid for events at a venue.

**API Endpoint:**
```
GET /api/bookings/venue/{venueId}/paid-users
```

#### 2. FindUsersWithoutBookingsInVenue (DataAccess/Sql/SqlBookingRepository.cs:300)

**Purpose:** Retrieve all user IDs who have never booked at a specific venue.

**SQL Query:**
```sql
SELECT u.Id
FROM Users u
WHERE NOT EXISTS (
  SELECT 1
  FROM Bookings b
  WHERE b.UserId = u.Id
    AND b.VenueId = @VenueId
)
```

**Use Case:** Targeted marketing to attract new customers to a venue.

**API Endpoint:**
```
GET /api/bookings/venue/{venueId}/users-without-bookings
```

### Catalog Retrieval Logic (Controllers/EventsController.cs:52)

**Purpose:** Retrieve future events with available seat information.

**Method:** `GetFutureEventsWithAvailability()`

**Returns:**
```json
[
  {
    "eventId": 1,
    "eventName": "Rock Concert 2025",
    "eventDate": "2025-12-15T20:00:00Z",
    "venueName": "Grand Concert Hall",
    "totalCapacity": 5000,
    "bookedSeats": 150,
    "availableSeats": 4850,
    "isAvailable": true
  }
]
```

## API Endpoints

### Bookings

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/bookings` | Get all bookings |
| GET | `/api/bookings/{id}` | Get booking by ID |
| GET | `/api/bookings/user/{userId}` | Get bookings by user |
| GET | `/api/bookings/venue/{venueId}` | Get bookings by venue |
| GET | `/api/bookings/venue/{venueId}/paid-users` | Advanced query 1 |
| GET | `/api/bookings/venue/{venueId}/users-without-bookings` | Advanced query 2 |
| POST | `/api/bookings` | Create new booking |
| PUT | `/api/bookings/{id}` | Update booking |
| DELETE | `/api/bookings/{id}` | Delete booking |

### Events

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/events` | Get all events |
| GET | `/api/events/{id}` | Get event by ID |
| GET | `/api/events/future` | Get future events |
| GET | `/api/events/future-with-availability` | Catalog retrieval |
| POST | `/api/events` | Create new event |
| PUT | `/api/events/{id}` | Update event |
| DELETE | `/api/events/{id}` | Delete event |

### Venues

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/venues` | Get all venues |
| GET | `/api/venues/{id}` | Get venue by ID |
| POST | `/api/venues` | Create new venue |
| PUT | `/api/venues/{id}` | Update venue |
| DELETE | `/api/venues/{id}` | Delete venue |

### Users

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/users` | Get all users |
| GET | `/api/users/{id}` | Get user by ID |
| GET | `/api/users/by-email/{email}` | Get user by email |
| POST | `/api/users` | Create new user |
| PUT | `/api/users/{id}` | Update user |
| DELETE | `/api/users/{id}` | Delete user |

## Configuration

### appsettings.json Options

```json
{
  "DataAccessMode": "InMemory|SQL",
  "PaymentMode": "Simulated|External",
  "ConnectionStrings": {
    "DefaultConnection": "SQL Server connection string"
  },
  "PaymentApiBaseUrl": "External payment API URL"
}
```

### Dependency Injection (Program.cs:42)

The system uses ASP.NET Core's built-in DI container:

```csharp
// Data Access Layer - configurable
if (dataAccessMode == "SQL")
{
    services.AddScoped<IBookingRepository>(provider =>
        new SqlBookingRepository(connectionString));
}
else
{
    services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();
}

// Payment Gateway - configurable
if (paymentMode == "External")
{
    services.AddHttpClient<IPaymentGateway, PaymentGateway>();
}
else
{
    services.AddScoped<IPaymentGateway, SimulatedPaymentGateway>();
}

// Booking Service
services.AddScoped<IBookingService, BookingService>();
```

## Running the Application

### Prerequisites
- .NET 8.0 SDK
- SQL Server (for SQL mode)

### Using In-Memory Mode (Default)

1. Navigate to the project directory:
   ```bash
   cd BookingSystem
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

4. Access Swagger UI:
   ```
   https://localhost:5001/swagger
   ```

### Using SQL Mode

1. Update `appsettings.json`:
   ```json
   {
     "DataAccessMode": "SQL",
     "ConnectionStrings": {
       "DefaultConnection": "Your-Connection-String-Here"
     }
   }
   ```

2. Initialize the database:
   ```bash
   sqlcmd -S localhost -i Database/schema.sql
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

### Testing the API

#### Create a Booking (with Payment)

```bash
POST /api/bookings
Content-Type: application/json

{
  "userId": 1,
  "eventId": 1,
  "numberOfSeats": 2,
  "totalAmount": 150.00,
  "creditCardNumber": "4111111111111111"
}
```

#### Get Future Events with Availability

```bash
GET /api/events/future-with-availability
```

#### Execute Advanced Query

```bash
GET /api/bookings/venue/1/paid-users
```

## Key Design Patterns

1. **Repository Pattern**: Abstracts data access
2. **Strategy Pattern**: Seating type polymorphism
3. **Dependency Injection**: Loose coupling, testability
4. **Factory Pattern**: Seating type deserialization
5. **DTO Pattern**: Request/Response objects

## Business Logic Highlights

### Booking Validation (Services/Booking/BookingService.cs:41)

The `CreateBookingAsync` method validates:
- User exists
- Event exists and is in the future
- Venue exists
- Sufficient capacity (overall and per section)
- Seating type constraints
- Payment validation
- Only creates booking if ALL checks pass

### Capacity Checking

```csharp
var currentBookings = await _bookingRepository.GetBookingCountForEventAsync(eventId);
var availableCapacity = venue.TotalCapacity - currentBookings;

if (eventItem.SeatingType != null)
{
    bool canAccommodate = eventItem.SeatingType.CanAccommodateBooking(
        requestedSeats, availableCapacity, sectionIdentifier);

    if (!canAccommodate)
    {
        return BookingResult.FailureResult("Insufficient capacity");
    }
}
```

## Testing Recommendations

1. **Unit Tests**: Test each service method in isolation
2. **Integration Tests**: Test with both In-Memory and SQL repositories
3. **Payment Tests**: Use SimulatedPaymentGateway for predictable results
4. **Capacity Tests**: Verify booking limits are enforced
5. **Advanced Query Tests**: Verify SQL query correctness

## Future Enhancements

- Add seat selection for FullReservedSeating
- Implement booking cancellation/refunds
- Add email notifications
- Implement authentication/authorization
- Add caching for frequently accessed data
- Implement event search and filtering
- Add reporting and analytics

## License

This is a technical challenge implementation demonstrating various software engineering concepts.
