using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using LiPi.Clinic.Identity.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LiPi.Clinic.Identity;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    // identity.users table dropped — users now in master.platform_users
    // public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<MfaMethod> MfaMethods => Set<MfaMethod>();
    public DbSet<PasswordHistory> PasswordHistories => Set<PasswordHistory>();
    public DbSet<LoginAttempt>   LoginAttempts   => Set<LoginAttempt>();
    public DbSet<SecurityPolicy> SecurityPolicies => Set<SecurityPolicy>();
    // public DbSet<ServiceAccount> ServiceAccounts => Set<ServiceAccount>();  // table dropped
    // public DbSet<ApiKey> ApiKeys => Set<ApiKey>();  // table dropped
    // public DbSet<AdSyncRun> AdSyncRuns => Set<AdSyncRun>();  // table dropped
    public DbSet<ClinicProfile> ClinicProfiles => Set<ClinicProfile>();
	public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
	// Phase 2.8 (A39): per-user LipiTable state. Sibling of UserPreferences.
	public DbSet<UserTablePreference> UserTablePreferences => Set<UserTablePreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Use 'identity' schema
        modelBuilder.HasDefaultSchema("identity");

        // Snake_case table + column naming convention
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Table name → snake_case (fixes "Users" → "users", "UserRoles" → "user_roles")
            var tableName = entity.GetTableName();
            if (!string.IsNullOrEmpty(tableName))
                entity.SetTableName(ToSnakeCase(tableName));

            foreach (var property in entity.GetProperties())
                property.SetColumnName(ToSnakeCase(property.Name));

            foreach (var key in entity.GetKeys())
                key.SetName(ToSnakeCase(key.GetName() ?? string.Empty));

            foreach (var fk in entity.GetForeignKeys())
                fk.SetConstraintName(ToSnakeCase(fk.GetConstraintName() ?? string.Empty));

            foreach (var index in entity.GetIndexes())
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName() ?? string.Empty));
        }
		
		
    modelBuilder.Entity<UserPreference>(e =>
    {
        e.HasKey(p => p.UserId);
        e.Property(p => p.ThemeMode).HasMaxLength(20);
        e.Property(p => p.Density).HasMaxLength(20);
        e.Property(p => p.FontSize).HasMaxLength(20);
        e.Property(p => p.Language).HasMaxLength(10);
        // HasDefaultValue not set here — defaults are enforced at DB level
        // via SQL DEFAULT clauses in the migration script.
    });

    // Phase 2.8 (A39) — UserTablePreference (LipiTable state). Sibling of UserPreference.
    // SPEC: docs/00-COMPONENTS/2.8/00-Phase2.8-Overview.md §2.5
    // Schema "identity" applied by HasDefaultSchema; columns snake_cased by the loop
    // above (user_id / table_id / prefs_json / updated_at). Composite PK + jsonb +
    // length cap configured here. The actual table is created by the hand-written SQL
    // migration (2026-05-16-phase-2.8-user-table-prefs-up.sql), applied per clinic DB.
    modelBuilder.Entity<UserTablePreference>(e =>
    {
        e.HasKey(p => new { p.UserId, p.TableId });
        e.Property(p => p.TableId).HasMaxLength(200);
        e.Property(p => p.PrefsJson).HasColumnType("jsonb");
        e.HasIndex(p => p.UserId);
        // updated_at: the app (EfUserTablePreferenceStore) sets this UTC value on every
        // write. NOT configured as store-generated so EF always sends it. The SQL
        // DEFAULT NOW() in the migration is only a safety net for non-EF inserts.
    });

        // =========== User — REMOVED (users now in master.platform_users) ===========
        // Config below kept for reference only
        /*
        var userConfig = modelBuilder.Entity<User>();
        userConfig.HasKey(u => u.Id);
        userConfig.Property(u => u.Id).HasDefaultValueSql("core.uuid_v7()");
        userConfig.Property(u => u.CreatedAt).HasDefaultValueSql("now()");
        userConfig.Property(u => u.UpdatedAt).HasDefaultValueSql("now()");
        userConfig.Property(u => u.IsMfaEnforced).HasDefaultValue(true);
        userConfig.Property(u => u.MustChangePassword).HasDefaultValue(false);
        userConfig.Property(u => u.IsAdManaged).HasDefaultValue(false);
        userConfig.Property(u => u.FailedLoginCount).HasDefaultValue(0);
        userConfig.Property(u => u.Status).HasDefaultValue("active");

        userConfig.HasIndex(u => new { u.ClinicId, u.Username }).IsUnique();
        userConfig.HasIndex(u => new { u.ClinicId, u.Email }).IsUnique()
            .HasFilter("email IS NOT NULL AND deleted_at IS NULL");
        userConfig.HasIndex(u => new { u.ClinicId, u.Status }).HasFilter("deleted_at IS NULL");
        userConfig.HasIndex(u => u.AdObjectGuid).HasFilter("ad_object_guid IS NOT NULL");
        userConfig.HasIndex(u => u.StaffId).HasFilter("staff_id IS NOT NULL");

        // Convert IPAddress
        userConfig.Property(u => u.LastLoginIp)
;  // IPAddress→inet handled natively by Npgsql 10

        userConfig.Property(u => u.ExtensionData).HasColumnType("jsonb");
        userConfig.Property(u => u.RowVersion).IsConcurrencyToken();
        */

        // =========== Role ===========
        var roleConfig = modelBuilder.Entity<Role>();
        roleConfig.HasKey(r => r.Id);
        roleConfig.Property(r => r.Id).HasDefaultValueSql("core.uuid_v7()");
        roleConfig.Property(r => r.CreatedAt).HasDefaultValueSql("now()");
        roleConfig.Property(r => r.UpdatedAt).HasDefaultValueSql("now()");
        roleConfig.Property(r => r.IsActive).HasDefaultValue(true);
        roleConfig.Property(r => r.IsSystemRole).HasDefaultValue(false);
        roleConfig.HasIndex(r => new { r.ClinicId, r.Code }).IsUnique();
        roleConfig.Property(r => r.RowVersion).IsConcurrencyToken();

        // =========== Permission ===========
        var permConfig = modelBuilder.Entity<Permission>();
        permConfig.HasKey(p => p.Id);
        permConfig.Property(p => p.Id).HasDefaultValueSql("core.uuid_v7()");
        permConfig.Property(p => p.IsPhiSensitive).HasDefaultValue(false);
        permConfig.HasIndex(p => p.PermissionCode).IsUnique();

        // =========== RolePermission ===========
        var rpConfig = modelBuilder.Entity<RolePermission>();
        rpConfig.HasKey(rp => new { rp.RoleId, rp.PermissionId });
        rpConfig.Property(rp => rp.GrantedAt).HasDefaultValueSql("now()");
        rpConfig.HasOne(rp => rp.Role).WithMany(r => r.RolePermissions).HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
        rpConfig.HasOne(rp => rp.Permission).WithMany(p => p.RolePermissions).HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // =========== UserRole ===========
        var urConfig = modelBuilder.Entity<UserRole>();
        urConfig.HasKey(ur => ur.Id);
        urConfig.Property(ur => ur.Id).HasDefaultValueSql("core.uuid_v7()");
        urConfig.Property(ur => ur.AssignedAt).HasDefaultValueSql("now()");
        urConfig.Property(ur => ur.ValidFrom).HasDefaultValueSql("now()");
        urConfig.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        urConfig.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
        // Unique index for global (NULL scope) case
        urConfig.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique()
            .HasFilter("scope_department_id IS NULL AND valid_to IS NULL");

        // =========== Session ===========
        var sessConfig = modelBuilder.Entity<Session>();
        sessConfig.HasKey(s => s.Id);
        sessConfig.Property(s => s.Id).HasDefaultValueSql("core.uuid_v7()");
        sessConfig.Property(s => s.IssuedAt).HasDefaultValueSql("now()");
        sessConfig.Property(s => s.LastActiveAt).HasDefaultValueSql("now()");
        sessConfig.Property(s => s.ClientIp)
;  // IPAddress→inet handled natively by Npgsql 10
        sessConfig.HasOne(s => s.User).WithMany(u => u.Sessions).HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        sessConfig.HasIndex(s => s.UserId).HasFilter("revoked_at IS NULL");
        sessConfig.HasIndex(s => s.JwtJti);
        sessConfig.HasIndex(s => s.ExpiresAt).HasFilter("revoked_at IS NULL");

        // =========== MfaMethod ===========
        var mfaConfig = modelBuilder.Entity<MfaMethod>();
        mfaConfig.HasKey(m => m.Id);
        mfaConfig.Property(m => m.Id).HasDefaultValueSql("core.uuid_v7()");
        mfaConfig.Property(m => m.CreatedAt).HasDefaultValueSql("now()");
        mfaConfig.Property(m => m.IsPrimary).HasDefaultValue(false);
        mfaConfig.Property(m => m.IsVerified).HasDefaultValue(false);
        mfaConfig.HasOne(m => m.User).WithMany(u => u.MfaMethods).HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        mfaConfig.HasIndex(m => m.UserId).HasFilter("revoked_at IS NULL");

        // =========== PasswordHistory ===========
        var phConfig = modelBuilder.Entity<PasswordHistory>();
        phConfig.HasKey(ph => ph.Id);
        phConfig.Property(ph => ph.Id).HasDefaultValueSql("core.uuid_v7()");
        phConfig.Property(ph => ph.SetAt).HasDefaultValueSql("now()");
        phConfig.HasOne(ph => ph.User).WithMany(u => u.PasswordHistory).HasForeignKey(ph => ph.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        phConfig.HasIndex(ph => new { ph.UserId, ph.SetAt }).IsDescending(false, true);

        // =========== LoginAttempt ===========
        var laConfig = modelBuilder.Entity<LoginAttempt>();
        laConfig.HasKey(la => la.Id);
        laConfig.Property(la => la.Id).HasDefaultValueSql("core.uuid_v7()");
        laConfig.Property(la => la.AttemptedAt).HasDefaultValueSql("now()");
        laConfig.Property(la => la.ClientIp)
;  // IPAddress→inet handled natively by Npgsql 10
        laConfig.HasOne(la => la.User).WithMany(u => u.LoginAttempts).HasForeignKey(la => la.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        laConfig.HasIndex(la => new { la.UserId, la.AttemptedAt }).IsDescending(false, true);
        laConfig.HasIndex(la => new { la.ClientIp, la.AttemptedAt }).IsDescending(false, true);
        laConfig.HasIndex(la => new { la.ClinicId, la.Outcome, la.AttemptedAt }).IsDescending(false, false, true);

        // // =========== ServiceAccount ===========
        // var saConfig = modelBuilder.Entity<ServiceAccount>();
        // saConfig.HasKey(sa => sa.Id);
        // saConfig.Property(sa => sa.Id).HasDefaultValueSql("core.uuid_v7()");
        // saConfig.Property(sa => sa.CreatedAt).HasDefaultValueSql("now()");
        // saConfig.Property(sa => sa.IsActive).HasDefaultValue(true);
        // saConfig.Property(sa => sa.AllowedIpCidrs).HasColumnType("inet[]");
        // saConfig.HasOne(sa => sa.User).WithMany().HasForeignKey(sa => sa.UserId)
        //     .IsRequired(false)
        //     .OnDelete(DeleteBehavior.SetNull);
        // saConfig.HasIndex(sa => new { sa.ClinicId, sa.Name }).IsUnique();
// 
//         // // =========== ApiKey ===========
        // var akConfig = modelBuilder.Entity<ApiKey>();
        // akConfig.HasKey(ak => ak.Id);
        // akConfig.Property(ak => ak.Id).HasDefaultValueSql("core.uuid_v7()");
        // akConfig.Property(ak => ak.CreatedAt).HasDefaultValueSql("now()");
        // akConfig.Property(ak => ak.Scopes).HasColumnType("text[]");
        // akConfig.Property(ak => ak.LastUsedIp)
// ;  // IPAddress→inet handled natively by Npgsql 10
        // akConfig.HasOne(ak => ak.ServiceAccount).WithMany(sa => sa.ApiKeys).HasForeignKey(ak => ak.ServiceAccountId)
        //     .OnDelete(DeleteBehavior.Cascade);
        // akConfig.HasIndex(ak => ak.KeyPrefix).IsUnique();
        // akConfig.HasIndex(ak => ak.ServiceAccountId).HasFilter("revoked_at IS NULL");
// 
//         // =========== ClinicProfile ===========
        var cpConfig = modelBuilder.Entity<ClinicProfile>();
        cpConfig.HasKey(cp => cp.Id);
        cpConfig.Property(cp => cp.Id).HasDefaultValueSql("core.uuid_v7()");
        cpConfig.Property(cp => cp.CreatedAt).HasDefaultValueSql("now()");
        cpConfig.Property(cp => cp.UpdatedAt).HasDefaultValueSql("now()");
        cpConfig.Property(cp => cp.Status).HasDefaultValue("active");
        cpConfig.Property(cp => cp.ExtensionData).HasColumnType("jsonb");
        cpConfig.HasIndex(cp => new { cp.PlatformUserId, cp.ClinicId }).IsUnique()
            .HasFilter("deleted_at IS NULL");

        // =========== SecurityPolicy ===========
        var spConfig = modelBuilder.Entity<SecurityPolicy>();
        spConfig.HasKey(sp => sp.Id);
        spConfig.Property(sp => sp.Id).HasDefaultValueSql("core.uuid_v7()");
        spConfig.Property(sp => sp.UpdatedAt).HasDefaultValueSql("now()");
        spConfig.Property(sp => sp.MinLength).HasDefaultValue(8);
        spConfig.Property(sp => sp.RequireUppercase).HasDefaultValue(true);
        spConfig.Property(sp => sp.RequireLowercase).HasDefaultValue(true);
        spConfig.Property(sp => sp.RequireDigit).HasDefaultValue(true);
        spConfig.Property(sp => sp.RequireSymbol).HasDefaultValue(true);
        spConfig.Property(sp => sp.ExpiryDays).HasDefaultValue(30);
        spConfig.Property(sp => sp.HistoryCount).HasDefaultValue(0);
        spConfig.Property(sp => sp.MaxFailedAttempts).HasDefaultValue(5);
        spConfig.Property(sp => sp.LockoutMinutes).HasDefaultValue(30);
        spConfig.Property(sp => sp.IdleTimeoutMinutes).HasDefaultValue(60);
        spConfig.Property(sp => sp.SessionMaxHours).HasDefaultValue(10);
        spConfig.Property(sp => sp.EnforceSingleSession).HasDefaultValue(true);
        spConfig.Property(sp => sp.MfaRequired).HasDefaultValue(false);
        spConfig.Property(sp => sp.MfaGraceDays).HasDefaultValue(0);

        // // =========== AdSyncRun ===========
        // var adasyncConfig = modelBuilder.Entity<AdSyncRun>();
        // adasyncConfig.HasKey(asr => asr.Id);
        // adasyncConfig.Property(asr => asr.Id).HasDefaultValueSql("core.uuid_v7()");
        // adasyncConfig.Property(asr => asr.StartedAt).HasDefaultValueSql("now()");
        // adasyncConfig.Property(asr => asr.ErrorDetails).HasColumnType("jsonb");
        // adasyncConfig.HasIndex(asr => new { asr.IdentityProviderId, asr.StartedAt }).IsDescending(false, true);
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsUpper(c))
            {
                if (i > 0 && !char.IsUpper(input[i - 1]))
                    sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
                sb.Append(c);
        }
        return sb.ToString();
    }
}
