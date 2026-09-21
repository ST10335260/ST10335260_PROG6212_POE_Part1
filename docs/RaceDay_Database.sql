USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'RaceDay')
BEGIN
    ALTER DATABASE RaceDay SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDay;
END
GO

CREATE DATABASE RaceDay;
GO

USE RaceDay;
GO

/* TABLE: Roles */
CREATE TABLE Roles (
    RoleID      INT IDENTITY(1,1) PRIMARY KEY,
    RoleName    NVARCHAR(50)  NOT NULL UNIQUE,
    Description NVARCHAR(255) NULL
);
GO

/* TABLE: Users */
CREATE TABLE Users (
    UserID          INT IDENTITY(1,1) PRIMARY KEY,
    Email           NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(255) NOT NULL,
    FirstName       NVARCHAR(100) NOT NULL,
    LastName        NVARCHAR(100) NOT NULL,
    DateOfBirth     DATE          NOT NULL,
    RoleID          INT           NOT NULL,
    CreatedAt       DATETIME2     DEFAULT GETUTCDATE(),
    IsActive        BIT           DEFAULT 1,
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);
GO

/* TABLE: Events */
CREATE TABLE Events (
    EventID         INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID     INT           NOT NULL,
    EventName       NVARCHAR(150) NOT NULL,
    Description     NVARCHAR(MAX) NULL,
    EventDate       DATE          NOT NULL,
    Location        NVARCHAR(150) NOT NULL,
    DistanceKm      DECIMAL(6,2)  NULL,
    IsActive        BIT           DEFAULT 1,
    CreatedAt       DATETIME2     DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Events_Users FOREIGN KEY (OrganiserID) REFERENCES Users(UserID)
);
GO

/* TABLE: Categories */
CREATE TABLE Categories (
    CategoryID      INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName    NVARCHAR(100) NOT NULL,
    Description     NVARCHAR(255) NULL,
    DefaultDistance DECIMAL(6,2)  NULL,
    CreatedAt       DATETIME2     DEFAULT GETUTCDATE()
);
GO

/* TABLE: EventCategories */
CREATE TABLE EventCategories (
    EventCategoryID     INT IDENTITY(1,1) PRIMARY KEY,
    EventID             INT           NOT NULL,
    CategoryID          INT           NOT NULL,
    EntryFee            DECIMAL(8,2)  NOT NULL DEFAULT 0,
    MaxParticipants     INT           NOT NULL DEFAULT 100,
    CurrentParticipants INT           NOT NULL DEFAULT 0,
    CreatedAt           DATETIME2     DEFAULT GETUTCDATE(),
    CONSTRAINT FK_EventCategories_Events FOREIGN KEY (EventID) REFERENCES Events(EventID),
    CONSTRAINT FK_EventCategories_Categories FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);
GO

/* TABLE: Enrolments */
CREATE TABLE Enrolments (
    EnrolmentID     INT IDENTITY(1,1) PRIMARY KEY,
    EventID         INT           NOT NULL,
    ParticipantID   INT           NOT NULL,
    EventCategoryID INT           NOT NULL,
    EnrolmentDate   DATETIME2     DEFAULT GETUTCDATE(),
    Status          NVARCHAR(50)  NOT NULL DEFAULT 'Pending',
    PaymentStatus   NVARCHAR(50)  NOT NULL DEFAULT 'Pending',
    BibNumber       INT           NULL,
    CONSTRAINT FK_Enrolments_Events FOREIGN KEY (EventID) REFERENCES Events(EventID),
    CONSTRAINT FK_Enrolments_Users FOREIGN KEY (ParticipantID) REFERENCES Users(UserID),
    CONSTRAINT FK_Enrolments_EventCategories FOREIGN KEY (EventCategoryID) REFERENCES EventCategories(EventCategoryID),
    CONSTRAINT UQ_Enrolments_Participant_Category UNIQUE (ParticipantID, EventCategoryID)
);
GO

/* TABLE: Results */
CREATE TABLE Results (
    ResultID        INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID     INT           NOT NULL UNIQUE,
    FinishTime      TIME          NOT NULL,
    Position        INT           NULL,
    OverallRank     INT           NULL,
    CategoryRank    INT           NULL,
    PacePerKm       DECIMAL(6,2)  NULL,
    Verified        BIT           DEFAULT 0,
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentID) REFERENCES Enrolments(EnrolmentID)
);
GO

/* TABLE: WeatherInfo */
CREATE TABLE WeatherInfo (
    WeatherID       INT IDENTITY(1,1) PRIMARY KEY,
    EventID         INT           NOT NULL,
    ForecastDate    DATE          NOT NULL,
    Temperature     DECIMAL(5,2)  NULL,
    Conditions      NVARCHAR(100) NULL,
    WindSpeed       DECIMAL(5,2)  NULL,
    CONSTRAINT FK_WeatherInfo_Events FOREIGN KEY (EventID) REFERENCES Events(EventID)
);
GO

/* SEED DATA */

-- Roles
INSERT INTO Roles (RoleName, Description) VALUES
('Organiser',   'Creates and manages events, categories and results.'),
('Participant', 'Browses events, enrols in categories and views results.');
GO

-- Users: 2 Organisers, 2 Participants (minimum required)
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, DateOfBirth, RoleID) VALUES
('sipho.ndlovu@raceday.co.za',  'HASHED_PASSWORD_1', 'Sipho',  'Ndlovu',  '1985-03-14', 1),
('amanda.pillay@raceday.co.za', 'HASHED_PASSWORD_2', 'Amanda', 'Pillay',  '1990-07-22', 1),
('thabo.mokoena@example.com',   'HASHED_PASSWORD_3', 'Thabo',  'Mokoena', '1996-11-02', 2),
('lerato.dube@example.com',     'HASHED_PASSWORD_4', 'Lerato', 'Dube',    '1998-05-30', 2);
GO

-- Events: 3 events (minimum required), one run by each organiser
INSERT INTO Events (OrganiserID, EventName, Description, EventDate, Location, DistanceKm) VALUES
(1, 'Durban Beachfront Marathon', 'Annual road race along the Durban beachfront.', '2026-11-14', 'Durban, KZN',        42.2),
(1, 'Midlands Trail Run',         'Off-road trail event through the KZN Midlands.', '2026-09-05', 'Howick, KZN',        15.0),
(2, 'Joburg City Fun Run',        'Family-friendly fun run through the Joburg CBD.', '2026-10-10', 'Johannesburg, GP',    5.0);
GO

-- Categories: a reusable master list of race distances
INSERT INTO Categories (CategoryName, Description, DefaultDistance) VALUES
('5km Fun Run',    'Short, family-friendly distance.', 5.0),
('10km',           'Standard road running distance.', 10.0),
('15km Trail',     'Off-road trail distance.', 15.0),
('Half Marathon',  'Half marathon distance.', 21.1),
('Full Marathon',  'Full marathon distance.', 42.2);
GO

-- EventCategories: which categories are offered at which event
INSERT INTO EventCategories (EventID, CategoryID, EntryFee, MaxParticipants) VALUES
(1, 2, 150.00, 500),  -- Durban Marathon: 10km
(1, 5, 300.00, 300),  -- Durban Marathon: Full Marathon
(2, 3, 180.00, 150),  -- Midlands Trail Run: 15km Trail
(3, 1,  80.00, 400);  -- Joburg Fun Run: 5km
GO