namespace Portal.Domain.Interfaces.External
{
    public interface IFirebaseCloudMessageService
    {
        Task<string> SendAsync(string registrationToken, string title, string description, string? clickAction = null);
        Task<bool> SendAllAsync(List<string> registrationTokens, string title, string description, string? clickAction = null);
    }
}
