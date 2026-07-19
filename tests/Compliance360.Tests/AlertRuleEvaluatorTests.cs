using Compliance360.Application.Notifications;

namespace Compliance360.Tests;

public sealed class AlertRuleEvaluatorTests
{
    [Fact]
    public void Evaluates_Grouped_And_Or_Not_Conditions_Deterministically()
    {
        const string condition =
            """
            {
              "type": "All",
              "children": [
                {
                  "type": "Compare",
                  "operator": "Equal",
                  "left": { "type": "VariableReference", "path": "entity.status" },
                  "right": { "type": "Constant", "value": "Submitted" }
                },
                {
                  "type": "Any",
                  "children": [
                    {
                      "type": "Compare",
                      "operator": "GreaterOrEqual",
                      "left": { "type": "VariableReference", "path": "entity.priority" },
                      "right": { "type": "Constant", "value": 3 }
                    },
                    {
                      "type": "Not",
                      "child": {
                        "type": "Exists",
                        "value": { "type": "VariableReference", "path": "entity.ownerId" }
                      }
                    }
                  ]
                }
              ]
            }
            """;
        const string payload = """{"entity":{"status":"Submitted","priority":4,"ownerId":"user-1"}}""";

        var result = new AlertRuleEvaluator().Evaluate(condition, payload);

        Assert.Equal(AlertTruthValue.True, result.Value);
        Assert.Equal(4, result.NodesEvaluated);
    }

    [Fact]
    public void Missing_Variable_Produces_Unknown()
    {
        const string condition =
            """
            {
              "type": "Compare",
              "operator": "Equal",
              "left": { "type": "VariableReference", "path": "entity.missing" },
              "right": { "type": "Constant", "value": "x" }
            }
            """;

        var result = new AlertRuleEvaluator().Evaluate(condition, """{"entity":{}}""");

        Assert.Equal(AlertTruthValue.Unknown, result.Value);
    }

    [Fact]
    public void Rejects_Unregistered_Executable_Node()
    {
        const string condition = """{"type":"Script","value":"return true;"}""";

        Assert.Throws<InvalidOperationException>(() => new AlertRuleEvaluator().Evaluate(condition, "{}"));
    }
}
