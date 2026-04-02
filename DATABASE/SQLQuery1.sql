 CREATE DATABASE RagnorGymDB;
GO

CREATE TABLE Admins
(
    AdminId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL,
    Password NVARCHAR(50) NOT NULL
);
GO

select * from Admins

INSERT INTO Admins (Username, Password)
VALUES ('admin', 'admin123');
GO
SELECT COUNT(*) FROM Admins WHERE Username=@u AND Password=@p

CREATE TABLE Users
(
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100),
    Email NVARCHAR(100),
    Password NVARCHAR(100),
    CreatedAt DATETIME DEFAULT GETDATE()
);

SELECT * FROM Users

INSERT INTO Users (FullName, Email, Password)
VALUES 
('Aman Kumar', 'aman@gmail.com', '123'),
('Rahul Sharma', 'rahul@gmail.com', '123');


CREATE TABLE MembershipPlans
(
    PlanId INT IDENTITY(1,1) PRIMARY KEY,
    PlanName NVARCHAR(100),
    Price DECIMAL(10,2),
    Duration NVARCHAR(50),
    Features NVARCHAR(MAX),
    CreatedAt DATETIME DEFAULT GETDATE()
);


	

CREATE TABLE UserMemberships
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT,
    PlanId INT,
    StartDate DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (PlanId) REFERENCES MembershipPlans(PlanId)
);


ALTER TABLE UserMemberships
ADD ExpiryDate DATETIME

DELETE FROM UserMemberships
WHERE Id NOT IN
(
    SELECT MIN(Id)
    FROM UserMemberships
    GROUP BY UserId, PlanId
);

SELECT * FROM UserMemberships

DELETE FROM UserMemberships;
SELECT * FROM MembershipPlans

DELETE FROM MembershipPlans;
DBCC CHECKIDENT ('MembershipPlans', RESEED, 0);

SELECT PlanId, PlanName FROM MembershipPlans

UPDATE MembershipPlans
SET Duration = '4'
WHERE PlanName = 'Physical and Natural'

ALTER TABLE MembershipPlans
ALTER COLUMN Duration INT;



ALTER TABLE MembershipPlans
ADD CONSTRAINT UQ_Plan UNIQUE (PlanName, Price, Duration);

select * from MembershipPlans

ALTER TABLE UserMemberships
ADD CONSTRAINT UQ_UserPlan UNIQUE (UserId, PlanId);

CREATE TABLE TrialBookings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100),
    PhoneNumber NVARCHAR(20),
    PreferredDate DATE,
    Message NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE()
);

select * from TrialBookings

ALTER TABLE TrialBookings
ADD STATUS NVARCHAR(20) DEFAULT 'Pending';

sp_help TrialBookings

ALTER TABLE TrialBookings
ADD CONSTRAINT PK_TrialBookings PRIMARY KEY (Id);

UPDATE TrialBookings
SET Status='Pending'
WHERE Id=1


CREATE TABLE TrainerAppointments
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100),
    PhoneNumber NVARCHAR(20),
    TrainerName NVARCHAR(100),
    AppointmentDate DATE,
    AppointmentTime NVARCHAR(20),
    Message NVARCHAR(300),
    Status NVARCHAR(50) DEFAULT 'Pending',
    CreatedAt DATETIME DEFAULT GETDATE()
)

select * from TrainerAppointments


ALTER TABLE UserMemberships
DROP CONSTRAINT FK_UserMemberships_PlanId;

select * from UserMemberships

ALTER TABLE UserMemberships
ADD CONSTRAINT FK_UserMemberships_PlanId
FOREIGN KEY (PlanId)
REFERENCES MembershipPlans(PlanId)
ON DELETE CASCADE;


CREATE TABLE Contacts (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100),
    Email NVARCHAR(100),
	
    Comment NVARCHAR(MAX),
    CreatedAt DATETIME
);

ALTER TABLE Contacts
ADD Status NVARCHAR(20) DEFAULT 'New'
	

select Status from Contacts


drop table Contacts

ALTER TABLE Contacts
ADD CONSTRAINT DF_Status DEFAULT 'New' FOR Status

ALTER TABLE Contacts ADD Status NVARCHAR(20)

