using BookingHub.Domain.Organizations;
using BookingHub.Domain.Reporting;
using BookingHub.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingHub.Infrastructure.Persistence.Configurations;

internal sealed class BookingExportJobConfiguration
    : IEntityTypeConfiguration<BookingExportJob>
{
    public void Configure(
        EntityTypeBuilder<BookingExportJob> builder)
    {
        builder.ToTable(
            "booking_export_jobs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.Property(x => x.StorageKey)
            .HasMaxLength(
                BookingExportJob.MaxStorageKeyLength);

        builder.Property(x => x.FileName)
            .HasMaxLength(
                BookingExportJob.MaxFileNameLength);

        builder.Property(x => x.LastError)
            .HasMaxLength(
                BookingExportJob.MaxErrorLength);

        builder.HasIndex(
            x => new
            {
                x.Status,
                x.CreatedAtUtc
            });

        builder.HasIndex(
            x => new
            {
                x.OrganizationId,
                x.RequestedByUserId,
                x.CreatedAtUtc
            });

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
