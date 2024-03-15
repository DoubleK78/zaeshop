using System.ComponentModel.DataAnnotations;
using Common;
using Common.Enums;

namespace Portal.Domain.Models.UserModels
{
    public class UserDeviceResponseModel
    {
        public int Id { get; set; }
        public string RegistrationToken { get; set; } = null!;

        public EDeviceType DeviceType { get; set; }
        public string DeviceTypeName => CommonHelper.GetDescription(DeviceType);

        public string? BrowserVersion { get; set; }
        public string? ScreenResolution { get; set; }

        public bool IsEnabled { get; set; }
        public int UserId { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
    }

    public class UserDeviceRequestModel
    {
        public string? DeviceTypeName { get; set; }

        [Required(ErrorMessage = "error_registration_token_required")]
        public string RegistrationToken { get; set; } = null!;

        public string? BrowserVersion { get; set; }
        public string? ScreenResolution { get; set; }
    }
}
