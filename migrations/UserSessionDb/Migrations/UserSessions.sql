CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;

CREATE TABLE "UserSessions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_UserSessions" PRIMARY KEY AUTOINCREMENT,
    "Renewed" TEXT NOT NULL,
    "Expires" TEXT NULL,
    "Ticket" TEXT NOT NULL,
    "Key" TEXT NOT NULL,
    "SubjectId" TEXT NOT NULL,
    "SessionId" TEXT NULL,
    "Scheme" TEXT NULL,
    "Created" TEXT NOT NULL
);

CREATE UNIQUE INDEX "IX_UserSessions_Key" ON "UserSessions" ("Key");

CREATE UNIQUE INDEX "IX_UserSessions_SessionId" ON "UserSessions" ("SessionId");

CREATE UNIQUE INDEX "IX_UserSessions_SubjectId_SessionId" ON "UserSessions" ("SubjectId", "SessionId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20210331125209_UserSessions', '5.0.0');

COMMIT;

