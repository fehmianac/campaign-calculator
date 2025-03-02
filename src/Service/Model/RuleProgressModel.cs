namespace Service.Model;

public class RuleProgressModel
{
    
    public string RuleId { get; set; } = null!;
    
    public List<SatisfiedRuleModel> SatisfiedRuleModels { get; set; } = new();

    public void AddSatisfiedRuleModel(long timestamp, string eventId, List<SatisfiedConditionModel> satisfiedModels)
    {
        SatisfiedRuleModels.Add(new SatisfiedRuleModel { Timestamp = timestamp, EventId = eventId, SatisfiedConditionModels = satisfiedModels });
    }
    
    public bool IsModified { get; set; }
    
    public bool IsStale { get; set; } = true;
}