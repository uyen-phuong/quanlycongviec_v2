using KHCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KHCT.Infrastructure.Persistence.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> b)
    {
        b.ToTable("attachment");
        b.HasKey(x => x.Id);
        b.Property(x => x.OwnerType).HasMaxLength(64).IsRequired();
        b.Property(x => x.FileName).HasMaxLength(512).IsRequired();
        b.Property(x => x.StoredPath).HasMaxLength(1024).IsRequired();
        b.Property(x => x.ContentType).HasMaxLength(128);
        b.HasIndex(x => new { x.OwnerType, x.OwnerId });
        b.HasOne(x => x.UploadedByUser)
            .WithMany()
            .HasForeignKey(x => x.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
