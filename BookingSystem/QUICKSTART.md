# Quick Start Guide - Booking System API

## Starting the Application

```bash
cd BookingSystem
dotnet restore
dotnet run
```

The API will be available at: `https://localhost:5001`
Swagger UI: `https://localhost:5001/swagger`

## API Testing with curl

### 1. Get All Venues

```bash
curl -X GET "https://localhost:5001/api/venues" -k
```

**Expected Response:**
```json
[
  {
    "Id": 1,
    "Name": "Grand Concert Hall",
    "Location": "123 Music Street, New York",
    "TotalCapacity": 5000
  },
  {
    "Id": 2,
    "Name": "Open Air Festival Grounds",
    "Location": "456 Park Avenue, Los Angeles",
    "TotalCapacity": 20000
  }
]
```

### 2. Get All Users

```bash
curl -X GET "https://localhost:5001/api/users" -k
```

### 3. Get Future Events with Availability (Catalog Retrieval)

```bash
curl -X GET "https://localhost:5001/api/events/future-with-availability" -k
```

**Expected Response:**
```json
[
  {
    "EventId": 1,
    "EventName": "Rock Concert 2025",
    "EventDate": "2025-12-15T20:00:00Z",
    "SeatingTypeName": "Full Reserved Seating",
    "VenueName": "Grand Concert Hall",
    "TotalCapacity": 5000,
    "BookedSeats": 6,
    "AvailableSeats": 4994,
    "IsAvailable": true
  }
]
```

### 4. Create a Booking (with Payment)

```bash
curl -X POST "https://localhost:5001/api/bookings" \
  -H "Content-Type: application/json" \
  -d '{
    "UserId": 1,
    "EventId": 1,
    "NumberOfSeats": 2,
    "CreditCardNumber": "4111111111111111",
    "TotalAmount": 150.00,
    "SectionIdentifier": null
  }' -k
```

**Expected Success Response:**
```json
{
  "Success": true,
  "Message": "Booking created successfully",
  "BookingId": 6,
  "PaymentId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

### 5. Create a Booking with Section (Jazz Night)

```bash
curl -X POST "https://localhost:5001/api/bookings" \
  -H "Content-Type: application/json" \
  -d '{
    "UserId": 2,
    "EventId": 3,
    "NumberOfSeats": 2,
    "CreditCardNumber": "4532111111111111",
    "TotalAmount": 200.00,
    "SectionIdentifier": "GoldenCircle"
  }' -k
```

### 6. Test Payment Rejection (Invalid Card)

```bash
curl -X POST "https://localhost:5001/api/bookings" \
  -H "Content-Type: application/json" \
  -d '{
    "UserId": 1,
    "EventId": 1,
    "NumberOfSeats": 2,
    "CreditCardNumber": "0000",
    "TotalAmount": 150.00,
    "SectionIdentifier": null
  }' -k
```

**Expected Failure Response:**
```json
{
  "Success": false,
  "Message": "Payment was rejected. Please check your payment information",
  "BookingId": null,
  "PaymentId": null
}
```

### 7. Get Bookings by User

```bash
curl -X GET "https://localhost:5001/api/bookings/user/1" -k
```

### 8. Get Bookings by Venue

```bash
curl -X GET "https://localhost:5001/api/bookings/venue/1" -k
```

### 9. ADVANCED QUERY 1 - Get Bookings for Paid Users at Venue

```bash
curl -X GET "https://localhost:5001/api/bookings/venue/1/paid-users" -k
```

**Purpose:** Returns only bookings for users who have at least one paid booking at the venue.

**Expected Response:**
```json
[
  {
    "Id": 1,
    "UserId": 1,
    "EventId": 1,
    "VenueId": 1,
    "NumberOfSeats": 2,
    "PaymentStatus": "Paid",
    "PaymentId": "PAY-123456",
    "TotalAmount": 150.00
  },
  {
    "Id": 2,
    "UserId": 2,
    "EventId": 1,
    "VenueId": 1,
    "NumberOfSeats": 4,
    "PaymentStatus": "Paid",
    "PaymentId": "PAY-123457",
    "TotalAmount": 300.00
  }
]
```

### 10. ADVANCED QUERY 2 - Get Users Without Bookings in Venue

```bash
curl -X GET "https://localhost:5001/api/bookings/venue/1/users-without-bookings" -k
```

**Purpose:** Returns user IDs who have never booked at the specified venue.

**Expected Response:**
```json
[3, 4, 5]
```

### 11. Create a New Venue

```bash
curl -X POST "https://localhost:5001/api/venues" \
  -H "Content-Type: application/json" \
  -d '{
    "Name": "Downtown Arena",
    "Location": "100 Main Street, Chicago",
    "TotalCapacity": 15000
  }' -k
```

### 12. Create a New User

```bash
curl -X POST "https://localhost:5001/api/users" \
  -H "Content-Type: application/json" \
  -d '{
    "FirstName": "Alice",
    "LastName": "Cooper",
    "Email": "alice.cooper@example.com"
  }' -k
```

### 13. Get User by Email

```bash
curl -X GET "https://localhost:5001/api/users/by-email/john.doe@example.com" -k
```

### 14. Get Specific Booking

```bash
curl -X GET "https://localhost:5001/api/bookings/1" -k
```

## Testing Different Seating Types

### Full Reserved Seating (Event ID: 1 - Rock Concert)
- No section identifier needed
- Each seat is individually reserved

```bash
curl -X POST "https://localhost:5001/api/bookings" \
  -H "Content-Type: application/json" \
  -d '{
    "UserId": 1,
    "EventId": 1,
    "NumberOfSeats": 3,
    "CreditCardNumber": "4111111111111111",
    "TotalAmount": 225.00
  }' -k
```

### Section Reserved Seating (Event ID: 3 - Jazz Night)
- Must specify section identifier
- Available sections: "GoldenCircle", "Balcony"

```bash
curl -X POST "https://localhost:5001/api/bookings" \
  -H "Content-Type: application/json" \
  -d '{
    "UserId": 2,
    "EventId": 3,
    "NumberOfSeats": 2,
    "CreditCardNumber": "4532111111111111",
    "TotalAmount": 200.00,
    "SectionIdentifier": "Balcony"
  }' -k
```

### Open Seating (Event ID: 2 - Summer Music Festival)
- No section identifier
- General admission only

```bash
curl -X POST "https://localhost:5001/api/bookings" \
  -H "Content-Type: application/json" \
  -d '{
    "UserId": 3,
    "EventId": 2,
    "NumberOfSeats": 5,
    "CreditCardNumber": "5555555555554444",
    "TotalAmount": 250.00
  }' -k
```

## Testing Validation & Error Cases

### 1. Insufficient Capacity
Try to book more seats than available:

```bash
curl -X POST "https://localhost:5001/api/bookings" \
  -H "Content-Type: application/json" \
  -d '{
    "UserId": 1,
    "EventId": 3,
    "NumberOfSeats": 500,
    "CreditCardNumber": "4111111111111111",
    "TotalAmount": 5000.00,
    "SectionIdentifier": "GoldenCircle"
  }' -k
```

**Expected:** Failure due to capacity

### 2. Invalid Section Identifier
Try to book with wrong section:

```bash
curl -X POST "https://localhost:5001/api/bookings" \
  -H "Content-Type: application/json" \
  -d '{
    "UserId": 1,
    "EventId": 3,
    "NumberOfSeats": 2,
    "CreditCardNumber": "4111111111111111",
    "TotalAmount": 200.00,
    "SectionIdentifier": "InvalidSection"
  }' -k
```

**Expected:** Failure due to invalid section

### 3. Missing Section for Section Reserved Event
Try to book section reserved event without section:

```bash
curl -X POST "https://localhost:5001/api/bookings" \
  -H "Content-Type: application/json" \
  -d '{
    "UserId": 1,
    "EventId": 3,
    "NumberOfSeats": 2,
    "CreditCardNumber": "4111111111111111",
    "TotalAmount": 200.00
  }' -k
```

**Expected:** Failure due to missing section

### 4. Non-Existent User

```bash
curl -X POST "https://localhost:5001/api/bookings" \
  -H "Content-Type: application/json" \
  -d '{
    "UserId": 999,
    "EventId": 1,
    "NumberOfSeats": 2,
    "CreditCardNumber": "4111111111111111",
    "TotalAmount": 150.00
  }' -k
```

**Expected:** "User with ID 999 not found"

### 5. Non-Existent Event

```bash
curl -X POST "https://localhost:5001/api/bookings" \
  -H "Content-Type: application/json" \
  -d '{
    "UserId": 1,
    "EventId": 999,
    "NumberOfSeats": 2,
    "CreditCardNumber": "4111111111111111",
    "TotalAmount": 150.00
  }' -k
```

**Expected:** "Event with ID 999 not found"

## Valid Test Credit Card Numbers

For the simulated payment gateway:
- `4111111111111111` (Visa)
- `4532111111111111` (Visa)
- `5555555555554444` (MasterCard)
- Any 13-19 digit number not starting with "0000"

Invalid card numbers:
- `0000` (too short)
- `0000000000000000` (starts with 0000)
- `12345` (too short)

## PowerShell Alternative (Windows)

If using PowerShell, use `Invoke-RestMethod`:

```powershell
# Get venues
Invoke-RestMethod -Uri "https://localhost:5001/api/venues" -Method Get -SkipCertificateCheck

# Create booking
$body = @{
    UserId = 1
    EventId = 1
    NumberOfSeats = 2
    CreditCardNumber = "4111111111111111"
    TotalAmount = 150.00
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/bookings" -Method Post -Body $body -ContentType "application/json" -SkipCertificateCheck
```

## Using Postman

1. Import the base URL: `https://localhost:5001`
2. Disable SSL certificate verification in Settings
3. Use the endpoints listed above
4. Set Content-Type header to `application/json` for POST/PUT requests

## Switching to SQL Mode

1. Update `appsettings.json`:
```json
{
  "DataAccessMode": "SQL",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BookingSystem;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

2. Initialize database:
```bash
sqlcmd -S localhost -i Database/schema.sql
```

3. Restart the application

All API endpoints work identically with SQL mode.

## Troubleshooting

### Port Already in Use
Change the port in `Properties/launchSettings.json` or use:
```bash
dotnet run --urls "https://localhost:5002"
```

### SSL Certificate Issues
Add `-k` flag to curl commands or use `--SkipCertificateCheck` in PowerShell

### Database Connection Issues
- Verify SQL Server is running
- Check connection string in appsettings.json
- Ensure database exists and schema is applied

## Next Steps

1. Explore Swagger UI for interactive API testing
2. Review the README.md for architecture details
3. Check Examples/BookingExamples.cs for programmatic usage
4. Examine Database/schema.sql for database design
