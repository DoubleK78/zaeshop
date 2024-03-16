using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portal.Domain.AggregatesModel.UserAggregate;

namespace Portal.Infrastructure.EntityConfigurations.UserAggregate
{
    public class UserDeviceEntityTypeConfiguration : IEntityTypeConfiguration<UserDevice>
    {
        public void Configure(EntityTypeBuilder<UserDevice> builder)
        {
            builder.ToTable(nameof(UserDevice));
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.User).WithMany(y => y.UserDevices).HasForeignKey(z => z.UserId);
        }
    }
}
