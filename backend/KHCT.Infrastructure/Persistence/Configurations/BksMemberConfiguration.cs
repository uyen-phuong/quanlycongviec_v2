using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KHCT.Infrastructure.Persistence.Configurations;

public class BksMemberConfiguration : IEntityTypeConfiguration<BksMember>
{
    public void Configure(EntityTypeBuilder<BksMember> b)
    {
        b.ToTable("bks_member");
        b.HasKey(x => x.Id);
        b.Property(x => x.FullName).HasMaxLength(255).IsRequired();
        b.Property(x => x.Title).HasMaxLength(128);
    }
}
