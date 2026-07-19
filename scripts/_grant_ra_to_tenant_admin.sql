-- Grant Regulatory Affairs permissions to Tenant Administrator on Irving tenant
INSERT INTO compliance360.role_permissions ("Id", "TenantId", "RoleId", "PermissionId", "CreatedAtUtc")
SELECT gen_random_uuid(), r."TenantId", r."Id", p."Id", NOW()
FROM compliance360.roles r
CROSS JOIN compliance360.permissions p
WHERE r."TenantId" = '82af3877-2786-4d39-bce8-c981101c771d'
  AND r."Name" = 'Tenant Administrator'
  AND p."Code" LIKE 'REGULATORY.%'
  AND NOT EXISTS (
    SELECT 1 FROM compliance360.role_permissions rp
    WHERE rp."TenantId" = r."TenantId" AND rp."RoleId" = r."Id" AND rp."PermissionId" = p."Id"
  );

SELECT p."Code"
FROM compliance360.role_permissions rp
JOIN compliance360.roles r ON r."Id" = rp."RoleId"
JOIN compliance360.permissions p ON p."Id" = rp."PermissionId"
WHERE r."TenantId" = '82af3877-2786-4d39-bce8-c981101c771d'
  AND r."Name" = 'Tenant Administrator'
  AND p."Code" LIKE 'REGULATORY.%'
ORDER BY p."Code";
