namespace Service.Model;

public class LimitModel
{
    public string CampaignId { get; set; } = default!;
    public bool IsHidden { get; set; }
    public int CompletedCount { get; set; }
    public decimal TotalPoint { get; set; }

}