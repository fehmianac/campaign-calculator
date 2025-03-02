using Service.Model;

namespace Service;

public interface IRepository
{
    Task<List<CampaignModel>> GetCampaignsAsync(CancellationToken cancellationToken);
    Task<List<ProgressModel>> GetUserProgressAsync(string userId, CancellationToken cancellationToken);
    Task<List<LimitModel>> GetLimitsAsync(CancellationToken cancellationToken);
    Task SaveProgressAsync(List<ProgressModel> progressList, CancellationToken ctx);
    Task SaveProgressHistoryAsync(List<ProgressHistoryModel> progressHistoryList, CancellationToken ctx);
    Task SaveCampaignLimitsAsync(List<LimitModel> campaignLimit, CancellationToken ctx);
}

public class Repository : IRepository
{
    public Task<List<CampaignModel>> GetCampaignsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(new List<CampaignModel>());
    }

    public Task<List<ProgressModel>> GetUserProgressAsync(string userId, CancellationToken cancellationToken)
    {
        return Task.FromResult(new List<ProgressModel>());
    }

    public Task<List<LimitModel>> GetLimitsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(new List<LimitModel>());
    }

    public Task SaveProgressAsync(List<ProgressModel> progressList, CancellationToken ctx)
    {
        return Task.CompletedTask;
    }

    public Task SaveProgressHistoryAsync(List<ProgressHistoryModel> progressHistoryList, CancellationToken ctx)
    {
        return Task.CompletedTask;
    }

    public Task SaveCampaignLimitsAsync(List<LimitModel> campaignLimit, CancellationToken ctx)
    {
        return Task.CompletedTask;
    }
}