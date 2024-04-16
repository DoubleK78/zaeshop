using Identity.Domain.AggregatesModel.UserAggregate;
using Microsoft.AspNetCore.Identity;

namespace Identity.Infrastructure
{
    public static class AppIdentityDbContextSeed
    {
        public static async Task SeedUsersAsync(UserManager<User> userManager)
        {
            if (!userManager.Users.Any())
            {
                // var user = new User
                // {
                //     FullName = "",
                //     Email = "",
                //     UserName = "",
                // };

                // await userManager.CreateAsync(user, "");
            }
        }
    }
}