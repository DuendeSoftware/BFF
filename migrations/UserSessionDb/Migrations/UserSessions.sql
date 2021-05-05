CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;

CREATE TABLE "UserSessions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_UserSessions" PRIMARY KEY AUTOINCREMENT,
    "ApplicationDiscriminator" TEXT NULL,
    "Renewed" TEXT NOT NULL,
    "Expires" TEXT NULL,
    "Ticket" TEXT NOT NULL,
    "Key" TEXT NOT NULL,
    "SubjectId" TEXT NOT NULL,
    "SessionId" TEXT NULL,
    "Scheme" TEXT NULL,
    "Created" TEXT NOT NULL
);

CREATE UNIQUE INDEX "IX_UserSessions_ApplicationDiscriminator_Key" ON "UserSessions" ("ApplicationDiscriminator", "Key");

CREATE UNIQUE INDEX "IX_UserSessions_ApplicationDiscriminator_SessionId" ON "UserSessions" ("ApplicationDiscriminator", "SessionId");

CREATE UNIQUE INDEX "IX_UserSessions_ApplicationDiscriminator_SubjectId_SessionId" ON "UserSessions" ("ApplicationDiscriminator", "SubjectId", "SessionId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20210505201937_UserSessions', '5.0.0');

COMMIT;

