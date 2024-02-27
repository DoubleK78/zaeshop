using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portal.Domain.AggregatesModel.CollectionAggregate;

namespace Portal.Infrastructure.EntityConfigurations.CollectionAggregate
{
    public class ContentItemEntityTypeConfiguration : IEntityTypeConfiguration<ContentItem>
    {
        public void Configure(EntityTypeBuilder<ContentItem> builder)
        {
            builder.ToTable(nameof(ContentItem));
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Collection).WithMany(y => y.ContentItems).HasForeignKey(z => z.CollectionId);

            // Index column CollectionId includes RelativeUrl help index seek instead of index scan
            builder.HasIndex(x => x.CollectionId).IncludeProperties(p => new { p.RelativeUrl });
        }
    }
}
