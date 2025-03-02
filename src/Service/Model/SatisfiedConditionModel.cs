namespace Service.Model;

public class SatisfiedConditionModel
{
    public string Key { get; set; } = default!;
    
    public MetadataValue? Value { get; set; }
    
    public bool IsSatisfied { get; set; }
}