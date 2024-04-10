namespace Identity.Domain.Interfaces.Business.Services;

public interface IUserFingerPrintService
{
    Task<bool> CheckBannedFromFingerPrintAsync(string fingerPrint);
    Task CreateOrUpdateAsync(string userId, string fingerPrint, string? additionalDetail);

}
