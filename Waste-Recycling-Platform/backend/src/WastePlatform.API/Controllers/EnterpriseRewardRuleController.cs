using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WastePlatform.Domain.Entities;
using WastePlatform.Infrastructure.Persistence;

namespace WastePlatform.API.Controllers;

[ApiController]
[Route("api/enterprise/reward-rules")]
[Authorize(Roles = "Enterprise")]
public class EnterpriseRewardRuleController : ControllerBase
{
    private readonly WastePlatformDbContext _context;

    public EnterpriseRewardRuleController(WastePlatformDbContext context)
    {
        _context = context;
    }

    private async Task<Enterprise?> GetCurrentEnterpriseAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return null;

        return await _context.Enterprises.FirstOrDefaultAsync(e => e.UserId == userId);
    }

    /// <summary>
    /// Lấy danh sách quy tắc điểm thưởng theo loại rác của Enterprise hiện tại.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRewardRules()
    {
        var enterprise = await GetCurrentEnterpriseAsync();
        if (enterprise == null)
            return Unauthorized(new { message = "Enterprise profile not found for current user." });

        var rewardRules = await _context.RewardRules
            .Where(rule => rule.EnterpriseId == enterprise.Id)
            .Include(rule => rule.WasteCategory)
            .OrderBy(rule => rule.WasteCategory.Name)
            .Select(rule => new
            {
                rule.Id,
                rule.WasteCategoryId,
                CategoryName = rule.WasteCategory.Name,
                rule.PointsPerReport,
                rule.BonusQuality,
                rule.IsActive
            })
            .ToListAsync();

        return Ok(rewardRules);
    }

    /// <summary>
    /// Cập nhật hàng loạt quy tắc điểm thưởng của Enterprise.
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateRewardRules([FromBody] UpdateEnterpriseRewardRulesRequest request)
    {
        var enterprise = await GetCurrentEnterpriseAsync();
        if (enterprise == null)
            return Unauthorized(new { message = "Enterprise profile not found for current user." });

        if (request.Rules == null || request.Rules.Count == 0)
            return BadRequest(new { message = "Rules cannot be empty." });

        if (request.Rules.Select(rule => rule.WasteCategoryId).Distinct().Count() != request.Rules.Count)
            return BadRequest(new { message = "Duplicate waste category IDs are not allowed." });

        if (request.Rules.Any(rule => rule.PointsPerReport < 0 || rule.BonusQuality < 0))
            return BadRequest(new { message = "PointsPerReport and BonusQuality must be non-negative." });

        var requestedCategoryIds = request.Rules.Select(rule => rule.WasteCategoryId).ToList();

        var validCategoryIds = await _context.WasteCategories
            .Where(category => requestedCategoryIds.Contains(category.Id))
            .Select(category => category.Id)
            .ToListAsync();

        if (validCategoryIds.Count != requestedCategoryIds.Count)
            return BadRequest(new { message = "One or more waste categories are invalid." });

        var existingRules = await _context.RewardRules
            .Where(rule => rule.EnterpriseId == enterprise.Id && requestedCategoryIds.Contains(rule.WasteCategoryId))
            .ToListAsync();

        var existingByCategory = existingRules.ToDictionary(rule => rule.WasteCategoryId);
        var now = DateTime.UtcNow;

        foreach (var inputRule in request.Rules)
        {
            if (existingByCategory.TryGetValue(inputRule.WasteCategoryId, out var existingRule))
            {
                existingRule.PointsPerReport = inputRule.PointsPerReport;
                existingRule.BonusQuality = inputRule.BonusQuality;
                existingRule.IsActive = inputRule.IsActive;
            }
            else
            {
                _context.RewardRules.Add(new RewardRule
                {
                    Id = Guid.NewGuid(),
                    EnterpriseId = enterprise.Id,
                    WasteCategoryId = inputRule.WasteCategoryId,
                    PointsPerReport = inputRule.PointsPerReport,
                    BonusQuality = inputRule.BonusQuality,
                    IsActive = inputRule.IsActive
                });
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Reward rules updated successfully.",
            updatedCount = request.Rules.Count,
            updatedAt = now
        });
    }
}

public class UpdateEnterpriseRewardRulesRequest
{
    public List<UpdateEnterpriseRewardRuleItem> Rules { get; set; } = new();
}

public class UpdateEnterpriseRewardRuleItem
{
    public required int WasteCategoryId { get; set; }
    public required int PointsPerReport { get; set; }
    public required int BonusQuality { get; set; }
    public required bool IsActive { get; set; }
}
