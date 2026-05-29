using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KHCT.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.ToTable("role");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(32).IsRequired();
        b.Property(x => x.Name).HasMaxLength(128).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();
    }
}
