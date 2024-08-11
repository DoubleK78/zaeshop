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

        builder.HasIndex(x => new { x.EventDate, x.UserId }).IncludeProperties(p => new { p.IpAddress, p.BrowserFingerprint });
    }
}
