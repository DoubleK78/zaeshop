using Identity.Domain.AggregatesModel.UserAggregate;
using Identity.Domain.Interfaces.Business.Services;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Implements.Business.Services;

public class UserFingerPrintService : IUserFingerPrintService
{
    private readonly AppIdentityDbContext _context;

    public UserFingerPrintService(AppIdentityDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CheckBannedFromFingerPrintAsync(string fingerPrint)
    {
        var isBanned = await _context.UserFingerPrints.AnyAsync(o => o.FingerPrint == fingerPrint && o.IsBanned);
        return isBanned;
    }

    public async Task CreateOrUpdateAsync(string userId, string fingerPrint, string? additionalDetail)
    {
        var userFingerPrint = await _context.UserFingerPrints.FirstOrDefaultAsync(o => o.FingerPrint == fingerPrint);
        if (userFingerPrint == null)
        {
            userFingerPrint = new UserFingerPrint
            {
                FingerPrint = fingerPrint,
                AdditionalDetail = additionalDetail,
                UserId = userId
            };

            _context.UserFingerPrints.Add(userFingerPrint);
        }
        else
        {
            userFingerPrint.UpdatedOnUtc = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}
