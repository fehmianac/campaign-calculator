namespace Service.Model;

public class SatisfiedRuleModel
{
    public long Timestamp { get; set; }
    
    public string? EventId { get; set; }
    
    public List<SatisfiedConditionModel> SatisfiedConditionModels { get; set; } = new();
}