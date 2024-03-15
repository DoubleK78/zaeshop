using System.ComponentModel;

namespace Common.Enums
{
    public enum EDeviceType
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("iOS")]
        iOS = 1,
        [Description("Android")]
        Android = 2
    }
}
