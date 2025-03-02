using AutoFixture;
using Moq;
using Service.Enum;
using Service.Model;
using Xunit;

namespace Service.Tests;

public class CampaignServiceTests
{
    private readonly IFixture _fixture;
    private Mock<IRepository> _mockRepository;
    private readonly CampaignService _campaignService;

    public CampaignServiceTests()
    {
        _fixture = new Fixture();
        _mockRepository = new Mock<IRepository>();
        _campaignService = new CampaignService(_mockRepository.Object);
    }

    [Fact]
    public async Task Checked_Campaign_Usage_Limit()
    {
        var cancellationToken = CancellationToken.None;
        var campaignModel = _fixture.Create<CampaignModel>();
        campaignModel.CompletionLimit = 100;
        var campaigns = new List<CampaignModel>
        {
            campaignModel
        };

        _mockRepository.Setup(q => q.GetCampaignsAsync(cancellationToken)).ReturnsAsync(campaigns);
        _mockRepository.Setup(q => q.GetLimitsAsync(cancellationToken)).ReturnsAsync(new List<LimitModel>
        {
            new LimitModel
            {
                CampaignId = campaignModel.Id,
                CompletedCount = 100
            }
        });

        var eventModel = _fixture.Create<EventModel>();
        var userModel = _fixture.Create<string>();

        var campaignsResponse = await _campaignService.CalculateProgress(eventModel, userModel, cancellationToken);

        Assert.Empty(campaignsResponse);
    }

    [Fact]
    public async Task Checked_Campaign_Progressed()
    {
        var cancellationToken = CancellationToken.None;
        var campaignModel = _fixture.Create<CampaignModel>();
        campaignModel.CompletionLimit = 100;
        campaignModel.StartDateUtc = DateTime.UtcNow.AddHours(-1);
        campaignModel.EndDateUtc = DateTime.UtcNow.AddHours(1);
        campaignModel.IsActive = true;
        campaignModel.IsLimitReached = false;
        campaignModel.ParentProgress = null;
        campaignModel.Progress = null;
        campaignModel.ParentId = null;
        campaignModel.Rules =
        [
            new RuleModel
            {
                NumberOfCall = 1,
                Event = "payment",
                Conditions = new List<RuleConditionModel>
                {
                    new RuleConditionModel
                    {
                        Key = "payment_method",
                        Operator = Operator.Equals,
                        Value = new List<string> { "wallet" }
                    }
                }
            }
        ];
        var campaigns = new List<CampaignModel>
        {
            campaignModel
        };

        _mockRepository.Setup(q => q.GetCampaignsAsync(cancellationToken)).ReturnsAsync(campaigns);
        _mockRepository.Setup(q => q.GetLimitsAsync(cancellationToken)).ReturnsAsync(new List<LimitModel>
        {
            new LimitModel
            {
                CampaignId = campaignModel.Id,
                CompletedCount = 99
            }
        });
        _mockRepository.Setup(q => q.GetUserProgressAsync(It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(new List<ProgressModel>());
        var eventModel = _fixture.Create<EventModel>();
        eventModel.Event = "payment";
        eventModel.UtcDate = DateTime.UtcNow;
        eventModel.Metadata = new Dictionary<string, MetadataValue>
        {
            {
                "payment_method", new MetadataValue
                {
                    Type = DataType.String, StringValue = "wallet"
                }
            }
        };
        var userModel = _fixture.Create<string>();

        var campaignsResponse = await _campaignService.CalculateProgress(eventModel, userModel, cancellationToken);

        Assert.Equal(campaignModel.Id, campaignsResponse.First().Id);
    }

    [Fact]
    public async Task Checked_Campaign_Progressed_With_2_Step_Should_Empty()
    {
        var cancellationToken = CancellationToken.None;
        var campaignModel = _fixture.Create<CampaignModel>();
        campaignModel.CompletionLimit = 100;
        campaignModel.StartDateUtc = DateTime.UtcNow.AddHours(-1);
        campaignModel.EndDateUtc = DateTime.UtcNow.AddHours(1);
        campaignModel.IsActive = true;
        campaignModel.IsLimitReached = false;
        campaignModel.ParentProgress = null;
        campaignModel.Progress = null;
        campaignModel.ParentId = null;
        campaignModel.Rules =
        [
            new RuleModel
            {
                NumberOfCall = 2,
                Event = "payment",
                Conditions = new List<RuleConditionModel>
                {
                    new RuleConditionModel
                    {
                        Key = "payment_method",
                        Operator = Operator.Equals,
                        Value = new List<string> { "wallet" }
                    }
                }
            }
        ];
        var campaigns = new List<CampaignModel>
        {
            campaignModel
        };

        _mockRepository.Setup(q => q.GetCampaignsAsync(cancellationToken)).ReturnsAsync(campaigns);
        _mockRepository.Setup(q => q.GetLimitsAsync(cancellationToken)).ReturnsAsync(new List<LimitModel>
        {
            new LimitModel
            {
                CampaignId = campaignModel.Id,
                CompletedCount = 99
            }
        });
        _mockRepository.Setup(q => q.GetUserProgressAsync(It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(new List<ProgressModel>
            {
            });
        var eventModel = _fixture.Create<EventModel>();
        eventModel.Event = "payment";
        eventModel.UtcDate = DateTime.UtcNow;
        eventModel.Metadata = new Dictionary<string, MetadataValue>
        {
            {
                "payment_method", new MetadataValue
                {
                    Type = DataType.String, StringValue = "wallet"
                }
            }
        };
        var userModel = _fixture.Create<string>();

        var campaignsResponse = await _campaignService.CalculateProgress(eventModel, userModel, cancellationToken);

        Assert.Empty(campaignsResponse);
    }


    [Fact]
    public async Task Checked_Campaign_Progressed_With_2_Step()
    {
        var cancellationToken = CancellationToken.None;
        var campaignModel = _fixture.Create<CampaignModel>();
        campaignModel.CompletionLimit = 100;
        campaignModel.StartDateUtc = DateTime.UtcNow.AddHours(-1);
        campaignModel.EndDateUtc = DateTime.UtcNow.AddHours(1);
        campaignModel.IsActive = true;
        campaignModel.IsLimitReached = false;
        campaignModel.ParentProgress = null;
        campaignModel.Progress = null;
        campaignModel.ParentId = null;
        campaignModel.Rules =
        [
            new RuleModel
            {
                Id = Guid.NewGuid().ToString(),
                NumberOfCall = 2,
                Event = "payment",
                Conditions = new List<RuleConditionModel>
                {
                    new RuleConditionModel
                    {
                        Key = "payment_method",
                        Operator = Operator.Equals,
                        Value = new List<string> { "wallet" }
                    }
                }
            }
        ];
        var campaigns = new List<CampaignModel>
        {
            campaignModel
        };

        _mockRepository.Setup(q => q.GetCampaignsAsync(cancellationToken)).ReturnsAsync(campaigns);
        _mockRepository.Setup(q => q.GetLimitsAsync(cancellationToken)).ReturnsAsync(new List<LimitModel>
        {
            new LimitModel
            {
                CampaignId = campaignModel.Id,
                CompletedCount = 99
            }
        });
        var userModel = _fixture.Create<string>();

        _mockRepository.Setup(q => q.GetUserProgressAsync(It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(new List<ProgressModel>
            {
                new ProgressModel(userModel, campaigns.First().Id, false)
                {
                    CompletedAtUtc = 0,
                    RuleProgresses = new List<RuleProgressModel>
                    {
                        new RuleProgressModel
                        {
                            RuleId = campaigns.First().Rules.First().Id,
                            SatisfiedRuleModels = new List<SatisfiedRuleModel>
                            {
                                new SatisfiedRuleModel
                                {
                                    EventId = Guid.NewGuid().ToString()
                                }
                            }
                        }
                    },
                    IsModified = false
                }
            });
        var eventModel = _fixture.Create<EventModel>();
        eventModel.Event = "payment";
        eventModel.UtcDate = DateTime.UtcNow;
        eventModel.Metadata = new Dictionary<string, MetadataValue>
        {
            {
                "payment_method", new MetadataValue
                {
                    Type = DataType.String, StringValue = "wallet"
                }
            }
        };

        var campaignsResponse = await _campaignService.CalculateProgress(eventModel, userModel, cancellationToken);

        Assert.NotEmpty(campaignsResponse);
    }
    
    
     [Fact]
    public async Task Checked_Campaign_Progressed_With_2_Step_With_TimeWindow_Should_Empty()
    {
        var cancellationToken = CancellationToken.None;
        var campaignModel = _fixture.Create<CampaignModel>();
        campaignModel.CompletionLimit = 100;
        campaignModel.StartDateUtc = DateTime.UtcNow.AddHours(-1);
        campaignModel.EndDateUtc = DateTime.UtcNow.AddHours(1);
        campaignModel.IsActive = true;
        campaignModel.IsLimitReached = false;
        campaignModel.ParentProgress = null;
        campaignModel.Progress = null;
        campaignModel.ParentId = null;
        campaignModel.Rules =
        [
            new RuleModel
            {
                Id = Guid.NewGuid().ToString(),
                NumberOfCall = 2,
                Event = "payment",
                Conditions = new List<RuleConditionModel>
                {
                    new RuleConditionModel
                    {
                        Key = "payment_method",
                        Operator = Operator.Equals,
                        Value = new List<string> { "wallet" }
                    }
                },
                TimeWindow = TimeWindow.Days,
                TimeWindowCount = 10
            }
        ];
        var campaigns = new List<CampaignModel>
        {
            campaignModel
        };

        _mockRepository.Setup(q => q.GetCampaignsAsync(cancellationToken)).ReturnsAsync(campaigns);
        _mockRepository.Setup(q => q.GetLimitsAsync(cancellationToken)).ReturnsAsync(new List<LimitModel>
        {
            new LimitModel
            {
                CampaignId = campaignModel.Id,
                CompletedCount = 99
            }
        });
        var userModel = _fixture.Create<string>();

        _mockRepository.Setup(q => q.GetUserProgressAsync(It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(new List<ProgressModel>
            {
                new ProgressModel(userModel, campaigns.First().Id, false)
                {
                    CompletedAtUtc = 0,
                    RuleProgresses = new List<RuleProgressModel>
                    {
                        new RuleProgressModel
                        {
                            RuleId = campaigns.First().Rules.First().Id,
                            SatisfiedRuleModels = new List<SatisfiedRuleModel>
                            {
                                new SatisfiedRuleModel
                                {
                                    EventId = Guid.NewGuid().ToString(),
                                    Timestamp = 10
                                },
                            }
                        }
                    },
                    IsModified = false
                }
            });
        var eventModel = _fixture.Create<EventModel>();
        eventModel.Event = "payment";
        eventModel.UtcDate = DateTime.UtcNow;
        eventModel.Metadata = new Dictionary<string, MetadataValue>
        {
            {
                "payment_method", new MetadataValue
                {
                    Type = DataType.String, StringValue = "wallet"
                }
            }
        };

        var campaignsResponse = await _campaignService.CalculateProgress(eventModel, userModel, cancellationToken);

        Assert.Empty(campaignsResponse);
    }
    
    
        [Fact]
    public async Task Checked_Campaign_Progressed_With_2_Step_With_TimeWindow()
    {
        var cancellationToken = CancellationToken.None;
        var campaignModel = _fixture.Create<CampaignModel>();
        campaignModel.CompletionLimit = 100;
        campaignModel.StartDateUtc = DateTime.UtcNow.AddHours(-1);
        campaignModel.EndDateUtc = DateTime.UtcNow.AddHours(1);
        campaignModel.IsActive = true;
        campaignModel.IsLimitReached = false;
        campaignModel.ParentProgress = null;
        campaignModel.Progress = null;
        campaignModel.ParentId = null;
        var utc = DateTimeOffset.UtcNow;
        campaignModel.Rules =
        [
            
            new RuleModel
            {
                Id = Guid.NewGuid().ToString(),
                NumberOfCall = 2,
                Event = "payment",
                Conditions = new List<RuleConditionModel>
                {
                    new RuleConditionModel
                    {
                        Key = "payment_method",
                        Operator = Operator.Equals,
                        Value = new List<string> { "wallet" }
                    }
                },
                TimeWindow = TimeWindow.Days,
                TimeWindowCount = 11
            }
        ];
        var campaigns = new List<CampaignModel>
        {
            campaignModel
        };

        _mockRepository.Setup(q => q.GetCampaignsAsync(cancellationToken)).ReturnsAsync(campaigns);
        _mockRepository.Setup(q => q.GetLimitsAsync(cancellationToken)).ReturnsAsync(new List<LimitModel>
        {
            new LimitModel
            {
                CampaignId = campaignModel.Id,
                CompletedCount = 99
            }
        });
        var userModel = _fixture.Create<string>();

        _mockRepository.Setup(q => q.GetUserProgressAsync(It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(new List<ProgressModel>
            {
                new ProgressModel(userModel, campaigns.First().Id, false)
                {
                    CompletedAtUtc = 0,
                    RuleProgresses = new List<RuleProgressModel>
                    {
                        new RuleProgressModel
                        {
                            RuleId = campaigns.First().Rules.First().Id,
                            SatisfiedRuleModels = new List<SatisfiedRuleModel>
                            {
                                new SatisfiedRuleModel
                                {
                                    EventId = Guid.NewGuid().ToString(),
                                    Timestamp =utc.AddDays(-10).ToUnixTimeMilliseconds()
                                },
                            }
                        }
                    },
                    IsModified = false
                }
            });
        var eventModel = _fixture.Create<EventModel>();
        eventModel.Event = "payment";
        eventModel.UtcDate = utc.DateTime;
        eventModel.UtcTimeStamp = utc.ToUnixTimeMilliseconds();
        eventModel.Metadata = new Dictionary<string, MetadataValue>
        {
            {
                "payment_method", new MetadataValue
                {
                    Type = DataType.String, StringValue = "wallet"
                }
            }
        };

        var campaignsResponse = await _campaignService.CalculateProgress(eventModel, userModel, cancellationToken);

        Assert.NotEmpty(campaignsResponse);
    }
}


