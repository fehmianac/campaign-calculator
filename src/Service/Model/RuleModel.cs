using System.Text.Json.Serialization;
using Service.Enum;

namespace Service.Model;

public class RuleModel
{
    public string Id { get; set; } = null!;

    public string Event { get; set; } = null!;

    public int NumberOfCall { get; set; }

    public TimeWindow TimeWindow { get; set; }

    public int TimeWindowCount { get; set; }

    public List<RuleConditionModel> Conditions { get; set; } = new();
    
    [JsonIgnore]
    public RuleProgressModel? Progress { get; set; }
    
    [JsonIgnore]
    public bool HasProgressed { get; set; }

    public void SetProgress(List<RuleProgressModel> progresses, DateTime utcNow)
    {
        foreach (var progress in progresses)
        {
            if (progress.RuleId.Equals(Id))
            {
                progress.IsStale = false;

                //Clearing timestamps which are not relevant anymore.
                if (TimeWindow != TimeWindow.None)
                {
                    var minTimeStamp = GetMinimumTimeStamp(utcNow, TimeWindow, TimeWindowCount);
                    progress.IsModified = progress.SatisfiedRuleModels.RemoveAll(q => q.Timestamp < minTimeStamp) > 0;
                }

                Progress = progress;
            }
        }
    }

    public bool Check(CampaignModel campaign, DateTime utcNow, EventModel eventModel)
    {
        //Order of the code is important this needs to be first check.
        //This means rule completed before this event, so we return true.
        if (Progress != null && Progress.SatisfiedRuleModels.Count == NumberOfCall)
        {
            return true;
        }

        if (!Event.Equals(eventModel.Event))
        {
            return false;
        }

        var minTimeStamp = GetMinimumTimeStamp(utcNow, TimeWindow, TimeWindowCount);

        if (TimeWindow != TimeWindow.None && eventModel.UtcTimeStamp < minTimeStamp)
        {
            return false;
        }

 
        bool allSatisfied = true;
        var satisfiedConditionModels = new List<SatisfiedConditionModel>();
        foreach (var condition in Conditions)
        {
            var satisfiedModel = condition.IsSatisfied(eventModel, Progress?.SatisfiedRuleModels.SelectMany(q => q.SatisfiedConditionModels));
            if (satisfiedModel is { IsSatisfied: true, Value: not null })
            {
                satisfiedConditionModels.Add(satisfiedModel);
            }

            allSatisfied = satisfiedModel.IsSatisfied;

            if (!allSatisfied)
            {
                break;
            }
        }

        if (allSatisfied)
        {
            campaign.Progress ??= new ProgressModel(eventModel.UserId, campaign.Id, campaign.IsHidden)
            {
                IsModified = true
            };

            Progress ??= new RuleProgressModel
            {
                RuleId = Id
            };

            Progress.AddSatisfiedRuleModel(eventModel.UtcTimeStamp, eventModel.Id, satisfiedConditionModels);
            Progress.IsModified = true;
            Progress.IsStale = false;
            HasProgressed = true;
            return Progress.SatisfiedRuleModels.Count == NumberOfCall;
        }

        return false;
    }

    private static long GetMinimumTimeStamp(DateTimeOffset utcDate, TimeWindow timeWindow, int timeWindowCount)
    {
        switch (timeWindow)
        {
            case TimeWindow.Hours:
                return utcDate.AddHours(-timeWindowCount).ToUnixTimeMilliseconds();
            case TimeWindow.Days:
                return utcDate.AddDays(-timeWindowCount).ToUnixTimeMilliseconds();
            case TimeWindow.Months:
                return utcDate.AddMonths(-timeWindowCount).ToUnixTimeMilliseconds();
            default:
                return 0;
        }
    }
}