namespace Service.Model;

public class ProgressModel
{
    public ProgressModel(string userId, string campaignId, bool isHidden)
    {
        
        CampaignId = campaignId;
        IsHidden = isHidden;
        UserId = userId;
    }

    public string UserId { get; set; }
    
    public string CampaignId { get; set; } = default!;

    
    public long CompletedAtUtc { get; set; }

    
    public bool IsHidden { get; set; }
    
    
    public List<RuleProgressModel> RuleProgresses { get; set; } = new();

    
    public bool IsModified { get; set; }
}