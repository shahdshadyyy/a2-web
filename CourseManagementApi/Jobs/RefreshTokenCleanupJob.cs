using CourseManagementApi.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseManagementApi.Jobs;

public class RefreshTokenCleanupJob
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RefreshTokenCleanupJob> _logger;

    public RefreshTokenCleanupJob(ApplicationDbContext context, ILogger<RefreshTokenCleanupJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        var now = DateTime.UtcNow;
        var stale = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt < now || rt.IsRevoked)
            .ToListAsync();

        if (stale.Count == 0)
        {
            _logger.LogInformation("Refresh token cleanup: nothing to remove.");
            return;
        }

        _context.RefreshTokens.RemoveRange(stale);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Refresh token cleanup removed {Count} tokens.", stale.Count);
    }
}
