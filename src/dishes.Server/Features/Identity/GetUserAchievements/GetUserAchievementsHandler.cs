using dishes.Server.Data;
using dishes.Server.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace dishes.Server.Features.Identity;

/// <summary>
/// Handler for retrieving a user's unlocked achievements.
/// Evaluates achievement criteria on-demand before returning results.
/// </summary>
public class GetUserAchievementsHandler
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<AppIdentityUser> _userManager;
    private readonly IEvaluateAchievementsService _evaluateService;

    public GetUserAchievementsHandler(
        AppDbContext dbContext,
        UserManager<AppIdentityUser> userManager,
        IEvaluateAchievementsService evaluateService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _evaluateService = evaluateService;
    }

    public async Task<List<AchievementResponse>> HandleAsync(ClaimsPrincipal user)
    {
        var appUser = await _userManager.GetUserAsync(user);
        if (appUser == null)
            throw new InvalidOperationException("User not found");

        // Parse the user ID to Guid
        if (!Guid.TryParse(appUser.Id, out var userId))
            throw new InvalidOperationException("Invalid user ID format");

        // Evaluate and persist any newly unlocked achievements
        var unlockedAchievements = await _evaluateService.EvaluateAndPersistAsync(userId);

        // Map to response DTOs
        var responses = new List<AchievementResponse>();
        foreach (var achievement in unlockedAchievements)
        {
            var def = AchievementDefinitions.GetAchievementById(achievement.AchievementId);
            if (def != null)
            {
                responses.Add(new AchievementResponse
                {
                    Id = achievement.AchievementId,
                    Name = def.Name,
                    Description = def.Description,
                    Icon = def.Icon,
                    UnlockedAt = achievement.UnlockedAt,
                    CreatedAt = achievement.CreatedAt
                });
            }
        }

        return responses;
    }
}
