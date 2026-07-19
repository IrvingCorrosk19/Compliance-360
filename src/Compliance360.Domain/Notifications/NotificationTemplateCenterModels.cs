using Compliance360.Domain.Common;

namespace Compliance360.Domain.Notifications;

public enum NotificationTemplateLifecycle
{
    Draft = 0,
    Review = 1,
    Approved = 2,
    Published = 3,
    Retired = 4,
    Archived = 5
}

public sealed class NotificationTemplateVersion : TenantEntity
{
    private NotificationTemplateVersion()
    {
        Locale = string.Empty;
        Subject = string.Empty;
        HtmlBody = string.Empty;
        VariablesJson = "[]";
    }

    public NotificationTemplateVersion(
        Guid tenantId,
        Guid notificationTemplateId,
        int version,
        string locale,
        string subject,
        string htmlBody,
        string? textBody,
        string variablesJson,
        string? brandingJson,
        Guid createdByUserId,
        DateTimeOffset createdAtUtc)
        : base(tenantId)
    {
        NotificationTemplateId = Guard.AgainstEmpty(notificationTemplateId, nameof(notificationTemplateId));
        Version = Guard.AgainstOutOfRange(version, nameof(version), 1, 100_000);
        Locale = Guard.AgainstNullOrWhiteSpace(locale, nameof(locale), 20);
        Subject = Guard.AgainstNullOrWhiteSpace(subject, nameof(subject), 250);
        HtmlBody = Guard.AgainstNullOrWhiteSpace(htmlBody, nameof(htmlBody), 64_000);
        TextBody = string.IsNullOrWhiteSpace(textBody) ? null : Guard.AgainstNullOrWhiteSpace(textBody, nameof(textBody), 16_000);
        VariablesJson = Guard.AgainstNullOrWhiteSpace(variablesJson, nameof(variablesJson), 16_000);
        BrandingJson = string.IsNullOrWhiteSpace(brandingJson) ? null : Guard.AgainstNullOrWhiteSpace(brandingJson, nameof(brandingJson), 16_000);
        CreatedByUserId = Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));
        CreatedAtUtc = createdAtUtc;
        Lifecycle = NotificationTemplateLifecycle.Draft;
    }

    public Guid NotificationTemplateId { get; private set; }

    public int Version { get; private set; }

    public string Locale { get; private set; }

    public string Subject { get; private set; }

    public string HtmlBody { get; private set; }

    public string? TextBody { get; private set; }

    public string VariablesJson { get; private set; }

    public string? BrandingJson { get; private set; }

    public NotificationTemplateLifecycle Lifecycle { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public Guid? ReviewedByUserId { get; private set; }

    public Guid? ApprovedByUserId { get; private set; }

    public Guid? PublishedByUserId { get; private set; }

    public DateTimeOffset? SubmittedForReviewAtUtc { get; private set; }

    public DateTimeOffset? ReviewedAtUtc { get; private set; }

    public DateTimeOffset? ApprovedAtUtc { get; private set; }

    public DateTimeOffset? PublishedAtUtc { get; private set; }

    public DateTimeOffset? RetiredAtUtc { get; private set; }

    public DateTimeOffset? ArchivedAtUtc { get; private set; }

    public void SubmitForReview(DateTimeOffset nowUtc)
    {
        EnsureState(NotificationTemplateLifecycle.Draft);
        Lifecycle = NotificationTemplateLifecycle.Review;
        SubmittedForReviewAtUtc = nowUtc;
        MarkUpdated(nowUtc);
    }

    public void RecordReview(Guid reviewerUserId, DateTimeOffset nowUtc)
    {
        EnsureState(NotificationTemplateLifecycle.Review);
        reviewerUserId = Guard.AgainstEmpty(reviewerUserId, nameof(reviewerUserId));
        if (reviewerUserId == CreatedByUserId)
        {
            throw new DomainException("Template maker cannot review the same version.");
        }

        ReviewedByUserId = reviewerUserId;
        ReviewedAtUtc = nowUtc;
        MarkUpdated(nowUtc);
    }

    public void Approve(Guid approverUserId, DateTimeOffset nowUtc)
    {
        EnsureState(NotificationTemplateLifecycle.Review);
        approverUserId = Guard.AgainstEmpty(approverUserId, nameof(approverUserId));
        if (!ReviewedByUserId.HasValue)
        {
            throw new DomainException("Template version must be reviewed before approval.");
        }

        if (approverUserId == CreatedByUserId)
        {
            throw new DomainException("Template maker cannot approve the same version.");
        }

        ApprovedByUserId = approverUserId;
        ApprovedAtUtc = nowUtc;
        Lifecycle = NotificationTemplateLifecycle.Approved;
        MarkUpdated(nowUtc);
    }

    public void Publish(Guid publisherUserId, DateTimeOffset nowUtc)
    {
        EnsureState(NotificationTemplateLifecycle.Approved);
        PublishedByUserId = Guard.AgainstEmpty(publisherUserId, nameof(publisherUserId));
        PublishedAtUtc = nowUtc;
        Lifecycle = NotificationTemplateLifecycle.Published;
        MarkUpdated(nowUtc);
    }

    public void Retire(DateTimeOffset nowUtc)
    {
        EnsureState(NotificationTemplateLifecycle.Published);
        RetiredAtUtc = nowUtc;
        Lifecycle = NotificationTemplateLifecycle.Retired;
        MarkUpdated(nowUtc);
    }

    public void Archive(DateTimeOffset nowUtc)
    {
        if (Lifecycle is not (NotificationTemplateLifecycle.Draft or NotificationTemplateLifecycle.Retired))
        {
            throw new DomainException("Only draft or retired template versions can be archived.");
        }

        ArchivedAtUtc = nowUtc;
        Lifecycle = NotificationTemplateLifecycle.Archived;
        MarkUpdated(nowUtc);
    }

    private void EnsureState(NotificationTemplateLifecycle expected)
    {
        if (Lifecycle != expected)
        {
            throw new DomainException($"Template version must be {expected} for this action.");
        }
    }
}
