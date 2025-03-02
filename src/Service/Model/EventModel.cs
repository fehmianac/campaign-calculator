using Service.Enum;

namespace Service.Model;

public class EventModel
{
    public string Id { get; set; } = default!;
    public string? ExternalId { get; set; }
    public string UserId { get; set; } = default!;
    public long UtcTimeStamp { get; set; }
    public DateTime UtcDate { get; set; }
    public string Event { get; set; } = default!;
    public Dictionary<string, MetadataValue> Metadata { get; set; } = new();
}

public class MetadataValue
{
    public DataType Type { get; set; }
    public string? StringValue { get; set; }
    public decimal? DecimalValue { get; set; }
    public bool? BooleanValue { get; set; }
    public DateTime? DateTimeValue { get; set; }
}