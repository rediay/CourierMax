-- ============================================================
-- CourierMax - Database Initialization Script
-- SQL Server
-- ============================================================

-- Create Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CourierMax')
BEGIN
    CREATE DATABASE CourierMax;
END
GO

USE CourierMax;
GO

-- ============================================================
-- TABLES
-- ============================================================

-- Drivers
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Drivers]') AND type in (N'U'))
BEGIN
    CREATE TABLE Drivers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Phone NVARCHAR(20) NULL,
        Email NVARCHAR(100) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NULL
    );
END
GO

-- Vehicles (relation 1:1 with Driver)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Vehicles]') AND type in (N'U'))
BEGIN
    CREATE TABLE Vehicles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Plate NVARCHAR(20) NOT NULL,
        DriverId INT NULL,
        MaxWeightKg DECIMAL(10,2) NOT NULL,
        MaxVolumeM3 DECIMAL(10,2) NOT NULL,
        CurrentWeightKg DECIMAL(10,2) NOT NULL DEFAULT 0,
        CurrentVolumeM3 DECIMAL(10,2) NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT UQ_Vehicles_Plate UNIQUE (Plate),
        CONSTRAINT UQ_Vehicles_DriverId UNIQUE (DriverId),
        CONSTRAINT FK_Vehicles_Drivers FOREIGN KEY (DriverId) REFERENCES Drivers(Id)
    );
END
GO

-- City Distances (reference data)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CityDistances]') AND type in (N'U'))
BEGIN
    CREATE TABLE CityDistances (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Origin NVARCHAR(50) NOT NULL,
        Destination NVARCHAR(50) NOT NULL,
        DistanceKm DECIMAL(10,2) NOT NULL,
        DistanceFee DECIMAL(18,2) NOT NULL,
        CONSTRAINT UQ_CityDistances_Origin_Destination UNIQUE (Origin, Destination)
    );
END
GO

-- Shipments
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Shipments]') AND type in (N'U'))
BEGIN
    CREATE TABLE Shipments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TrackingCode NVARCHAR(20) NOT NULL,
        SenderName NVARCHAR(100) NOT NULL,
        SenderPhone NVARCHAR(20) NOT NULL,
        SenderAddress NVARCHAR(200) NOT NULL,
        RecipientName NVARCHAR(100) NOT NULL,
        RecipientPhone NVARCHAR(20) NOT NULL,
        RecipientAddress NVARCHAR(200) NOT NULL,
        PackageWeight DECIMAL(10,2) NOT NULL,
        PackageLength DECIMAL(10,2) NOT NULL,
        PackageWidth DECIMAL(10,2) NOT NULL,
        PackageHeight DECIMAL(10,2) NOT NULL,
        PackageType INT NOT NULL,
        ServiceType INT NOT NULL,
        Origin NVARCHAR(50) NOT NULL,
        Destination NVARCHAR(50) NOT NULL,
        Status INT NOT NULL DEFAULT 0,
        VehicleId INT NULL,
        DriverId INT NULL,
        TotalCost DECIMAL(18,2) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT UQ_Shipments_TrackingCode UNIQUE (TrackingCode),
        CONSTRAINT FK_Shipments_Vehicles FOREIGN KEY (VehicleId) REFERENCES Vehicles(Id),
        CONSTRAINT FK_Shipments_Drivers FOREIGN KEY (DriverId) REFERENCES Drivers(Id),
        CONSTRAINT CK_PackageWeight CHECK (PackageWeight >= 0.1 AND PackageWeight <= 100),
        CONSTRAINT CK_PackageDimensions CHECK (
            PackageLength >= 1 AND PackageLength <= 200
            AND PackageWidth >= 1 AND PackageWidth <= 200
            AND PackageHeight >= 1 AND PackageHeight <= 200
        )
    );
END
GO

-- Shipment Status Histories
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShipmentStatusHistories]') AND type in (N'U'))
BEGIN
    CREATE TABLE ShipmentStatusHistories (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ShipmentId INT NOT NULL,
        PreviousStatus INT NULL,
        NewStatus INT NOT NULL,
        ChangedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        Reason NVARCHAR(500) NULL,
        ChangedBy NVARCHAR(100) NOT NULL,
        CONSTRAINT FK_StatusHistories_Shipments FOREIGN KEY (ShipmentId) REFERENCES Shipments(Id)
    );
END
GO

-- ============================================================
-- SEED DATA
-- ============================================================

-- Drivers
IF NOT EXISTS (SELECT 1 FROM Drivers)
BEGIN
    INSERT INTO Drivers (Name, Phone, Email, IsActive) VALUES
        ('Juan P�rez', '3001234567', 'juan.perez@courier.com', 1),
        ('Mar�a L�pez', '3102345678', 'maria.lopez@courier.com', 1),
        ('Carlos Ruiz', '3203456789', 'carlos.ruiz@courier.com', 1);
END
GO

-- Vehicles
IF NOT EXISTS (SELECT 1 FROM Vehicles)
BEGIN
    INSERT INTO Vehicles (Plate, DriverId, MaxWeightKg, MaxVolumeM3) VALUES
        ('ABC-123', 1, 500, 10),
        ('DEF-456', 2, 300, 6),
        ('GHI-789', 3, 800, 15);
END
GO

-- City Distances
IF NOT EXISTS (SELECT 1 FROM CityDistances)
BEGIN
    INSERT INTO CityDistances (Origin, Destination, DistanceKm, DistanceFee) VALUES
        ('Bogot�', 'Medell�n', 480, 12000),
        ('Bogot�', 'Cali', 360, 9000),
        ('Bogot�', 'Barranquilla', 950, 20000),
        ('Medell�n', 'Cali', 310, 8000),
        ('Medell�n', 'Barranquilla', 650, 15000),
        ('Cali', 'Barranquilla', 900, 18000);
END
GO
