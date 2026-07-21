namespace HR.Identity.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using HR.Identity.Domain;

/// <summary>
/// Entity Framework Core database context for Identity Service.
/// </summary>
public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<UserClaim> UserClaims { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<RoleClaim> RoleClaims { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Email, e.TenantId }).IsUnique();
            entity.HasIndex(e => new { e.Username, e.TenantId }).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(256);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasMany(e => e.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.UserClaims)
                .WithOne(uc => uc.User)
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Name, e.TenantId }).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasMany(e => e.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.RolePermissions)
                .WithOne(rp => rp.Role)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.RoleClaims)
                .WithOne(rc => rc.Role)
                .HasForeignKey(rc => rc.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Permission entity
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Resource, e.Action, e.TenantId }).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Resource).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(256);
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasMany(e => e.RolePermissions)
                .WithOne(rp => rp.Permission)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure UserRole junction
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure UserClaim
        modelBuilder.Entity<UserClaim>(entity =>
        {
            entity.ToTable("UserClaims");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClaimType).IsRequired().HasMaxLength(256);
            entity.Property(e => e.ClaimValue).IsRequired().HasMaxLength(500);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure RefreshToken
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.Property(e => e.Token).IsRequired();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure RolePermission junction
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure RoleClaim
        modelBuilder.Entity<RoleClaim>(entity =>
        {
            entity.ToTable("RoleClaims");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ClaimType).IsRequired().HasMaxLength(256);
            entity.Property(e => e.ClaimValue).IsRequired().HasMaxLength(500);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure Outbox
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.HasIndex(e => e.ProcessedOnUtc);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}
