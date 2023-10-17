﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portal.Domain.AggregatesModel.UserAggregate;

namespace Portal.Infrastructure.EntityConfigurations.UserAggregate;
public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(nameof(User));
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.IdentityUserId).IsUnique();
    }
}