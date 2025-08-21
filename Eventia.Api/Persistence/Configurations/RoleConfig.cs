using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Security.Domain;

namespace Persistence.Configurations;

public class RoleConfig : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.ToTable("Roles");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(50);
        b.HasIndex(x => x.Name).IsUnique();
        b.Property(x => x.IsActive).IsRequired();
    }
}
