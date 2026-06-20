namespace Compliance360.Domain.Common;

public abstract class Entity
{
    protected Entity()
    {
        Id = Guid.NewGuid();
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; protected set; }

    public DateTimeOffset CreatedAtUtc { get; protected set; }

    public DateTimeOffset? UpdatedAtUtc { get; protected set; }

    public void MarkUpdated(DateTimeOffset updatedAtUtc)
    {
        UpdatedAtUtc = updatedAtUtc;
    }
}

public abstract class TenantEntity : Entity, ITenantScoped
{
    protected TenantEntity()
    {
    }

    protected TenantEntity(Guid tenantId)
    {
        TenantId = Guard.AgainstEmpty(tenantId, nameof(tenantId));
    }

    public Guid TenantId { get; protected set; }
}

public interface ITenantScoped
{
    Guid TenantId { get; }
}

public sealed class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}

public static class Guard
{
    public static Guid AgainstEmpty(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException($"{parameterName} cannot be empty.");
        }

        return value;
    }

    public static string AgainstNullOrWhiteSpace(string? value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{parameterName} is required.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"{parameterName} cannot exceed {maxLength} characters.");
        }

        return trimmed;
    }

    public static int AgainstOutOfRange(int value, string parameterName, int minValue, int maxValue)
    {
        if (value < minValue || value > maxValue)
        {
            throw new DomainException($"{parameterName} must be between {minValue} and {maxValue}.");
        }

        return value;
    }
}
