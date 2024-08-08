using Identity.Domain.AggregatesModel.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.EntityConfigurations;

public class UserActivityLogEntityTypeConfiguration : IEntityTypeConfiguration<UserActivityLog>
{
    public void Configure(EntityTypeBuilder<UserActivityLog> builder)
    {
        builder.ToTable(nameof(UserActivityLog));
        builder.HasKey(x => x.Id);

        builder.HasIndex(o => new { o.UserId, o.EventDate }).IncludeProperties(x => new { x.LogTimes, x.IpAddress });
        builder.HasIndex(o => o.Fingerprinting).IncludeProperties(x => new { x.UserId, x.EventDate });
    }
}
