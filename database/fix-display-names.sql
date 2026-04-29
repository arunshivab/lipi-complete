-- Fix display names in master.platform_users from identity.users ExtensionData
UPDATE master.platform_users pu
SET 
    display_name = COALESCE(
        NULLIF((iu.extension_data::jsonb)->>'displayName', ''),
        TRIM(
            CONCAT(
                NULLIF((iu.extension_data::jsonb)->>'firstName', ''),
                ' ',
                NULLIF((iu.extension_data::jsonb)->>'lastName', '')
            )
        ),
        pu.username
    ),
    first_name = COALESCE(NULLIF((iu.extension_data::jsonb)->>'firstName', ''), pu.first_name),
    last_name  = COALESCE(NULLIF((iu.extension_data::jsonb)->>'lastName',  ''), pu.last_name),
    updated_at = now()
FROM identity.users iu
WHERE iu.username = pu.username
  AND iu.deleted_at IS NULL
  AND pu.deleted_at IS NULL;

-- Verify
SELECT username, display_name, first_name, last_name FROM master.platform_users WHERE deleted_at IS NULL;
