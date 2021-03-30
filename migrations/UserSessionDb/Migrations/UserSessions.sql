IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [UserSessions] (
    [Id] int NOT NULL IDENTITY,
    [Renewed] datetime2 NOT NULL,
    [Expires] datetime2 NULL,
    [Ticket] nvarchar(max) NOT NULL,
    [Key] nvarchar(200) NOT NULL,
    [SubjectId] nvarchar(200) NOT NULL,
    [SessionId] nvarchar(450) NULL,
    [Scheme] nvarchar(200) NULL,
    [Created] datetime2 NOT NULL,
    CONSTRAINT [PK_UserSessions] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_UserSessions_Key] ON [UserSessions] ([Key]);
GO

CREATE UNIQUE INDEX [IX_UserSessions_SessionId] ON [UserSessions] ([SessionId]) WHERE [SessionId] IS NOT NULL;
GO

CREATE UNIQUE INDEX [IX_UserSessions_SubjectId_SessionId] ON [UserSessions] ([SubjectId], [SessionId]) WHERE [SessionId] IS NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210330165735_UserSessions', N'5.0.0');
GO

COMMIT;
GO

