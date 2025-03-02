using System.Globalization;
using Service.Model;

namespace Service;

public class CampaignService
{
    private readonly IRepository _repository;

    public CampaignService(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CampaignModel>> CalculateProgress(EventModel eventModel, string userId, CancellationToken ctx)
    {
        var utcNow = DateTime.UtcNow;
        var campaigns = await _repository.GetCampaignsAsync(ctx);
        if (!campaigns.Any())
        {
            return new List<CampaignModel>();
        }

        var campaignLimit = await _repository.GetLimitsAsync(ctx);
        var campaignProgresses = await _repository.GetUserProgressAsync(userId, ctx);


        foreach (var campaign in campaigns)
        {
            campaign.CheckLimitReached(campaignLimit);
        }

        campaigns.RemoveAll(q => q.IsLimitReached);

        if (!campaigns.Any())
            return new List<CampaignModel>();
        
        campaignLimit.Clear(); //We will use same list for insert

        foreach (var campaign in campaigns)
        {
            var parent = campaign.ParentId != null ? campaigns.FirstOrDefault(q => q.Id == campaign.ParentId) : null;
            campaign.SetProgress(campaignProgresses, utcNow, parent);
        }

        var progressHistoryList = new List<ProgressHistoryModel>();
        foreach (var campaign in campaigns)
        {
            (int completed, int total) = campaign.Check(utcNow, eventModel.UtcDate, eventModel);
            if (completed != -1 && campaign is { IsRepeatable: false} && campaign.Rules.Any(q => q.HasProgressed))
            {
                progressHistoryList.Add(new ProgressHistoryModel
                {
                    UserId = userId,
                    CampaignId = campaign.Id,
                    IsHidden = campaign.IsHidden,
                    Completed = completed,
                    Total = total,
                    IsCompleted = campaign.IsCompleted
                });
            }
        }

        var progressList = new List<ProgressModel>();

        foreach (var campaign in campaigns)
        {
            if (campaign.Progress != null)
            {
                campaign.Progress.RuleProgresses.Clear();
                foreach (var rule in campaign.Rules)
                {
                    if (rule.Progress != null)
                    {
                        campaign.Progress.RuleProgresses.Add(rule.Progress);
                    }
                }

                if (campaign.Progress.RuleProgresses.RemoveAll(q => q.IsStale) > 0)
                {
                    campaign.Progress.IsModified = true;
                }

                if (campaign.Progress.IsModified || campaign.Progress.RuleProgresses.Any(q => q.IsModified))
                {
                    progressList.Add(campaign.Progress);
                }
            }

            if (!campaign.IsCompleted)
            {
                continue;
            }

            if (campaign.CompletionLimit.HasValue)
            {
                campaignLimit.Add(new LimitModel()
                {
                    CampaignId = campaign.Id,
                    IsHidden = campaign.IsHidden,
                    CompletedCount = 1
                });
            }
        }
        
      
        var progressedCampaigns = campaigns.Where(c => c.Rules.Any(r => r.HasProgressed)).ToList();
        //TODO select one of them
        
        await _repository.SaveProgressAsync(progressList, ctx);
        await _repository.SaveProgressHistoryAsync(progressHistoryList, ctx);
        await _repository.SaveCampaignLimitsAsync(campaignLimit, ctx);
        
        return campaigns.Where(q=> q.IsCompleted).ToList();
    }
}