using System.Globalization;
using System.Text.Json;

namespace Compliance360.Application.Notifications;

public enum AlertTruthValue
{
    False = 0,
    True = 1,
    Unknown = 2
}

public sealed record AlertRuleEvaluationResult(AlertTruthValue Value, int NodesEvaluated, string Explanation);

public interface IAlertRuleEvaluator
{
    AlertRuleEvaluationResult Evaluate(string conditionJson, string eventPayloadJson);
}

public sealed class AlertRuleEvaluator : IAlertRuleEvaluator
{
    private const int MaximumDepth = 20;
    private const int MaximumNodes = 500;
    private int _nodes;

    public AlertRuleEvaluationResult Evaluate(string conditionJson, string eventPayloadJson)
    {
        using var condition = JsonDocument.Parse(conditionJson, new JsonDocumentOptions { MaxDepth = MaximumDepth + 5 });
        using var payload = JsonDocument.Parse(eventPayloadJson, new JsonDocumentOptions { MaxDepth = 64 });
        _nodes = 0;
        var value = EvaluateNode(condition.RootElement, payload.RootElement, 0);
        return new AlertRuleEvaluationResult(value, _nodes, $"Condition evaluated to {value} using {_nodes} AST nodes.");
    }

    private AlertTruthValue EvaluateNode(JsonElement node, JsonElement payload, int depth)
    {
        if (depth > MaximumDepth)
        {
            throw new InvalidOperationException($"Alert condition exceeds maximum depth {MaximumDepth}.");
        }

        _nodes++;
        if (_nodes > MaximumNodes)
        {
            throw new InvalidOperationException($"Alert condition exceeds maximum node count {MaximumNodes}.");
        }

        var type = RequiredString(node, "type");
        return type switch
        {
            "All" => EvaluateAll(RequiredArray(node, "children"), payload, depth),
            "Any" => EvaluateAny(RequiredArray(node, "children"), payload, depth),
            "Not" => Negate(EvaluateNode(RequiredProperty(node, "child"), payload, depth + 1)),
            "Compare" => EvaluateComparison(node, payload),
            "Exists" => ResolveValue(RequiredProperty(node, "value"), payload).HasValue ? AlertTruthValue.True : AlertTruthValue.False,
            "IsNull" => EvaluateIsNull(node, payload),
            "IsEmpty" => EvaluateIsEmpty(node, payload),
            "Constant" => ReadBooleanConstant(node),
            _ => throw new InvalidOperationException($"Unsupported alert AST node '{type}'.")
        };
    }

    private AlertTruthValue EvaluateAll(JsonElement children, JsonElement payload, int depth)
    {
        var unknown = false;
        foreach (var child in children.EnumerateArray())
        {
            var result = EvaluateNode(child, payload, depth + 1);
            if (result == AlertTruthValue.False) return AlertTruthValue.False;
            unknown |= result == AlertTruthValue.Unknown;
        }

        return unknown ? AlertTruthValue.Unknown : AlertTruthValue.True;
    }

    private AlertTruthValue EvaluateAny(JsonElement children, JsonElement payload, int depth)
    {
        var unknown = false;
        foreach (var child in children.EnumerateArray())
        {
            var result = EvaluateNode(child, payload, depth + 1);
            if (result == AlertTruthValue.True) return AlertTruthValue.True;
            unknown |= result == AlertTruthValue.Unknown;
        }

        return unknown ? AlertTruthValue.Unknown : AlertTruthValue.False;
    }

    private static AlertTruthValue Negate(AlertTruthValue value)
    {
        return value switch
        {
            AlertTruthValue.True => AlertTruthValue.False,
            AlertTruthValue.False => AlertTruthValue.True,
            _ => AlertTruthValue.Unknown
        };
    }

    private static AlertTruthValue EvaluateComparison(JsonElement node, JsonElement payload)
    {
        var left = ResolveValue(RequiredProperty(node, "left"), payload);
        var right = ResolveValue(RequiredProperty(node, "right"), payload);
        if (!left.HasValue || !right.HasValue)
        {
            return AlertTruthValue.Unknown;
        }

        var operation = RequiredString(node, "operator");
        var result = operation switch
        {
            "Equal" => JsonEquals(left.Value, right.Value, ignoreCase: false),
            "NotEqual" => !JsonEquals(left.Value, right.Value, ignoreCase: false),
            "EqualsIgnoreCase" => JsonEquals(left.Value, right.Value, ignoreCase: true),
            "GreaterThan" => Compare(left.Value, right.Value) > 0,
            "GreaterOrEqual" => Compare(left.Value, right.Value) >= 0,
            "LessThan" => Compare(left.Value, right.Value) < 0,
            "LessOrEqual" => Compare(left.Value, right.Value) <= 0,
            "Contains" => Contains(left.Value, right.Value),
            "StartsWith" => StringValue(left.Value).StartsWith(StringValue(right.Value), StringComparison.OrdinalIgnoreCase),
            "EndsWith" => StringValue(left.Value).EndsWith(StringValue(right.Value), StringComparison.OrdinalIgnoreCase),
            "In" => In(left.Value, right.Value),
            "Before" => DateValue(left.Value) < DateValue(right.Value),
            "After" => DateValue(left.Value) > DateValue(right.Value),
            _ => throw new InvalidOperationException($"Unsupported comparison operator '{operation}'.")
        };
        return result ? AlertTruthValue.True : AlertTruthValue.False;
    }

    private static AlertTruthValue EvaluateIsNull(JsonElement node, JsonElement payload)
    {
        var value = ResolveValue(RequiredProperty(node, "value"), payload);
        return !value.HasValue || value.Value.ValueKind == JsonValueKind.Null
            ? AlertTruthValue.True
            : AlertTruthValue.False;
    }

    private static AlertTruthValue EvaluateIsEmpty(JsonElement node, JsonElement payload)
    {
        var value = ResolveValue(RequiredProperty(node, "value"), payload);
        if (!value.HasValue) return AlertTruthValue.Unknown;
        var isEmpty = value.Value.ValueKind switch
        {
            JsonValueKind.String => string.IsNullOrEmpty(value.Value.GetString()),
            JsonValueKind.Array => value.Value.GetArrayLength() == 0,
            JsonValueKind.Object => !value.Value.EnumerateObject().Any(),
            JsonValueKind.Null => true,
            _ => false
        };
        return isEmpty ? AlertTruthValue.True : AlertTruthValue.False;
    }

    private static AlertTruthValue ReadBooleanConstant(JsonElement node)
    {
        var value = RequiredProperty(node, "value");
        if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            return AlertTruthValue.Unknown;
        }

        return value.GetBoolean() ? AlertTruthValue.True : AlertTruthValue.False;
    }

    private static JsonElement? ResolveValue(JsonElement expression, JsonElement payload)
    {
        var type = RequiredString(expression, "type");
        if (type == "Constant")
        {
            return RequiredProperty(expression, "value");
        }

        if (type != "VariableReference")
        {
            throw new InvalidOperationException($"Value expression '{type}' is not allowed.");
        }

        var path = RequiredString(expression, "path");
        if (path.Length > 500 || path.Split('.').Length > 20)
        {
            throw new InvalidOperationException("Variable path exceeds configured limits.");
        }

        var current = payload;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out current))
            {
                return null;
            }
        }

        return current;
    }

    private static bool JsonEquals(JsonElement left, JsonElement right, bool ignoreCase)
    {
        if (left.ValueKind == JsonValueKind.String && right.ValueKind == JsonValueKind.String)
        {
            return string.Equals(left.GetString(), right.GetString(), ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }

        if (TryDecimal(left, out var leftNumber) && TryDecimal(right, out var rightNumber))
        {
            return leftNumber == rightNumber;
        }

        return string.Equals(left.GetRawText(), right.GetRawText(), StringComparison.Ordinal);
    }

    private static int Compare(JsonElement left, JsonElement right)
    {
        if (TryDecimal(left, out var leftNumber) && TryDecimal(right, out var rightNumber))
        {
            return leftNumber.CompareTo(rightNumber);
        }

        if (TryDate(left, out var leftDate) && TryDate(right, out var rightDate))
        {
            return leftDate.CompareTo(rightDate);
        }

        return string.Compare(StringValue(left), StringValue(right), StringComparison.OrdinalIgnoreCase);
    }

    private static bool Contains(JsonElement left, JsonElement right)
    {
        if (left.ValueKind == JsonValueKind.Array)
        {
            return left.EnumerateArray().Any(item => JsonEquals(item, right, ignoreCase: true));
        }

        return StringValue(left).Contains(StringValue(right), StringComparison.OrdinalIgnoreCase);
    }

    private static bool In(JsonElement left, JsonElement right)
    {
        if (right.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("The In operator requires an array on the right side.");
        }

        return right.EnumerateArray().Any(item => JsonEquals(left, item, ignoreCase: true));
    }

    private static string StringValue(JsonElement value)
    {
        return value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.GetRawText();
    }

    private static DateTimeOffset DateValue(JsonElement value)
    {
        return TryDate(value, out var parsed)
            ? parsed
            : throw new InvalidOperationException("Date comparison requires ISO-8601 date values.");
    }

    private static bool TryDate(JsonElement value, out DateTimeOffset parsed)
    {
        parsed = default;
        return value.ValueKind == JsonValueKind.String
            && DateTimeOffset.TryParse(value.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out parsed);
    }

    private static bool TryDecimal(JsonElement value, out decimal parsed)
    {
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out parsed))
        {
            return true;
        }

        parsed = default;
        return false;
    }

    private static JsonElement RequiredProperty(JsonElement node, string propertyName)
    {
        if (!node.TryGetProperty(propertyName, out var value))
        {
            throw new InvalidOperationException($"Alert AST property '{propertyName}' is required.");
        }

        return value;
    }

    private static JsonElement RequiredArray(JsonElement node, string propertyName)
    {
        var value = RequiredProperty(node, propertyName);
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException($"Alert AST property '{propertyName}' must be an array.");
        }

        return value;
    }

    private static string RequiredString(JsonElement node, string propertyName)
    {
        var value = RequiredProperty(node, propertyName);
        if (value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString()))
        {
            throw new InvalidOperationException($"Alert AST property '{propertyName}' must be a non-empty string.");
        }

        return value.GetString()!;
    }
}
