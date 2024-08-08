using Identity.Domain.AggregatesModel.UserAggregate;
using Identity.Infrastructure.EntityConfigurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure
{
    public class AppIdentityDbContext : IdentityDbContext<User>
    {
        public DbSet<UserActivityLog> UserActivityLogs { get; set; }

        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options)
        {
        }

        protected AppIdentityDbContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new UserEntityTypeConfiguration());
            builder.ApplyConfiguration(new UserTokenEntityTypeConfiguration());
            builder.ApplyConfiguration(new UserActivityLogEntityTypeConfiguration());
        }
    }
}