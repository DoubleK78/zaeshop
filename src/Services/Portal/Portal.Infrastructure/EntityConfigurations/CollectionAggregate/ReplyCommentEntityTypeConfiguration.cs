using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portal.Domain.AggregatesModel.CollectionAggregate;

namespace Portal.Infrastructure.EntityConfigurations.CollectionAggregate
{
    public class ReplyCommentEntityTypeConfiguration : IEntityTypeConfiguration<ReplyComment>
    {
        public void Configure(EntityTypeBuilder<ReplyComment> builder)
        {
            builder.ToTable(nameof(ReplyComment));
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Comment).WithMany(y => y.ReplyComments).HasForeignKey(z => z.CommentId);
            builder.HasOne(x => x.User).WithMany(y => y.ReplyComments).HasForeignKey(z => z.UserId);
        }
    }
}
