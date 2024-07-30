using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portal.Domain.AggregatesModel.UserAggregate;

namespace Portal.Infrastructure.EntityConfigurations.UserAggregate
{
    public class UserActivityLogEntityTypeConfiguration : IEntityTypeConfiguration<UserActivityLog>
    {
        public void Configure(EntityTypeBuilder<UserActivityLog> builder)
        {
            builder.ToTable(nameof(UserActivityLog));
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.User).WithMany(y => y.UserActivityLogs).HasForeignKey(z => z.UserId);

            builder.HasIndex(x => x.UserId);
        }
    }
}
