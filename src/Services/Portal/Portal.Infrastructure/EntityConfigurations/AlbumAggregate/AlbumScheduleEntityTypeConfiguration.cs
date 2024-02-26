using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portal.Domain.AggregatesModel.AlbumAggregate;

namespace Portal.Infrastructure.EntityConfigurations.AlbumAggregate
{
    public class AlbumScheduleEntityTypeConfiguration : IEntityTypeConfiguration<ScheduleAlbum>
    {
        public void Configure(EntityTypeBuilder<ScheduleAlbum> builder)
        {
            builder.ToTable(nameof(ScheduleAlbum));
            builder.HasKey(o => o.Id);
        }
    }
}
