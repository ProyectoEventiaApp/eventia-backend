using Microsoft.EntityFrameworkCore;
using Modules.Users.Domain;
using Modules.Security.Domain;
using Modules.Tickets.Domain;
using Modules.Events.Domain;
using Modules.Auditing.Domain;

namespace Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSets
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<Ticket> Tickets { get; set; } = null!;
    public DbSet<TicketType> TicketTypes { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ===== Users =====
        mb.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);
            b.Property(u => u.Id).ValueGeneratedOnAdd();
            b.Property(u => u.Name).IsRequired().HasMaxLength(150);
            b.Property(u => u.Email).IsRequired().HasMaxLength(200);
            b.HasIndex(u => u.Email).IsUnique();
            b.Property(u => u.PasswordHash).IsRequired();
            b.Property(u => u.IsActive).HasDefaultValue(true);

            b.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ===== Roles =====
        mb.Entity<Role>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.Id).ValueGeneratedOnAdd();
            b.Property(r => r.Name).IsRequired().HasMaxLength(100);
            b.HasIndex(r => r.Name).IsUnique();
            b.Property(r => r.IsActive).HasDefaultValue(true);
        });

        // ===== Permissions =====
        mb.Entity<Permission>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Id).ValueGeneratedOnAdd();
            b.Property(p => p.Key).IsRequired().HasMaxLength(150);
            b.HasIndex(p => p.Key).IsUnique();
            b.Property(p => p.Name).IsRequired().HasMaxLength(150);
            b.Property(p => p.Description).HasMaxLength(500);
            b.Property(p => p.IsActive).HasDefaultValue(true);
        });

        // ===== RolePermissions =====
        mb.Entity<RolePermission>(b =>
        {
            b.HasKey(rp => new { rp.RoleId, rp.PermissionId });

            b.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade); 

            b.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade); // 
        });

        // ===== Tickets =====
        mb.Entity<Ticket>(b =>
        {
            b.HasKey(t => t.Id);
            b.Property(t => t.Id).ValueGeneratedOnAdd();
            b.Property(t => t.Title).IsRequired().HasMaxLength(200);
            b.Property(t => t.Description).HasMaxLength(1000);
            b.Property(t => t.Status).HasDefaultValue(TicketStatus.Open);

            b.Property(t => t.CreatedAt)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");

            b.Property(t => t.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);
        });

        // ===== TicketType =====
        mb.Entity<TicketType>(b =>
        {
            b.HasKey(tt => tt.Id);
            b.Property(tt => tt.Id).ValueGeneratedOnAdd();
            b.Property(tt => tt.Name).IsRequired().HasMaxLength(100);
            b.HasIndex(tt => tt.Name).IsUnique();
        });

        // ===== Events =====
        mb.Entity<Event>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).ValueGeneratedOnAdd();
            b.Property(e => e.Title).IsRequired().HasMaxLength(200);
            b.Property(e => e.Description).HasMaxLength(1000);
            b.Property(e => e.StartDate).IsRequired();
            b.Property(e => e.EndDate).IsRequired();
            b.Property(e => e.Location).HasMaxLength(300);
            b.Property(e => e.MaxAttendees).HasDefaultValue(0);
            b.Property(e => e.CurrentAttendees).HasDefaultValue(0);
            
            b.Property(e => e.IsActive).HasDefaultValue(true);
            b.Property(e => e.CreatedAt)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");
            b.Property(e => e.UpdatedAt)
                .HasColumnType("datetime2")
                .IsRequired(false);
            
            b.HasIndex(e => e.StartDate);
            b.HasIndex(e => e.IsActive);
            b.HasIndex(e => new { e.StartDate, e.EndDate });
        });

        mb.Entity<AuditLog>(b =>
        {
            b.HasKey(a => a.Id);
            b.Property(a => a.Id).ValueGeneratedOnAdd();
            b.Property(a => a.Action).IsRequired().HasMaxLength(100);
            b.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
            b.Property(a => a.EntityId).IsRequired().HasMaxLength(50);
            b.Property(a => a.Description).IsRequired().HasMaxLength(500);
            b.Property(a => a.OldValues).HasColumnType("TEXT");
            b.Property(a => a.NewValues).HasColumnType("TEXT");
            b.Property(a => a.UserId).IsRequired();
            b.Property(a => a.CreatedAt)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETUTCDATE()");
            b.Property(a => a.IpAddress).HasMaxLength(45); 
            b.Property(a => a.UserAgent).HasMaxLength(500);
            b.HasIndex(a => new { a.EntityType, a.EntityId });
            b.HasIndex(a => a.UserId);
            b.HasIndex(a => a.CreatedAt);
            b.HasIndex(a => a.Action);
        });
    }
}
