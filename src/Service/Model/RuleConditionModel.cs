using Service.Enum;

namespace Service.Model;


public class RuleConditionModel
{
    public string Key { get; set; } = default!;
    public Operator Operator { get; set; }
    public List<string> Value { get; set; } = new();
    public DataType DataType { get; set; }
    public bool ValueMustBeUnique { get; set; }

    public SatisfiedConditionModel IsSatisfied(EventModel eventModel, IEnumerable<SatisfiedConditionModel>? satisfiedModels)
    {
        if (!Value.Any())
        {
            return CreateSatisfiedModel(false);
        }

        if (!eventModel.Metadata.TryGetValue(Key, out var dataValue))
        {
            return CreateSatisfiedModel(false);
        }

        if (DataType.Equals(DataType.Boolean))
        {
            return IsBoolConditionSatisfied(dataValue);
        }

        if (DataType.Equals(DataType.DateTime))
        {
            return IsDateTimeConditionSatisfied(dataValue);
        }

        if (DataType.Equals(DataType.Numeric))
        {
            return IsNumericConditionSatisfied(dataValue);
        }

        if (DataType.Equals(DataType.String))
        {
            return IsStringConditionSatisfied(dataValue, satisfiedModels);
        }

        return CreateSatisfiedModel(false);
    }

    private SatisfiedConditionModel IsBoolConditionSatisfied(MetadataValue dataValue)
    {
        if (!dataValue.BooleanValue.HasValue)
        {
            return CreateSatisfiedModel(false);
        }

        var strValue = Value.First();
        if (!bool.TryParse(strValue, out var value))
        {
            return CreateSatisfiedModel(false);
        }

        switch (Operator)
        {
            case Operator.Equals:
                return CreateSatisfiedModel(dataValue.BooleanValue.Value == value);
            case Operator.NotEquals:
                return CreateSatisfiedModel(dataValue.BooleanValue.Value != value);
            case Operator.GreaterThan:
            case Operator.GreaterThanOrEquals:
            case Operator.LessThan:
            case Operator.LessThanOrEquals:
            case Operator.StringStartsWith:
            case Operator.StringEndsWith:
            case Operator.StringContains:
            case Operator.ExistInList:
            case Operator.NotExistInList:
            default:
                return CreateSatisfiedModel(false);
        }
    }

    private SatisfiedConditionModel IsDateTimeConditionSatisfied(MetadataValue dataValue)
    {
        if (!dataValue.DateTimeValue.HasValue)
        {
            return CreateSatisfiedModel(false);
        }

        var strValue = Value.First();
        if (!DateTimeOffset.TryParse(strValue, out var value))
        {
            return CreateSatisfiedModel(false);
        }

        switch (Operator)
        {
            case Operator.Equals:
                return CreateSatisfiedModel(dataValue.DateTimeValue.Value == value.UtcDateTime);
            case Operator.NotEquals:
                return CreateSatisfiedModel(dataValue.DateTimeValue.Value != value.UtcDateTime);
            case Operator.GreaterThan:
                return CreateSatisfiedModel(dataValue.DateTimeValue.Value > value.UtcDateTime);
            case Operator.GreaterThanOrEquals:
                return CreateSatisfiedModel(dataValue.DateTimeValue.Value >= value.UtcDateTime);
            case Operator.LessThan:
                return CreateSatisfiedModel(dataValue.DateTimeValue.Value < value.UtcDateTime);
            case Operator.LessThanOrEquals:
                return CreateSatisfiedModel(dataValue.DateTimeValue.Value <= value.UtcDateTime);
            case Operator.StringStartsWith:
            case Operator.StringEndsWith:
            case Operator.StringContains:
            case Operator.ExistInList:
            case Operator.NotExistInList:
            default:
                return CreateSatisfiedModel(false);
        }
    }

    private SatisfiedConditionModel IsNumericConditionSatisfied(MetadataValue dataValue)
    {
        if (!dataValue.DecimalValue.HasValue)
        {
            return CreateSatisfiedModel(false);
        }

        var strValue = Value.First();
        if (!decimal.TryParse(strValue, out var value))
        {
            return CreateSatisfiedModel(false);
        }

        switch (Operator)
        {
            case Operator.Equals:
                return CreateSatisfiedModel(dataValue.DecimalValue.Value == value);
            case Operator.NotEquals:
                return CreateSatisfiedModel(dataValue.DecimalValue.Value != value);
            case Operator.GreaterThan:
                return CreateSatisfiedModel(dataValue.DecimalValue.Value > value);
            case Operator.GreaterThanOrEquals:
                return CreateSatisfiedModel(dataValue.DecimalValue.Value >= value);
            case Operator.LessThan:
                return CreateSatisfiedModel(dataValue.DecimalValue.Value < value);
            case Operator.LessThanOrEquals:
                return CreateSatisfiedModel(dataValue.DecimalValue.Value <= value);
            case Operator.StringStartsWith:
            case Operator.StringEndsWith:
            case Operator.StringContains:
            case Operator.ExistInList:
            case Operator.NotExistInList:
            default:
                return CreateSatisfiedModel(false);
        }
    }

    private SatisfiedConditionModel IsStringConditionSatisfied(MetadataValue dataValue, IEnumerable<SatisfiedConditionModel>? satisfiedModels)
    {
        if (string.IsNullOrWhiteSpace(dataValue.StringValue))
        {
            return CreateSatisfiedModel(false);
        }

        bool isSatisfied;

        switch (Operator)
        {
            case Operator.Equals:
                return CreateSatisfiedModel(dataValue.StringValue.Equals(Value.First()));
            case Operator.NotEquals:
                return CreateSatisfiedModel(!dataValue.StringValue.Equals(Value.First()));
            case Operator.StringStartsWith:
                return CreateSatisfiedModel(dataValue.StringValue.StartsWith(Value.First()));
            case Operator.StringEndsWith:
                return CreateSatisfiedModel(dataValue.StringValue.EndsWith(Value.First()));
            case Operator.StringContains:
                return CreateSatisfiedModel(dataValue.StringValue.Contains(Value.First()));
            case Operator.ExistInList:
                isSatisfied = Value.Contains(dataValue.StringValue);
                if (ValueMustBeUnique && satisfiedModels != null)
                {
                    return CreateSatisfiedModel(isSatisfied && !IsMetadataValueUsedBefore(satisfiedModels, dataValue.StringValue), dataValue);
                }

                return CreateSatisfiedModel(isSatisfied, dataValue);
            case Operator.NotExistInList:
                isSatisfied = !Value.Contains(dataValue.StringValue);
                if (ValueMustBeUnique && satisfiedModels != null)
                {
                    return CreateSatisfiedModel(isSatisfied && !IsMetadataValueUsedBefore(satisfiedModels, dataValue.StringValue), dataValue);
                }

                return CreateSatisfiedModel(isSatisfied, dataValue);
            case Operator.GreaterThan:
            case Operator.GreaterThanOrEquals:
            case Operator.LessThan:
            case Operator.LessThanOrEquals:
            default:
                return CreateSatisfiedModel(false);
        }
    }

    private bool IsMetadataValueUsedBefore(IEnumerable<SatisfiedConditionModel> satisfiedModels, string stringValue)
    {
        var satisfiedConditionModels = satisfiedModels.Where(q => q.Key == Key);
        foreach (var satisfiedConditionModel in satisfiedConditionModels)
        {
            if (satisfiedConditionModel.Value?.StringValue == stringValue)
            {
                return true;
            }
        }

        return false;
    }

    private SatisfiedConditionModel CreateSatisfiedModel(bool isSatisfied, MetadataValue? value = null)
    {
        return new SatisfiedConditionModel
        {
            Key = Key,
            IsSatisfied = isSatisfied,
            Value = value
        };
    }
}