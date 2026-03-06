/*
  Re-runnable MSSQL init script for identity endpoints.
  Recreates objects from scratch on every run.
*/

IF OBJECT_ID(N'dbo.PasswordResets', N'U') IS NOT NULL
    DROP TABLE dbo.PasswordResets;
GO

IF OBJECT_ID(N'dbo.UserOneTimePasswordResetTokens', N'U') IS NOT NULL
    DROP TABLE dbo.UserOneTimePasswordResetTokens;
GO

IF OBJECT_ID(N'dbo.UserLockoutPolicies', N'U') IS NOT NULL
    DROP TABLE dbo.UserLockoutPolicies;
GO

IF OBJECT_ID(N'dbo.UserLockouts', N'U') IS NOT NULL
    DROP TABLE dbo.UserLockouts;
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
    CreatedAtUtc DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE dbo.UserLockouts
(
    UserId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    CurrentFailedAttempts INT NOT NULL DEFAULT 0,
    LockedUntil DATETIMEOFFSET NULL,
    MaxFailedAttempts INT NOT NULL DEFAULT 5,
    LockoutDurationMinutes INT NOT NULL DEFAULT 15,
    CreatedAtUtc DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_UserLockouts_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
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
    CreatedAtUtc DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_UserSessions_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
);
GO

CREATE TABLE dbo.UserOneTimePasswordResetTokens
(
    UserId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    ResetToken NVARCHAR(512) NOT NULL UNIQUE,
    ExpiresAt DATETIMEOFFSET NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    CreatedAtUtc DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc DATETIMEOFFSET NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_UserOneTimePasswordResetTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
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

    INSERT INTO dbo.UserLockouts (UserId)
    VALUES (@UserId);
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
        u.CreatedAtUtc,
        u.UpdatedAtUtc,
        ul.CurrentFailedAttempts,
        ul.LockedUntil,
        ul.MaxFailedAttempts,
        ul.LockoutDurationMinutes,
        ul.CreatedAtUtc AS LockoutCreatedAtUtc,
        ul.UpdatedAtUtc AS LockoutUpdatedAtUtc
    FROM dbo.Users u
    INNER JOIN dbo.UserLockouts ul ON ul.UserId = u.UserId
    WHERE u.Email = @Email;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Identity_UpdateUserLockout
    @UserId UNIQUEIDENTIFIER,
    @CurrentFailedAttempts INT,
    @LockedUntil DATETIMEOFFSET = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.UserLockouts
    SET CurrentFailedAttempts = @CurrentFailedAttempts,
        LockedUntil = @LockedUntil,
        UpdatedAtUtc = SYSUTCDATETIME()
    WHERE UserId = @UserId;
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
        RefreshTokenExpiresAt = @NextRefreshTokenExpiresAt,
        UpdatedAtUtc = SYSUTCDATETIME()
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
    SET IsActive = 0,
        UpdatedAtUtc = SYSUTCDATETIME()
    WHERE SessionId = @SessionId
      AND IsActive = 1;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Identity_GetUserOneTimePasswordResetToken
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        t.UserId,
        t.ResetToken,
        t.ExpiresAt,
        t.IsUsed,
        t.CreatedAtUtc,
        t.UpdatedAtUtc
    FROM dbo.UserOneTimePasswordResetTokens t
    WHERE t.UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Identity_SaveUserOneTimePasswordResetToken
    @UserId UNIQUEIDENTIFIER,
    @ResetToken NVARCHAR(512),
    @ExpiresAt DATETIMEOFFSET
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.UserOneTimePasswordResetTokens WHERE UserId = @UserId)
    BEGIN
        UPDATE dbo.UserOneTimePasswordResetTokens
        SET ResetToken = @ResetToken,
            ExpiresAt = @ExpiresAt,
            IsUsed = 0,
            UpdatedAtUtc = SYSUTCDATETIME()
        WHERE UserId = @UserId;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.UserOneTimePasswordResetTokens (UserId, ResetToken, ExpiresAt)
        VALUES (@UserId, @ResetToken, @ExpiresAt);
    END
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Identity_ClearUserOneTimePasswordResetToken
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.UserOneTimePasswordResetTokens
    WHERE UserId = @UserId;
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

    EXEC dbo.usp_Identity_SaveUserOneTimePasswordResetToken
        @UserId = @UserId,
        @ResetToken = @ResetToken,
        @ExpiresAt = @ExpiresAt;
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
    FROM dbo.UserOneTimePasswordResetTokens pr
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
    FROM dbo.UserOneTimePasswordResetTokens pr
    WHERE pr.ResetToken = @ResetToken
      AND pr.IsUsed = 0
      AND pr.ExpiresAt > SYSUTCDATETIME();

    IF @UserId IS NULL
        RETURN;

    UPDATE dbo.Users
    SET PasswordHash = @NewPasswordHash,
        UpdatedAtUtc = SYSUTCDATETIME()
    WHERE UserId = @UserId;

    UPDATE dbo.UserOneTimePasswordResetTokens
    SET IsUsed = 1,
        UpdatedAtUtc = SYSUTCDATETIME()
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
