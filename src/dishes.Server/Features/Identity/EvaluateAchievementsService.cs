using dishes.Server.Data;
using dishes.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace dishes.Server.Features.Identity;

/// <summary>
/// Service for evaluating achievement unlock criteria and persisting unlocked achievements.
/// </summary>
public interface IEvaluateAchievementsService
{
    /// <summary>
    /// Evaluates all achievement unlock criteria for a user and persists any newly unlocked achievements.
    /// Returns all currently unlocked achievements for the user, ordered by unlock date.
    /// </summary>
    Task<List<UserAchievement>> EvaluateAndPersistAsync(Guid userId);
}

public class EvaluateAchievementsService : IEvaluateAchievementsService
{
    private readonly AppDbContext _dbContext;

    public EvaluateAchievementsService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<UserAchievement>> EvaluateAndPersistAsync(Guid userId)
    {
        var userIdString = userId.ToString();

        // Get existing unlocked achievements to avoid re-unlocking
        var existingAchievements = await _dbContext.UserAchievements
            .Where(ua => ua.UserId == userIdString)
            .Select(ua => ua.AchievementId)
            .ToListAsync();

        var newlyUnlocked = new List<UserAchievement>();

        // Check Top Contributor: 5+ published recipes
        if (!existingAchievements.Contains(AchievementDefinitions.TopContributorId))
        {
            var topContributorUnlocked = await CheckTopContributor(userId);
            if (topContributorUnlocked)
            {
                var ua = new UserAchievement(
                    id: Guid.NewGuid(),
                    userId: userIdString,
                    achievementId: AchievementDefinitions.TopContributorId,
                    unlockedAt: DateTime.UtcNow
                );
                newlyUnlocked.Add(ua);
            }
        }

        // Check Master Chef: 50+ published recipes
        if (!existingAchievements.Contains(AchievementDefinitions.MasterChefId))
        {
            var masterChefUnlocked = await CheckMasterChef(userId);
            if (masterChefUnlocked)
            {
                var ua = new UserAchievement(
                    id: Guid.NewGuid(),
                    userId: userIdString,
                    achievementId: AchievementDefinitions.MasterChefId,
                    unlockedAt: DateTime.UtcNow
                );
                newlyUnlocked.Add(ua);
            }
        }

        // Check Hot Streak: 7 consecutive publishing days
        if (!existingAchievements.Contains(AchievementDefinitions.HotStreakId))
        {
            var hotStreakUnlocked = await CheckHotStreak(userId);
            if (hotStreakUnlocked)
            {
                var ua = new UserAchievement(
                    id: Guid.NewGuid(),
                    userId: userIdString,
                    achievementId: AchievementDefinitions.HotStreakId,
                    unlockedAt: DateTime.UtcNow
                );
                newlyUnlocked.Add(ua);
            }
        }

        // Check Video Star: at least 1 published recipe with video
        if (!existingAchievements.Contains(AchievementDefinitions.VideoStarId))
        {
            var videoStarUnlocked = await CheckVideoStar(userId);
            if (videoStarUnlocked)
            {
                var ua = new UserAchievement(
                    id: Guid.NewGuid(),
                    userId: userIdString,
                    achievementId: AchievementDefinitions.VideoStarId,
                    unlockedAt: DateTime.UtcNow
                );
                newlyUnlocked.Add(ua);
            }
        }

        // Persist newly unlocked achievements
        if (newlyUnlocked.Any())
        {
            _dbContext.UserAchievements.AddRange(newlyUnlocked);
            await _dbContext.SaveChangesAsync();
        }

        // Return all unlocked achievements
        var allUnlocked = await _dbContext.UserAchievements
            .Where(ua => ua.UserId == userIdString)
            .OrderByDescending(ua => ua.UnlockedAt)
            .ToListAsync();

        return allUnlocked;
    }

    private async Task<bool> CheckTopContributor(Guid userId)
    {
        var publishedRecipeCount = await _dbContext.Recipes
            .Where(r => r.CreatorId == userId && r.Status == RecipeStatus.Published)
            .CountAsync();

        return publishedRecipeCount >= 5;
    }

    private async Task<bool> CheckMasterChef(Guid userId)
    {
        var publishedRecipeCount = await _dbContext.Recipes
            .Where(r => r.CreatorId == userId && r.Status == RecipeStatus.Published)
            .CountAsync();

        return publishedRecipeCount >= 50;
    }

    private async Task<bool> CheckHotStreak(Guid userId)
    {
        // Get all published recipes by the user, grouped by calendar date (UTC)
        var publishDates = await _dbContext.Recipes
            .Where(r => r.CreatorId == userId && r.Status == RecipeStatus.Published)
            .Select(r => r.CreatedAt.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();

        if (publishDates.Count < 7)
            return false;

        // Find the longest consecutive sequence of dates
        int maxStreak = 1;
        int currentStreak = 1;

        for (int i = 1; i < publishDates.Count; i++)
        {
            if (publishDates[i] == publishDates[i - 1].AddDays(1))
            {
                currentStreak++;
                if (currentStreak >= 7)
                    return true;
            }
            else
            {
                currentStreak = 1;
            }
        }

        return maxStreak >= 7;
    }

    private async Task<bool> CheckVideoStar(Guid userId)
    {
        var hasVideoRecipe = await _dbContext.Recipes
            .Where(r => r.CreatorId == userId && 
                       r.Status == RecipeStatus.Published && 
                       r.VideoUrl != null && 
                       r.VideoUrl != "")
            .AnyAsync();

        return hasVideoRecipe;
    }
}
