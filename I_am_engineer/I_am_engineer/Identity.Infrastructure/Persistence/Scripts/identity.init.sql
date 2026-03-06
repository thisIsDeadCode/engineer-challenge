/*
  Re-runnable MSSQL init script for identity endpoints.
  Recreates objects from scratch on every run.
*/

IF OBJECT_ID(N'dbo.PasswordResets', N'U') IS NOT NULL
    DROP TABLE dbo.PasswordResets;
GO

IF OBJECT_ID(N'dbo.UserSessions', N'U') IS NOT NULL
    DROP TABLE dbo.UserSessions;
GO

IF OBJECT_ID(N'dbo.UserLockoutPolicies', N'U') IS NOT NULL
    DROP TABLE dbo.UserLockoutPolicies;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
    DROP TABLE dbo.Users;
GO

CREATE TABLE dbo.Users
(
    UserId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Email NVARCHAR(320) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(512) NOT NULL,
    DisplayName NVARCHAR(128) NOT NULL DEFAULT N'User',
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE dbo.UserLockoutPolicies
(
    UserId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    MaxFailedAttempts INT NOT NULL,
    LockoutDurationMinutes INT NOT NULL,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_UserLockoutPolicies_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_UserLockoutPolicies_MaxFailedAttempts CHECK (MaxFailedAttempts > 0),
    CONSTRAINT CK_UserLockoutPolicies_LockoutDurationMinutes CHECK (LockoutDurationMinutes > 0)
);
GO

CREATE TABLE dbo.UserSessions
(
    SessionId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    RefreshToken NVARCHAR(512) NOT NULL UNIQUE,
    RefreshTokenExpiresAt DATETIMEOFFSET NOT NULL,
    DeviceId NVARCHAR(128) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_UserSessions_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
);
GO

CREATE TABLE dbo.PasswordResets
(
    PasswordResetId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    ResetToken NVARCHAR(512) NOT NULL UNIQUE,
    ExpiresAt DATETIMEOFFSET NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_PasswordResets_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
);
GO

CREATE OR ALTER PROCEDURE dbo.usp_Identity_CreateUser
    @UserId UNIQUEIDENTIFIER,
    @Email NVARCHAR(320),
    @PasswordHash NVARCHAR(512)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Users (UserId, Email, PasswordHash)
    VALUES (@UserId, @Email, @PasswordHash);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Identity_GetUserCredentialsByEmail
    @Email NVARCHAR(320)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        u.UserId,
        u.Email,
        u.PasswordHash,
        u.IsActive,
        ISNULL(ulp.MaxFailedAttempts, 5) AS MaxFailedAttempts,
        ISNULL(ulp.LockoutDurationMinutes, 15) AS LockoutDurationMinutes
    FROM dbo.Users u
    LEFT JOIN dbo.UserLockoutPolicies ulp ON ulp.UserId = u.UserId
    WHERE u.Email = @Email;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Identity_CreateSession
    @SessionId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @RefreshToken NVARCHAR(512),
    @RefreshTokenExpiresAt DATETIMEOFFSET,
    @DeviceId NVARCHAR(128) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.UserSessions (SessionId, UserId, RefreshToken, RefreshTokenExpiresAt, DeviceId)
    VALUES (@SessionId, @UserId, @RefreshToken, @RefreshTokenExpiresAt, @DeviceId);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Identity_RefreshSession
    @RefreshToken NVARCHAR(512),
    @NextRefreshToken NVARCHAR(512),
    @NextRefreshTokenExpiresAt DATETIMEOFFSET
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SessionId UNIQUEIDENTIFIER;
    DECLARE @UserId UNIQUEIDENTIFIER;

    SELECT TOP (1)
        @SessionId = s.SessionId,
        @UserId = s.UserId
    FROM dbo.UserSessions s
    WHERE s.RefreshToken = @RefreshToken
      AND s.IsActive = 1
      AND s.RefreshTokenExpiresAt > SYSUTCDATETIME();

    IF @SessionId IS NULL
        RETURN;

    UPDATE dbo.UserSessions
    SET RefreshToken = @NextRefreshToken,
        RefreshTokenExpiresAt = @NextRefreshTokenExpiresAt
    WHERE SessionId = @SessionId;

    SELECT
        SessionId,
        UserId,
        RefreshToken,
        RefreshTokenExpiresAt
    FROM dbo.UserSessions
    WHERE SessionId = @SessionId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Identity_DeactivateSession
    @SessionId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.UserSessions
    SET IsActive = 0
    WHERE SessionId = @SessionId
      AND IsActive = 1;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Identity_CreatePasswordReset
    @Email NVARCHAR(320),
    @ResetToken NVARCHAR(512),
    @ExpiresAt DATETIMEOFFSET
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserId UNIQUEIDENTIFIER;

    SELECT TOP (1) @UserId = u.UserId
    FROM dbo.Users u
    WHERE u.Email = @Email
      AND u.IsActive = 1;

    IF @UserId IS NULL
        RETURN;

    INSERT INTO dbo.PasswordResets (UserId, ResetToken, ExpiresAt)
    VALUES (@UserId, @ResetToken, @ExpiresAt);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Identity_GetPasswordReset
    @ResetToken NVARCHAR(512)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        pr.UserId,
        pr.ResetToken,
        pr.ExpiresAt,
        pr.IsUsed
    FROM dbo.PasswordResets pr
    WHERE pr.ResetToken = @ResetToken;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Identity_ConfirmPasswordReset
    @ResetToken NVARCHAR(512),
    @NewPasswordHash NVARCHAR(512)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserId UNIQUEIDENTIFIER;

    SELECT TOP (1) @UserId = pr.UserId
    FROM dbo.PasswordResets pr
    WHERE pr.ResetToken = @ResetToken
      AND pr.IsUsed = 0
      AND pr.ExpiresAt > SYSUTCDATETIME();

    IF @UserId IS NULL
        RETURN;

    UPDATE dbo.Users
    SET PasswordHash = @NewPasswordHash
    WHERE UserId = @UserId;

    UPDATE dbo.PasswordResets
    SET IsUsed = 1
    WHERE ResetToken = @ResetToken;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Identity_GetMyProfile
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        u.UserId,
        u.Email,
        u.DisplayName
    FROM dbo.Users u
    WHERE u.UserId = @UserId
      AND u.IsActive = 1;
END;
GO
