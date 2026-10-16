-- 1. CREATE DATABASE

CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

-- 2. CREATE TABLES
-- 2.1 Roles Table (Lookup)
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200) NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- 2.2 Users Table
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    RoleId INT NOT NULL,
    ProfilePictureUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId),
    CONSTRAINT CHK_Users_Email CHECK (Email LIKE '%_@__%.__%')
);
GO

-- 2.3 Events Table
CREATE TABLE Events (
    EventId INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    EventDate DATETIME NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    Distance DECIMAL(8,2) NOT NULL,
    EventType NVARCHAR(20) NOT NULL CHECK (EventType IN ('Run', 'Walk', 'Cycle')),
    BannerImageUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Events_Users FOREIGN KEY (OrganiserId) REFERENCES Users(UserId)
);
GO

-- 2.4 Categories Table
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    Description NVARCHAR(200) NULL,
    CategoryType NVARCHAR(20) NOT NULL CHECK (CategoryType IN ('Age', 'Distance')),
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- 2.5 EventCategories Junction Table (Many-to-Many)
CREATE TABLE EventCategories (
    EventCategoryId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL,
    CategoryId INT NOT NULL,
    CONSTRAINT FK_EventCategories_Events FOREIGN KEY (EventId) REFERENCES Events(EventId) ON DELETE CASCADE,
    CONSTRAINT FK_EventCategories_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId) ON DELETE CASCADE,
    CONSTRAINT UQ_EventCategories_EventCategory UNIQUE (EventId, CategoryId)
);
GO

-- 2.6 Enrolments Table
CREATE TABLE Enrolments (
    EnrolmentId INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantId INT NOT NULL,
    EventId INT NOT NULL,
    CategoryId INT NOT NULL,
    EnrolmentDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled', 'Completed')),
    CONSTRAINT FK_Enrolments_Users FOREIGN KEY (ParticipantId) REFERENCES Users(UserId),
    CONSTRAINT FK_Enrolments_Events FOREIGN KEY (EventId) REFERENCES Events(EventId) ON DELETE CASCADE,
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId),
    CONSTRAINT UQ_Enrolments_ParticipantEvent UNIQUE (ParticipantId, EventId)
);
GO

-- 2.7 Results Table
CREATE TABLE Results (
    ResultId INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentId INT NOT NULL,
    FinishTime TIME NOT NULL,
    FinishingPosition INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentId) REFERENCES Enrolments(EnrolmentId) ON DELETE CASCADE,
    CONSTRAINT UQ_Results_Enrolment UNIQUE (EnrolmentId)
);
GO


-- 3. CREATE INDEXES FOR PERFORMANCE

CREATE INDEX IX_Users_RoleId ON Users(RoleId);
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Events_OrganiserId ON Events(OrganiserId);
CREATE INDEX IX_Events_EventDate ON Events(EventDate);
CREATE INDEX IX_EventCategories_EventId ON EventCategories(EventId);
CREATE INDEX IX_EventCategories_CategoryId ON EventCategories(CategoryId);
CREATE INDEX IX_Enrolments_ParticipantId ON Enrolments(ParticipantId);
CREATE INDEX IX_Enrolments_EventId ON Enrolments(EventId);
CREATE INDEX IX_Enrolments_CategoryId ON Enrolments(CategoryId);
CREATE INDEX IX_Enrolments_Status ON Enrolments(Status);
CREATE INDEX IX_Results_EnrolmentId ON Results(EnrolmentId);
GO


-- 4. SEED DATA
-- 4.1 Seed Roles
INSERT INTO Roles (RoleName, Description) VALUES
('Organiser', 'Event organiser who can create and manage events, categories, and results'),
('Participant', 'Event participant who can browse events, enrol, and view results');
GO

-- 4.2 Seed Users (Passwords are hashed - using placeholder hashes for demonstration)
-- Note: In production, passwords would be properly hashed using bcrypt or ASP.NET Core Identity
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, RoleId, ProfilePictureUrl) VALUES
('thabo.mokoena@raceday.co.za', 'hashed_password_1', 'Thabo', 'Mokoena', 1, NULL),
('linda.zulu@raceday.co.za', 'hashed_password_2', 'Linda', 'Zulu', 1, NULL),
('sipho.ndlovu@gmail.com', 'hashed_password_3', 'Sipho', 'Ndlovu', 2, NULL),
('zanele.dlamini@gmail.com', 'hashed_password_4', 'Zanele', 'Dlamini', 2, NULL);
GO

-- 4.3 Seed Categories
INSERT INTO Categories (Name, Description, CategoryType) VALUES
('Under 20', 'Participants aged under 20 years', 'Age'),
('Senior (20-39)', 'Participants aged 20 to 39 years', 'Age'),
('Master (40+)', 'Participants aged 40 years and over', 'Age'),
('10km', '10 kilometre distance category', 'Distance'),
('21km', '21.1 kilometre half-marathon category', 'Distance'),
('42km', '42.2 kilometre marathon category', 'Distance');
GO

-- 4.4 Seed Events
INSERT INTO Events (OrganiserId, Name, Description, EventDate, Location, Distance, EventType, BannerImageUrl) VALUES
(1, 'Soweto Marathon', 'Annual Soweto Marathon through the streets of Soweto', '2026-09-15 06:00:00', 'Soweto, Johannesburg', 42.20, 'Run', NULL),
(1, 'Cape Town Cycle Tour', 'World-famous Cape Town Cycle Tour', '2026-10-20 07:00:00', 'Cape Town', 109.00, 'Cycle', NULL),
(2, 'Two Oceans Marathon', 'Ultra-marathon with stunning coastal views', '2026-11-05 05:30:00', 'Cape Town', 56.00, 'Run', NULL);
GO

-- 4.5 Link Events to Categories (EventCategories)
INSERT INTO EventCategories (EventId, CategoryId) VALUES
-- Soweto Marathon categories
(1, 1), (1, 2), (1, 3), (1, 6),
-- Cape Town Cycle Tour categories
(2, 1), (2, 2), (2, 3),
-- Two Oceans Marathon categories
(3, 1), (3, 2), (3, 3), (3, 5);
GO

-- 4.6 Seed Enrolments
INSERT INTO Enrolments (ParticipantId, EventId, CategoryId, Status) VALUES
(3, 1, 2, 'Confirmed'),
(3, 2, 1, 'Pending'),
(4, 1, 3, 'Confirmed'),
(4, 3, 2, 'Pending');
GO

-- 4.7 Seed Results
INSERT INTO Results (EnrolmentId, FinishTime, FinishingPosition) VALUES
(1, '03:45:12', 47),
(3, '04:12:08', 89);
GO


-- 5. VERIFICATION QUERIES

-- Verify all tables have been created
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';
GO

-- Verify seed data
SELECT * FROM Roles;
SELECT * FROM Users;
SELECT * FROM Categories;
SELECT * FROM Events;
SELECT * FROM EventCategories;
SELECT * FROM Enrolments;
SELECT * FROM Results;
GO

-- ============================================================
-- 6. CLEANUP (Optional - uncomment to drop database)
-- ============================================================
-- USE master;
-- GO
-- DROP DATABASE RaceDayDB;
-- GO