-- V1__InitialPermissionSeed.sql
-- Initial migration to seed system tenant, admin user, and permissions

-- Create a system admin tenant
INSERT INTO "Tenants" 
("Id", "Code", "CreatedAt", "CreatedById", "CreatedByName", "Description", "Domain", "IsActive", "IsDeleted", "Name", "SubscriptionPlan", "UpdatedAt", "UpdatedById", "UpdatedByName")
VALUES 
('11111111-1111-1111-1111-111111111111', 
'SYSTEM', 
NOW(), 
'00000000-0000-0000-0000-000000000001', 
'system', 
'System tenant for administrative purposes', 
'system.local', 
TRUE, 
FALSE, 
'System Tenant', 
'Enterprise', 
NULL, 
NULL, 
NULL);

-- Create an admin user
-- Note: In a real production environment, you would use a more secure way to set passwords
INSERT INTO "Users" 
("Id", "CreatedAt", "CreatedById", "CreatedByName", "Email", "EmailConfirmed", "FirstName", "IsActive", "IsDeleted", "LastLoginAt", "LastName", "PasswordHash", "PhoneNumber", "Role", "TenantId", "UpdatedAt", "UpdatedById", "UpdatedByName", "Username")
VALUES 
('22222222-2222-2222-2222-222222222222', 
NOW(), 
'00000000-0000-0000-0000-000000000001', 
'system', 
'admin@system.local', 
TRUE, 
'System', 
TRUE, 
FALSE, 
NULL, 
'Admin', 
-- This is a placeholder for the password hash; in a real scenario, you would generate this separately
-- BCrypt hash of 'admin123' - for PostgreSQL, you might need to store this as a string directly
'$2a$12$FS0VN5m3jqYmAm5X5MHaYOMXhU3fFv9toPr.7BQD6XmQYcJ.hzNZe', 
NULL, 
'Admin', 
'11111111-1111-1111-1111-111111111111', 
NULL, 
NULL, 
NULL, 
'admin');

-- Add admin permissions
-- Users.View permission
INSERT INTO "UserClaims" 
("Id", "CreatedAt", "CreatedById", "CreatedByName", "ClaimType", "ClaimValue", "IsDeleted", "UpdatedAt", "UpdatedById", "UpdatedByName", "UserId")
VALUES 
(gen_random_uuid(), 
NOW(), 
'00000000-0000-0000-0000-000000000001', 
'system', 
'permission', 
'Users.View', 
FALSE, 
NULL, 
NULL, 
NULL, 
'22222222-2222-2222-2222-222222222222');

-- Users.Create permission
INSERT INTO "UserClaims" 
("Id", "CreatedAt", "CreatedById", "CreatedByName", "ClaimType", "ClaimValue", "IsDeleted", "UpdatedAt", "UpdatedById", "UpdatedByName", "UserId")
VALUES 
(gen_random_uuid(), 
NOW(), 
'00000000-0000-0000-0000-000000000001', 
'system', 
'permission', 
'Users.Create', 
FALSE, 
NULL, 
NULL, 
NULL, 
'22222222-2222-2222-2222-222222222222');

-- Users.Update permission
INSERT INTO "UserClaims" 
("Id", "CreatedAt", "CreatedById", "CreatedByName", "ClaimType", "ClaimValue", "IsDeleted", "UpdatedAt", "UpdatedById", "UpdatedByName", "UserId")
VALUES 
(gen_random_uuid(), 
NOW(), 
'00000000-0000-0000-0000-000000000001', 
'system', 
'permission', 
'Users.Update', 
FALSE, 
NULL, 
NULL, 
NULL, 
'22222222-2222-2222-2222-222222222222');

-- Users.Delete permission
INSERT INTO "UserClaims" 
("Id", "CreatedAt", "CreatedById", "CreatedByName", "ClaimType", "ClaimValue", "IsDeleted", "UpdatedAt", "UpdatedById", "UpdatedByName", "UserId")
VALUES 
(gen_random_uuid(), 
NOW(), 
'00000000-0000-0000-0000-000000000001', 
'system', 
'permission', 
'Users.Delete', 
FALSE, 
NULL, 
NULL, 
NULL, 
'22222222-2222-2222-2222-222222222222');

-- Tenants.View permission
INSERT INTO "UserClaims" 
("Id", "CreatedAt", "CreatedById", "CreatedByName", "ClaimType", "ClaimValue", "IsDeleted", "UpdatedAt", "UpdatedById", "UpdatedByName", "UserId")
VALUES 
(gen_random_uuid(), 
NOW(), 
'00000000-0000-0000-0000-000000000001', 
'system', 
'permission', 
'Tenants.View', 
FALSE, 
NULL, 
NULL, 
NULL, 
'22222222-2222-2222-2222-222222222222');

-- Tenants.Create permission
INSERT INTO "UserClaims" 
("Id", "CreatedAt", "CreatedById", "CreatedByName", "ClaimType", "ClaimValue", "IsDeleted", "UpdatedAt", "UpdatedById", "UpdatedByName", "UserId")
VALUES 
(gen_random_uuid(), 
NOW(), 
'00000000-0000-0000-0000-000000000001', 
'system', 
'permission', 
'Tenants.Create', 
FALSE, 
NULL, 
NULL, 
NULL, 
'22222222-2222-2222-2222-222222222222');

-- Tenants.Update permission
INSERT INTO "UserClaims" 
("Id", "CreatedAt", "CreatedById", "CreatedByName", "ClaimType", "ClaimValue", "IsDeleted", "UpdatedAt", "UpdatedById", "UpdatedByName", "UserId")
VALUES 
(gen_random_uuid(), 
NOW(), 
'00000000-0000-0000-0000-000000000001', 
'system', 
'permission', 
'Tenants.Update', 
FALSE, 
NULL, 
NULL, 
NULL, 
'22222222-2222-2222-2222-222222222222');

-- Tenants.Delete permission
INSERT INTO "UserClaims" 
("Id", "CreatedAt", "CreatedById", "CreatedByName", "ClaimType", "ClaimValue", "IsDeleted", "UpdatedAt", "UpdatedById", "UpdatedByName", "UserId")
VALUES 
(gen_random_uuid(), 
NOW(), 
'00000000-0000-0000-0000-000000000001', 
'system', 
'permission', 
'Tenants.Delete', 
FALSE, 
NULL, 
NULL, 
NULL, 
'22222222-2222-2222-2222-222222222222');
