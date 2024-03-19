using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portal.Domain.AggregatesModel.CollectionAggregate;

namespace Portal.Infrastructure.EntityConfigurations.CollectionAggregate
{
    public class CollectionViewEntityTypeConfiguration : IEntityTypeConfiguration<CollectionView>
    {
        public void Configure(EntityTypeBuilder<CollectionView> builder)
        {
            builder.ToTable(nameof(CollectionView));
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Collection).WithMany(y => y.CollectionViews).HasForeignKey(z => z.CollectionId);
            builder.HasOne(x => x.User).WithMany(y => y.CollectionViews).HasForeignKey(z => z.UserId);

            // Index To Calculate Views more efficient
            builder.HasIndex(cv => cv.CollectionId)
                .IncludeProperties(cv => new { cv.Date, cv.View })
                .HasDatabaseName("nci_msft_1_CollectionView_394301034398E39CFFB2DD574D69974B")
                .HasFilter(null)
                .IsUnique(false);
        }
    }
}
