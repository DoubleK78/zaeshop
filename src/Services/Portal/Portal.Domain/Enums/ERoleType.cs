using System.ComponentModel;

namespace Portal.Domain.Enums
{
    public enum ERoleType
    {
        [Description("User")]
        User = 0,
        [Description("Partner")]
        Partner = 1,
        [Description("Admin")]
        Administrator = 2,
        [Description("User Premium")]
        UserPremium = 3,
        [Description("User Super Premium")]
        UserSuperPremium = 4
    }
}
