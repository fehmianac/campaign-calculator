using StackExchange.Redis;

namespace Service.Model;

public class CampaignModel
{
    public string Id { get; set; } = default!;
    public string? ParentId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime? StartDateUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }
    public int? CompletionLimit { get; set; }
    public bool IsRepeatable { get; set; }
    public bool IsHidden { get; set; }
    public bool IsActive { get; set; }
    public AwardModel? Award { get; set; }
    public List<RuleModel> Rules { get; set; } = new();
    public HashSet<string>? UserTags { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public string CreatedBy { get; set; } = default!;
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
    public ProgressModel? Progress { get; set; }
    public ProgressModel? ParentProgress { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsLimitReached { get; set; }


    public void SetProgress(List<ProgressModel> campaignProgresses, DateTime utcNow, CampaignModel? parentcampaign)
    {
        foreach (var progress in campaignProgresses)
        {
            if (progress.CampaignId.Equals(Id))
            {
                if (!IsHidden && progress.IsHidden)
                {
                    continue;
                }

                foreach (var rule in Rules)
                {
                    rule.SetProgress(progress.RuleProgresses, utcNow);
                }

                Progress = progress;
                return;
            }

            if (string.IsNullOrWhiteSpace(ParentId) || !progress.CampaignId.Equals(ParentId))
            {
                continue;
            }


            if (parentcampaign is { IsHidden: false } && progress.IsHidden)
            {
                continue;
            }

            ParentProgress = progress;
        }
    }

    public (int completed, int total) Check(DateTime utcNow, DateTime eventDateUtc, EventModel eventModel)
    {
        if (!IsActive)
        {
            return (-1, -1);
        }

        //This means campaign completed before this event, so we return.
        if (Progress is { CompletedAtUtc: > 0 })
        {
            return (-1, -1);
        }

        //Parent campaign has to be completed before this campaign.
        if (!string.IsNullOrWhiteSpace(ParentId) && (ParentProgress == null || ParentProgress.CompletedAtUtc == 0))
        {
            return (-1, -1);
        }

        if (StartDateUtc.HasValue && eventDateUtc <= StartDateUtc.Value)
        {
            return (-1, -1);
        }

        if (EndDateUtc.HasValue && eventDateUtc > EndDateUtc.Value)
        {
            return (-1, -1);
        }

        var totalNumberOfCall = Rules.Sum(q => q.NumberOfCall);
        var completedRuleCount = Rules.Count(rule => rule.Check(this, utcNow, eventModel));
        if (completedRuleCount != Rules.Count)
        {
            var completedNumberOfCall = Rules.Sum(q =>
            {
                if (q.Progress != null)
                {
                    return q.Progress.SatisfiedRuleModels.Count;
                }

                return 0;
            });

            return (completedNumberOfCall, totalNumberOfCall);
        }

        if (!IsRepeatable)
        {
            Progress!.CompletedAtUtc = eventModel.UtcTimeStamp;
            Progress!.IsModified = true;
        }

        foreach (var rule in Rules)
        {
            if (rule.Progress == null)
            {
                continue;
            }

            rule.Progress.IsStale = true;
        }

        IsCompleted = true;
        return (totalNumberOfCall, totalNumberOfCall);
    }

    public void CheckLimitReached(List<LimitModel> campaignLimitEntities)
    {
        if (CompletionLimit == null)
        {
            return;
        }

        if (CompletionLimit is 0)
        {
            IsLimitReached = true;
            return;
        }

        var campaignLimitEntity =
            campaignLimitEntities.FirstOrDefault(q => q.CampaignId == Id && q.IsHidden == IsHidden);
        if (campaignLimitEntity == null)
        {
            return;
        }

        if (CompletionLimit.HasValue && campaignLimitEntity.CompletedCount >= CompletionLimit.Value)
        {
            IsLimitReached = true;
        }
    }
}