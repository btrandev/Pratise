-- V0__Initial_Schema.sql
-- Initial schema for AdminService database

-- Create Tenants table
CREATE TABLE "Tenants" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Code" VARCHAR(50) NOT NULL,
    "Description" VARCHAR(500) NULL,
    "Domain" VARCHAR(100) NOT NULL,
    "SubscriptionPlan" VARCHAR(20) NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL,
    "CreatedById" UUID NOT NULL,
    "CreatedByName" VARCHAR(200) NULL,
    "UpdatedAt" TIMESTAMP NULL,
    "UpdatedById" UUID NULL,
    "UpdatedByName" VARCHAR(200) NULL
);

-- Create indexes on Tenants table
CREATE UNIQUE INDEX "IX_Tenants_Code" ON "Tenants" ("Code") WHERE "IsDeleted" = FALSE;
CREATE UNIQUE INDEX "IX_Tenants_Domain" ON "Tenants" ("Domain") WHERE "IsDeleted" = FALSE;

-- Create Users table
CREATE TABLE "Users" (
    "Id" UUID PRIMARY KEY,
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "Email" VARCHAR(150) NOT NULL,
    "Username" VARCHAR(100) NOT NULL,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "PhoneNumber" VARCHAR(20) NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "EmailConfirmed" BOOLEAN NOT NULL DEFAULT FALSE,
    "LastLoginAt" TIMESTAMP NULL,
    "Role" VARCHAR(20) NULL,
    "TenantId" UUID NOT NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL,
    "CreatedById" UUID NOT NULL,
    "CreatedByName" VARCHAR(200) NULL,
    "UpdatedAt" TIMESTAMP NULL,
    "UpdatedById" UUID NULL,
    "UpdatedByName" VARCHAR(200) NULL,
    CONSTRAINT "FK_Users_Tenants_TenantId" FOREIGN KEY ("TenantId") REFERENCES "Tenants" ("Id") ON DELETE RESTRICT
);

-- Create indexes on Users table
CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email") WHERE "IsDeleted" = FALSE;
CREATE UNIQUE INDEX "IX_Users_Username" ON "Users" ("Username") WHERE "IsDeleted" = FALSE;
CREATE INDEX "IX_Users_TenantId" ON "Users" ("TenantId");

-- Create UserClaims table
CREATE TABLE "UserClaims" (
    "Id" UUID PRIMARY KEY,
    "ClaimType" VARCHAR(100) NOT NULL,
    "ClaimValue" VARCHAR(500) NOT NULL,
    "UserId" UUID NOT NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL,
    "CreatedById" UUID NOT NULL,
    "CreatedByName" VARCHAR(200) NULL,
    "UpdatedAt" TIMESTAMP NULL,
    "UpdatedById" UUID NULL,
    "UpdatedByName" VARCHAR(200) NULL,
    CONSTRAINT "FK_UserClaims_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

-- Create index on UserClaims table
CREATE INDEX "IX_UserClaims_UserId" ON "UserClaims" ("UserId");
