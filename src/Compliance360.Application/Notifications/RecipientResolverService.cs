using System.Text.Json;
using System.Text.Json.Serialization;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Notifications;
using Compliance360.Shared;

namespace Compliance360.Application.Notifications;

public sealed class RecipientResolverService : IRecipientResolverService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IRecipientResolverRepository _repository;
    private readonly INotificationAuditService _audit;
    private readonly IClock _clock;

    public RecipientResolverService(IRecipientResolverRepository repository, INotificationAuditService audit, IClock clock)
    {
        _repository = repository;
        _audit = audit;
        _clock = clock;
    }

    public async Task<Result<RecipientPreview>> PreviewAsync(PreviewRecipientsCommand command, CancellationToken cancellationToken = default)
    {
        var version = await _repository.GetVersionAsync(command.TenantId, command.DefinitionId, command.VersionId, cancellationToken);
        if (version is null)
        {
            return Result<RecipientPreview>.Failure("Alert definition version not found.");
        }

        RecipientRule[] rules;
        try
        {
            rules = JsonSerializer.Deserialize<RecipientRule[]>(version.RecipientRulesJson, JsonOptions) ?? [];
        }
        catch (JsonException exception)
        {
            return Result<RecipientPreview>.Failure($"Recipient rules are invalid: {exception.Message}");
        }

        var resolved = new List<ResolvedRecipient>();
        var warnings = new List<string>();
        foreach (var rule in rules)
        {
            await ResolveRuleAsync(command, rule, resolved, warnings, cancellationToken);
        }

        await ApplyPreferencesAsync(command.TenantId, command.Channel, resolved, warnings, cancellationToken);
        var fallbackApplied = false;
        if (resolved.Count == 0)
        {
            var fallback = await _repository.GetFallbackAsync(command.TenantId, cancellationToken);
            if (fallback is not null && fallback.Mode != RecipientFallbackMode.None && fallback.TargetId.HasValue)
            {
                fallbackApplied = true;
                await ResolveFallbackAsync(command, fallback, resolved, warnings, cancellationToken);
                await ApplyPreferencesAsync(command.TenantId, command.Channel, resolved, warnings, cancellationToken);
            }
        }

        var deduplicated = resolved
            .GroupBy(item => item.Address, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderBy(item => item.Routing).First())
            .OrderBy(item => item.Address, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        await _audit.AppendAsync(command.TenantId, command.RequestedByUserId, nameof(AlertDefinitionVersion), version.Id, AuditAction.Viewed, true, null, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return Result<RecipientPreview>.Success(new RecipientPreview(
            version.Id,
            deduplicated.Where(item => item.Routing == RecipientRouting.To).ToArray(),
            deduplicated.Where(item => item.Routing == RecipientRouting.Cc).ToArray(),
            deduplicated.Where(item => item.Routing == RecipientRouting.Bcc).ToArray(),
            warnings,
            fallbackApplied));
    }

    public Task<Result> SetPreferenceAsync(SetRecipientPreferenceCommand command, CancellationToken cancellationToken = default) =>
        PersistAsync(command.TenantId, command.RequestedByUserId, async () =>
        {
            var preference = await _repository.GetPreferenceAsync(command.TenantId, command.RequestedByUserId, command.Channel, cancellationToken);
            if (preference is null)
            {
                await _repository.AddAsync(new NotificationPreference(command.TenantId, command.RequestedByUserId, command.Channel, command.Enabled), cancellationToken);
            }
            else
            {
                preference.SetEnabled(command.Enabled, _clock.UtcNow);
            }
        }, cancellationToken);

    public async Task<Result<RecipientDirectorySummary>> GetDirectoryAsync(
        Guid tenantId,
        Guid requestedByUserId,
        CancellationToken cancellationToken = default)
    {
        var groups = await _repository.ListGroupsAsync(tenantId, cancellationToken);
        var departments = await _repository.ListDepartmentsAsync(tenantId, cancellationToken);
        var external = await _repository.ListExternalRecipientsAsync(tenantId, cancellationToken);
        var lists = await _repository.ListDistributionListsAsync(tenantId, cancellationToken);
        var fallback = await _repository.GetFallbackAsync(tenantId, cancellationToken);
        await _audit.AppendAsync(tenantId, requestedByUserId, "RecipientDirectory", tenantId, AuditAction.Viewed, true, null, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return Result<RecipientDirectorySummary>.Success(new RecipientDirectorySummary(
            groups.Select(item => new RecipientDirectoryItem(item.Id, item.Name)).ToArray(),
            departments.Select(item => new RecipientDirectoryItem(item.Id, item.Name)).ToArray(),
            external.Select(item => new RecipientDirectoryItem(item.Id, item.DisplayName, item.Email, item.IsActive)).ToArray(),
            lists.Select(item => new RecipientDirectoryItem(item.Id, item.Name)).ToArray(),
            fallback is null ? null : new RecipientFallbackSummary(fallback.Mode, fallback.TargetId, fallback.Routing)));
    }

    public async Task<Result<Guid>> CreateGroupAsync(CreateRecipientGroupCommand command, CancellationToken cancellationToken = default)
    {
        var entity = new RecipientGroup(command.TenantId, command.Name);
        return await PersistEntityAsync(command.TenantId, command.RequestedByUserId, entity, cancellationToken);
    }

    public Task<Result> AddGroupMemberAsync(AddRecipientGroupMemberCommand command, CancellationToken cancellationToken = default) =>
        PersistAsync(command.TenantId, command.RequestedByUserId, () => _repository.AddAsync(new RecipientGroupMember(command.TenantId, command.GroupId, command.UserId), cancellationToken), cancellationToken);

    public async Task<Result<Guid>> CreateDepartmentAsync(CreateRecipientDepartmentCommand command, CancellationToken cancellationToken = default)
    {
        var entity = new RecipientDepartment(command.TenantId, command.Name);
        return await PersistEntityAsync(command.TenantId, command.RequestedByUserId, entity, cancellationToken);
    }

    public Task<Result> SetDirectoryProfileAsync(SetRecipientDirectoryProfileCommand command, CancellationToken cancellationToken = default) =>
        PersistAsync(command.TenantId, command.RequestedByUserId, async () =>
        {
            var profile = await _repository.GetProfileAsync(command.TenantId, command.UserId, cancellationToken);
            if (profile is null)
            {
                await _repository.AddAsync(new RecipientDirectoryProfile(command.TenantId, command.UserId, command.DepartmentId, command.SupervisorUserId), cancellationToken);
            }
            else
            {
                profile.Update(command.DepartmentId, command.SupervisorUserId, _clock.UtcNow);
            }
        }, cancellationToken);

    public async Task<Result<Guid>> AuthorizeExternalAsync(AuthorizeExternalRecipientCommand command, CancellationToken cancellationToken = default)
    {
        var entity = new AuthorizedExternalRecipient(command.TenantId, command.Email, command.DisplayName);
        return await PersistEntityAsync(command.TenantId, command.RequestedByUserId, entity, cancellationToken);
    }

    public async Task<Result<Guid>> CreateDistributionListAsync(CreateRecipientDistributionListCommand command, CancellationToken cancellationToken = default)
    {
        var entity = new RecipientDistributionList(command.TenantId, command.Name);
        return await PersistEntityAsync(command.TenantId, command.RequestedByUserId, entity, cancellationToken);
    }

    public Task<Result> AddDistributionListMemberAsync(AddRecipientDistributionListMemberCommand command, CancellationToken cancellationToken = default) =>
        PersistAsync(command.TenantId, command.RequestedByUserId, () => _repository.AddAsync(
            new RecipientDistributionListMember(command.TenantId, command.DistributionListId, command.UserId, command.ExternalRecipientId), cancellationToken), cancellationToken);

    public Task<Result> SetFallbackAsync(SetRecipientFallbackCommand command, CancellationToken cancellationToken = default) =>
        PersistAsync(command.TenantId, command.RequestedByUserId, async () =>
        {
            var fallback = await _repository.GetFallbackAsync(command.TenantId, cancellationToken);
            if (fallback is null)
            {
                await _repository.AddAsync(new RecipientFallbackConfiguration(command.TenantId, command.Mode, command.TargetId, command.Routing), cancellationToken);
            }
            else
            {
                fallback.Set(command.Mode, command.TargetId, command.Routing, _clock.UtcNow);
            }
        }, cancellationToken);

    private async Task ResolveRuleAsync(
        PreviewRecipientsCommand command,
        RecipientRule rule,
        List<ResolvedRecipient> recipients,
        List<string> warnings,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<RecipientUserRecord> users = [];
        switch (rule.Kind)
        {
            case RecipientKind.Owner:
            case RecipientKind.Creator:
            case RecipientKind.Responsible:
            case RecipientKind.Reviewer:
            case RecipientKind.Approver:
            case RecipientKind.Submitter:
                if (command.RelationshipUsers.TryGetValue(rule.Kind, out var userId) && userId.HasValue)
                {
                    users = await _repository.GetUsersAsync(command.TenantId, [userId.Value], cancellationToken);
                }
                break;
            case RecipientKind.Supervisor:
                var supervisedUserId = ResolveSupervisorSubject(command, rule.Value);
                if (supervisedUserId.HasValue)
                {
                    var supervisor = await _repository.GetSupervisorAsync(command.TenantId, supervisedUserId.Value, cancellationToken);
                    users = supervisor is null ? [] : [supervisor];
                }
                break;
            case RecipientKind.Role:
                if (TryGuid(rule.Value, rule.Kind, warnings, out var roleId))
                    users = await _repository.GetUsersByRoleAsync(command.TenantId, roleId, cancellationToken);
                break;
            case RecipientKind.Group:
                if (TryGuid(rule.Value, rule.Kind, warnings, out var groupId))
                    users = await _repository.GetUsersByGroupAsync(command.TenantId, groupId, cancellationToken);
                break;
            case RecipientKind.Department:
                if (TryGuid(rule.Value, rule.Kind, warnings, out var departmentId))
                    users = await _repository.GetUsersByDepartmentAsync(command.TenantId, departmentId, cancellationToken);
                break;
            case RecipientKind.ExplicitUser:
                if (TryGuid(rule.Value, rule.Kind, warnings, out var explicitUserId))
                    users = await _repository.GetUsersAsync(command.TenantId, [explicitUserId], cancellationToken);
                break;
            case RecipientKind.ExternalEmail:
                if (!string.IsNullOrWhiteSpace(rule.Value))
                {
                    var external = await _repository.GetExternalAsync(command.TenantId, rule.Value, cancellationToken);
                    if (external is null)
                        warnings.Add($"External recipient '{rule.Value}' is not authorized for this tenant.");
                    else
                        recipients.Add(new ResolvedRecipient(null, external.Email, external.DisplayName, rule.Routing, rule.Kind, true));
                }
                break;
            case RecipientKind.DistributionList:
                if (TryGuid(rule.Value, rule.Kind, warnings, out var listId))
                {
                    var members = await _repository.GetDistributionListAsync(command.TenantId, listId, cancellationToken);
                    recipients.AddRange(members.Select(member => new ResolvedRecipient(member.UserId, member.Email, member.DisplayName, rule.Routing, rule.Kind, member.IsExternal)));
                }
                break;
            case RecipientKind.Subscription:
                var topic = string.IsNullOrWhiteSpace(rule.Value) ? command.Topic : rule.Value;
                if (!string.IsNullOrWhiteSpace(topic))
                {
                    var subscribers = await _repository.GetSubscriptionsAsync(command.TenantId, topic, command.Channel, cancellationToken);
                    recipients.AddRange(subscribers.Select(member => new ResolvedRecipient(member.UserId, member.Email, member.DisplayName, rule.Routing, rule.Kind, member.IsExternal)));
                }
                break;
        }

        recipients.AddRange(users.Select(user => new ResolvedRecipient(user.UserId, user.Email, user.DisplayName, rule.Routing, rule.Kind, false)));
        if (users.Count == 0 && rule.Kind is not RecipientKind.ExternalEmail and not RecipientKind.DistributionList and not RecipientKind.Subscription)
        {
            warnings.Add($"Rule '{rule.Kind}' resolved no active tenant users.");
        }
    }

    private async Task ResolveFallbackAsync(
        PreviewRecipientsCommand command,
        RecipientFallbackConfiguration fallback,
        List<ResolvedRecipient> recipients,
        List<string> warnings,
        CancellationToken cancellationToken)
    {
        var kind = fallback.Mode switch
        {
            RecipientFallbackMode.User => RecipientKind.ExplicitUser,
            RecipientFallbackMode.Role => RecipientKind.Role,
            RecipientFallbackMode.Group => RecipientKind.Group,
            RecipientFallbackMode.Department => RecipientKind.Department,
            RecipientFallbackMode.DistributionList => RecipientKind.DistributionList,
            _ => RecipientKind.ExplicitUser
        };
        await ResolveRuleAsync(command, new RecipientRule(kind, fallback.Routing, fallback.TargetId?.ToString()), recipients, warnings, cancellationToken);
    }

    private async Task ApplyPreferencesAsync(Guid tenantId, NotificationChannel channel, List<ResolvedRecipient> recipients, List<string> warnings, CancellationToken cancellationToken)
    {
        var userIds = recipients.Where(item => item.UserId.HasValue).Select(item => item.UserId!.Value).Distinct().ToArray();
        if (userIds.Length == 0)
            return;

        var preferences = await _repository.GetPreferencesAsync(tenantId, userIds, channel, cancellationToken);
        var disabled = recipients.Where(item => item.UserId.HasValue && preferences.TryGetValue(item.UserId.Value, out var enabled) && !enabled).ToArray();
        foreach (var recipient in disabled)
        {
            recipients.Remove(recipient);
            warnings.Add($"User '{recipient.UserId}' disabled the {channel} channel.");
        }
    }

    private static Guid? ResolveSupervisorSubject(PreviewRecipientsCommand command, string? value)
    {
        if (Enum.TryParse<RecipientKind>(value, true, out var relationship)
            && command.RelationshipUsers.TryGetValue(relationship, out var related))
            return related;
        return command.RelationshipUsers.TryGetValue(RecipientKind.Supervisor, out var explicitSupervisor) ? explicitSupervisor : null;
    }

    private static bool TryGuid(string? value, RecipientKind kind, List<string> warnings, out Guid id)
    {
        if (Guid.TryParse(value, out id))
            return true;
        warnings.Add($"Rule '{kind}' requires a valid target identifier.");
        return false;
    }

    private async Task<Result<Guid>> PersistEntityAsync(Guid tenantId, Guid userId, TenantEntity entity, CancellationToken cancellationToken)
    {
        var result = await PersistAsync(tenantId, userId, () => _repository.AddAsync(entity, cancellationToken), cancellationToken);
        return result.IsSuccess ? Result<Guid>.Success(entity.Id) : Result<Guid>.Failure(result.Error!);
    }

    private async Task<Result> PersistAsync(Guid tenantId, Guid userId, Func<Task> mutation, CancellationToken cancellationToken)
    {
        try
        {
            await mutation();
            await _audit.AppendAsync(tenantId, userId, "RecipientResolver", userId, AuditAction.AlertConfigurationChanged, true, null, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }
}
