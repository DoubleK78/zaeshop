using System.ComponentModel;

namespace Common.Enums
{
    public enum ERoleType
    {
        [Description("User")]
        User = 0,
        [Description("User Premium")]
        UserPremium = 1,
        [Description("User Super Premium")]
        UserSuperPremium = 2,
        [Description("Partner")]
        Partner = 3,
        [Description("Admin")]
        Administrator = 99
    }
}
