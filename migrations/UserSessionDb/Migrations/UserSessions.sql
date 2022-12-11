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
    [Id] bigint NOT NULL IDENTITY,
    [ApplicationName] nvarchar(200) NULL,
    [SubjectId] nvarchar(200) NOT NULL,
    [SessionId] nvarchar(450) NULL,
    [Created] datetime2 NOT NULL,
    [Renewed] datetime2 NOT NULL,
    [Expires] datetime2 NULL,
    [Ticket] nvarchar(max) NOT NULL,
    [Key] nvarchar(200) NOT NULL,
    CONSTRAINT [PK_UserSessions] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_UserSessions_ApplicationName_Key] ON [UserSessions] ([ApplicationName], [Key]) WHERE [ApplicationName] IS NOT NULL;
GO

CREATE UNIQUE INDEX [IX_UserSessions_ApplicationName_SessionId] ON [UserSessions] ([ApplicationName], [SessionId]) WHERE [ApplicationName] IS NOT NULL AND [SessionId] IS NOT NULL;
GO

CREATE UNIQUE INDEX [IX_UserSessions_ApplicationName_SubjectId_SessionId] ON [UserSessions] ([ApplicationName], [SubjectId], [SessionId]) WHERE [ApplicationName] IS NOT NULL AND [SessionId] IS NOT NULL;
GO

CREATE INDEX [IX_UserSessions_Expires] ON [UserSessions] ([Expires]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20221206212957_UserSessions', N'6.0.9');
GO

COMMIT;
GO

