// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);

using System.Text.Json.Serialization;

namespace VersionCheckerApi.Controllers.Models;

public class MessageData
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("html")]
    public string? Html { get; set; }

    [JsonPropertyName("markdown")]
    public string? Markdown { get; set; }
}

public class DetailedMessage
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("html")]
    public string? Html { get; set; }

    [JsonPropertyName("markdown")]
    public string? Markdown { get; set; }
}

public class ProjectData
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("visibility")]
    public string? Visibility { get; set; }

    [JsonPropertyName("lastUpdateTime")]
    public DateTime? LastUpdateTime { get; set; }
}

public class RepositoryData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("project")]
    public ProjectData? Project { get; set; }

    [JsonPropertyName("defaultBranch")]
    public string? DefaultBranch { get; set; }

    [JsonPropertyName("remoteUrl")]
    public string? RemoteUrl { get; set; }
}

public class CreatedBy
{
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("uniqueName")]
    public string? UniqueName { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }
}

public class LastMergeSourceCommit
{
    [JsonPropertyName("commitId")]
    public string? CommitId { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class LastMergeTargetCommit
{
    [JsonPropertyName("commitId")]
    public string? CommitId { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class LastMergeCommit
{
    [JsonPropertyName("commitId")]
    public string? CommitId { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class Reviewer
{
    [JsonPropertyName("reviewerUrl")]
    public string? ReviewerUrl { get; set; }

    [JsonPropertyName("vote")]
    public int? Vote { get; set; }

    [JsonPropertyName("displayName")]
    public string?DisplayName { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("uniqueName")]
    public string? UniqueName { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("isContainer")]
    public bool? IsContainer { get; set; }
}

public class Commit
{
    [JsonPropertyName("commitId")]
    public string? CommitId { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class Web
{
    [JsonPropertyName("href")]
    public string? Href { get; set; }
}

public class Statuses
{
    [JsonPropertyName("href")]
    public string? Href { get; set; }
}

public class Links
{
    [JsonPropertyName("web")]
    public Web? Web { get; set; }

    [JsonPropertyName("statuses")]
    public Statuses? Statuses { get; set; }
}

public class Resource
{
    [JsonPropertyName("repository")]
    public RepositoryData Repository { get; set; }

    [JsonPropertyName("pullRequestId")]
    public int? PullRequestId { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("createdBy")]
    public CreatedBy? CreatedBy { get; set; }

    [JsonPropertyName("creationDate")]
    public DateTime? CreationDate { get; set; }

    [JsonPropertyName("closedDate")]
    public DateTime? ClosedDate { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("sourceRefName")]
    public string? SourceRefName { get; set; }

    [JsonPropertyName("targetRefName")]
    public string? TargetRefName { get; set; }

    [JsonPropertyName("mergeStatus")]
    public string? MergeStatus { get; set; }

    [JsonPropertyName("mergeId")]
    public string? MergeId { get; set; }

    [JsonPropertyName("lastMergeSourceCommit")]
    public LastMergeSourceCommit? LastMergeSourceCommit { get; set; }

    [JsonPropertyName("lastMergeTargetCommit")]
    public LastMergeTargetCommit? LastMergeTargetCommit { get; set; }

    [JsonPropertyName("lastMergeCommit")]
    public LastMergeCommit? LastMergeCommit { get; set; }

    [JsonPropertyName("reviewers")]
    public List<Reviewer>? Reviewers { get; set; }

    [JsonPropertyName("commits")]
    public List<Commit>? Commits { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("_links")]
    public Links? Links { get; set; }
}

public class Collection
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public class Account
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public class Project
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public class ResourceContainers
{
    [JsonPropertyName("collection")]
    public Collection? Collection { get; set; }

    [JsonPropertyName("account")]
    public Account? Account { get; set; }

    [JsonPropertyName("project")]
    public Project? Project { get; set; }
}

public class DevopsMergeHook
{
    [JsonPropertyName("subscriptionId")]
    public string? SubscriptionId { get; set; }

    [JsonPropertyName("notificationId")]
    public int? NotificationId { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("eventType")]
    public string? EventType { get; set; }

    [JsonPropertyName("publisherId")]
    public string? PublisherId { get; set; }

    [JsonPropertyName("message")]
    public MessageData? Message { get; set; }

    [JsonPropertyName("detailedMessage")]
    public DetailedMessage? DetailedMessage { get; set; }

    [JsonPropertyName("resource")]
    public Resource? Resource { get; set; }

    [JsonPropertyName("resourceVersion")]
    public string? ResourceVersion { get; set; }

    [JsonPropertyName("resourceContainers")]
    public ResourceContainers? ResourceContainers { get; set; }

    [JsonPropertyName("createdDate")]
    public DateTime? CreatedDate { get; set; }
}