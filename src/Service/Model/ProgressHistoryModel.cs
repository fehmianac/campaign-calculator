namespace Service.Model;

public class ProgressHistoryModel
{
    public string UserId { get; set; } = default!;
    public string CampaignId { get; set; } = default!;
    public bool IsHidden { get; set; }
    public int Completed { get; set; }
    public int Total { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}