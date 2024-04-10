using Identity.Domain.AggregatesModel.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.EntityConfigurations;

public class UserFingerPrintEntityTypeConfiguration : IEntityTypeConfiguration<UserFingerPrint>
{
    public void Configure(EntityTypeBuilder<UserFingerPrint> builder)
    {
        builder.ToTable(nameof(User));
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.User).WithMany(y => y.UserFingerPrints).HasForeignKey(z => z.UserId);
        builder.HasIndex(x => x.FingerPrint).IncludeProperties(x => new { x.IsBanned });
    }
}
