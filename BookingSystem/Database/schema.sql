-- BookingSystem Database Schema
-- This schema supports venue management, event booking with different seating types, and payment tracking

-- Users Table
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT CK_Users_Email CHECK (Email LIKE '%_@__%.__%')
);

-- Venues Table
CREATE TABLE Venues (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Location NVARCHAR(500) NOT NULL,
    TotalCapacity INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT CK_Venues_Capacity CHECK (TotalCapacity > 0)
);

-- Events Table with Seating Type Support
CREATE TABLE Events (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    VenueId INT NOT NULL,
    EventDate DATETIME2 NOT NULL,
    EventType NVARCHAR(100) NOT NULL,
    SeatingTypeName NVARCHAR(50) NOT NULL, -- 'FullReserved', 'SectionReserved', 'Open'
    SeatingConfiguration NVARCHAR(MAX), -- JSON for section details if applicable
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Events_Venues FOREIGN KEY (VenueId) REFERENCES Venues(Id),
    CONSTRAINT CK_Events_SeatingType CHECK (SeatingTypeName IN ('FullReserved', 'SectionReserved', 'Open'))
);

-- Bookings Table with Payment Status
CREATE TABLE Bookings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    EventId INT NOT NULL,
    VenueId INT NOT NULL,
    NumberOfSeats INT NOT NULL,
    SectionIdentifier NVARCHAR(100), -- NULL for open seating or full reserved
    PaymentStatus NVARCHAR(20) NOT NULL, -- 'Pending', 'Paid', 'Failed', 'Refunded'
    PaymentId NVARCHAR(100),
    TotalAmount DECIMAL(10, 2) NOT NULL,
    BookingDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Bookings_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_Bookings_Events FOREIGN KEY (EventId) REFERENCES Events(Id),
    CONSTRAINT FK_Bookings_Venues FOREIGN KEY (VenueId) REFERENCES Venues(Id),
    CONSTRAINT CK_Bookings_Seats CHECK (NumberOfSeats > 0),
    CONSTRAINT CK_Bookings_Amount CHECK (TotalAmount >= 0),
    CONSTRAINT CK_Bookings_PaymentStatus CHECK (PaymentStatus IN ('Pending', 'Paid', 'Failed', 'Refunded'))
);

-- Indexes for Performance Optimization

-- User lookups
CREATE INDEX IX_Users_Email ON Users(Email);

-- Event queries
CREATE INDEX IX_Events_VenueId ON Events(VenueId);
CREATE INDEX IX_Events_EventDate ON Events(EventDate);
CREATE INDEX IX_Events_VenueId_EventDate ON Events(VenueId, EventDate);

-- Booking queries - most frequently used
CREATE INDEX IX_Bookings_UserId ON Bookings(UserId);
CREATE INDEX IX_Bookings_EventId ON Bookings(EventId);
CREATE INDEX IX_Bookings_VenueId ON Bookings(VenueId);
CREATE INDEX IX_Bookings_PaymentStatus ON Bookings(PaymentStatus);

-- Composite indexes for advanced queries
CREATE INDEX IX_Bookings_VenueId_PaymentStatus ON Bookings(VenueId, PaymentStatus);
CREATE INDEX IX_Bookings_EventId_SectionIdentifier ON Bookings(EventId, SectionIdentifier);
CREATE INDEX IX_Bookings_UserId_VenueId ON Bookings(UserId, VenueId);

-- Sample Data for Testing

-- Insert sample users
INSERT INTO Users (FirstName, LastName, Email) VALUES
('John', 'Doe', 'john.doe@example.com'),
('Jane', 'Smith', 'jane.smith@example.com'),
('Robert', 'Johnson', 'robert.johnson@example.com'),
('Emily', 'Williams', 'emily.williams@example.com'),
('Michael', 'Brown', 'michael.brown@example.com');

-- Insert sample venues
INSERT INTO Venues (Name, Location, TotalCapacity) VALUES
('Grand Concert Hall', '123 Music Street, New York', 5000),
('Open Air Festival Grounds', '456 Park Avenue, Los Angeles', 20000),
('Jazz Club Downtown', '789 Blues Road, Chicago', 300);

-- Insert sample events
INSERT INTO Events (Name, Description, VenueId, EventDate, EventType, SeatingTypeName, SeatingConfiguration) VALUES
('Rock Concert 2025', 'Amazing rock concert', 1, '2025-12-15 20:00:00', 'Concert', 'FullReserved', '{"TotalSeats": 5000}'),
('Summer Music Festival', 'All-day music festival', 2, '2025-08-20 12:00:00', 'Festival', 'Open', NULL),
('Jazz Night', 'Evening jazz performance', 3, '2025-07-10 19:00:00', 'Concert', 'SectionReserved', '{"Sections": {"GoldenCircle": 100, "Balcony": 200}}');

-- Insert sample bookings
INSERT INTO Bookings (UserId, EventId, VenueId, NumberOfSeats, SectionIdentifier, PaymentStatus, PaymentId, TotalAmount) VALUES
(1, 1, 1, 2, NULL, 'Paid', 'PAY-123456', 150.00),
(2, 1, 1, 4, NULL, 'Paid', 'PAY-123457', 300.00),
(3, 2, 2, 5, NULL, 'Paid', 'PAY-123458', 250.00),
(1, 3, 3, 2, 'GoldenCircle', 'Paid', 'PAY-123459', 200.00),
(4, 3, 3, 3, 'Balcony', 'Pending', NULL, 120.00);

-- Views for Common Queries

-- View: Future Events with Available Seats
CREATE VIEW FutureEventsWithAvailability AS
SELECT
    e.Id AS EventId,
    e.Name AS EventName,
    e.Description,
    e.EventDate,
    e.EventType,
    e.SeatingTypeName,
    v.Id AS VenueId,
    v.Name AS VenueName,
    v.Location AS VenueLocation,
    v.TotalCapacity,
    COALESCE(SUM(b.NumberOfSeats), 0) AS BookedSeats,
    v.TotalCapacity - COALESCE(SUM(b.NumberOfSeats), 0) AS AvailableSeats
FROM Events e
INNER JOIN Venues v ON e.VenueId = v.Id
LEFT JOIN Bookings b ON e.Id = b.EventId
WHERE e.EventDate > GETUTCDATE()
GROUP BY e.Id, e.Name, e.Description, e.EventDate, e.EventType, e.SeatingTypeName,
         v.Id, v.Name, v.Location, v.TotalCapacity;

-- View: User Booking Summary
CREATE VIEW UserBookingSummary AS
SELECT
    u.Id AS UserId,
    u.FirstName,
    u.LastName,
    u.Email,
    COUNT(b.Id) AS TotalBookings,
    SUM(CASE WHEN b.PaymentStatus = 'Paid' THEN 1 ELSE 0 END) AS PaidBookings,
    SUM(CASE WHEN b.PaymentStatus = 'Paid' THEN b.TotalAmount ELSE 0 END) AS TotalSpent
FROM Users u
LEFT JOIN Bookings b ON u.Id = b.UserId
GROUP BY u.Id, u.FirstName, u.LastName, u.Email;

-- Comments for Documentation
EXEC sp_addextendedproperty
    @name = N'MS_Description',
    @value = N'Main users table storing customer information',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'Users';

EXEC sp_addextendedproperty
    @name = N'MS_Description',
    @value = N'Venues where events are held with capacity constraints',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'Venues';

EXEC sp_addextendedproperty
    @name = N'MS_Description',
    @value = N'Events with different seating types (FullReserved, SectionReserved, Open)',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'Events';

EXEC sp_addextendedproperty
    @name = N'MS_Description',
    @value = N'Bookings with payment tracking and section assignments',
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE',  @level1name = N'Bookings';
