using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portal.Domain.AggregatesModel.CollectionAggregate;

namespace Portal.Infrastructure.EntityConfigurations.CollectionAggregate
{
    public class CommentEntityTypeConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable(nameof(Comment));
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Album).WithMany(y => y.Comments).HasForeignKey(z => z.AlbumId);
            builder.HasOne(x => x.Collection).WithMany(y => y.Comments).HasForeignKey(z => z.CollectionId);
            builder.HasOne(x => x.User).WithMany(y => y.Comments).HasForeignKey(z => z.UserId);
        }
    }
}
