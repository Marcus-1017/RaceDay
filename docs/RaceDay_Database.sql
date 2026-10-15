CREATE DATABASE RaceDayDB;
GO
USE RaceDayDB;
GO
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(150) NOT NULL UNIQUE,
    PasswordHash VARCHAR(100) NOT NULL,
    Role VARCHAR(20) NOT NULL
);
CREATE TABLE Events (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    EventName VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    EventDate DATE NOT NULL,
    Location VARCHAR(150) NOT NULL,
    Distance DECIMAL(5,2),
    EventType VARCHAR(20) NOT NULL,
    OrganiserID INT NOT NULL,
    FOREIGN KEY (OrganiserID)
    REFERENCES Users(UserID)
);
CREATE TABLE Category (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName VARCHAR(50) NOT NULL,
    EventID INT NOT NULL,
    FOREIGN KEY (EventID)
    REFERENCES Events(EventID)
);
CREATE TABLE Enrolment (
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentStatus VARCHAR(20) NOT NULL DEFAULT 'Pending',
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    FOREIGN KEY (ParticipantID) REFERENCES Users(UserID),
    FOREIGN KEY (EventID) REFERENCES Events(EventID),
    FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID)
);
CREATE TABLE Result (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    FinishTime VARCHAR(20),
    FinishPosition INT,
    EnrolmentID INT NOT NULL,
    FOREIGN KEY (EnrolmentID) REFERENCES Enrolment(EnrolmentID)
);
CREATE TABLE EventImage (
    ImageID INT IDENTITY(1,1) PRIMARY KEY,
    ImageURL VARCHAR(150) NOT NULL,
    EventID INT NOT NULL,
    FOREIGN KEY (EventID) REFERENCES Events(EventID)
);

CREATE TABLE ProfileImage (
    ImageID INT IDENTITY(1,1) PRIMARY KEY,
    ImageURL VARCHAR(150) NOT NULL,
    UserID INT NOT NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
INSERT INTO Users (FullName, Email, PasswordHash, Role) VALUES
('Yinlin', 'yinlin@seeddata.com', 'akjfd*$HUNF388f', 'Participant'),
('Camellya', 'camellya@gmail.com', 'yHRF$348NJREREj', 'Participant'),
('Jinhsi', 'jinhsi@seedata.com', '584HG4h84nj39r#@', 'Organiser'),
('Changli', 'changli@wuwa.com', '%&*$nfhdk48&$fn', 'Organiser');

INSERT INTO Events (EventName, Description, EventDate, Location, Distance, EventType, OrganiserID) VALUES
('Joburg City Run', 'A refreshing run through Joburgs most beautiful area.', '2026-10-10', 'Johannesburg', 10.00, 'Run', 3),
('Durban Beachfront Cycle', 'A Beachfront cycling event in Durban.', '2026-11-15', 'Durban', 21.00, 'Cycle', 3),
('Cape Town Community Walk', 'A get-together with the community in the greenery of Cape Town.', '2026-12-20', 'Cape Town', 5.00, 'Walk', 4);

INSERT INTO Category (CategoryName, EventID) VALUES
('Under 20', 1),
('Open', 1),
('Masters 40+', 1),
('21km Social', 2),
('21km Race', 2),
('Family Walk', 3),
('Senior Walk', 3);

INSERT INTO Enrolment (EnrolmentStatus, ParticipantID, EventID, CategoryID) VALUES
('Confirmed', 1, 1, 2),
('Confirmed', 2, 1, 1),
('Pending', 2, 2, 5),
('Confirmed', 1, 3, 6);

INSERT INTO Result (FinishTime, FinishPosition, EnrolmentID) VALUES
('00:52:41', 8, 1),
('00:47:15', 3, 2);
