IF DB_ID(N'WinFormsCoursework') IS NULL CREATE DATABASE WinFormsCoursework;
GO
USE WinFormsCoursework;
GO
IF OBJECT_ID('dbo.AppUsers','U') IS NULL
CREATE TABLE dbo.AppUsers(
 UserId INT IDENTITY PRIMARY KEY,
 Username NVARCHAR(50) NOT NULL UNIQUE,
 PasswordHash VARBINARY(32) NOT NULL,
 PasswordSalt VARBINARY(16) NOT NULL,
 DisplayName NVARCHAR(100) NOT NULL,
 CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO
IF OBJECT_ID('dbo.Courses','U') IS NULL
CREATE TABLE dbo.Courses(
 CourseId INT IDENTITY PRIMARY KEY,
 CourseCode NVARCHAR(20) NOT NULL UNIQUE,
 CourseName NVARCHAR(100) NOT NULL,
 IsActive BIT NOT NULL DEFAULT 1
);
GO
IF OBJECT_ID('dbo.Students','U') IS NULL
CREATE TABLE dbo.Students(
 StudentId INT IDENTITY PRIMARY KEY,
 FullName NVARCHAR(100) NOT NULL,
 Phone NVARCHAR(20) NOT NULL,
 BirthDate DATE NOT NULL,
 Gender NVARCHAR(20) NOT NULL,
 CourseId INT NOT NULL REFERENCES dbo.Courses(CourseId),
 ReceiveNews BIT NOT NULL DEFAULT 0,
 CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO
IF NOT EXISTS(SELECT 1 FROM dbo.Courses)
INSERT dbo.Courses(CourseCode,CourseName) VALUES
('CS101',N'Lập trình C# cơ bản'),('DB201',N'Cơ sở dữ liệu SQL Server'),
('NW301',N'Quản trị mạng máy tính'),('SC401',N'An toàn thông tin');
GO
