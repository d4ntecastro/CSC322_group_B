using System.Collections.Generic;
using System.Linq;
using CalcEngine.Grammar.Values;

namespace CalcEngine.Grammar.Tree;

/// <summary>
/// A composite node with zero or more children, representing a function
/// call, e.g. "SUM(B2:B45)" or "IF(B2>10, "high", "low")".
///
/// Deliberately, this node does NOT know how SUM, AVERAGE, IF, etc. are
/// actually computed — it only knows the function's name and its
/// already-parsed argument nodes, and delegates the real work to
/// context.CallFunction(...). That keeps the function library itself
/// (built with the Strategy/Factory pattern) entirely inside the
/// Evaluator module, so this Grammar module never needs to change when a
/// new function is added or an existing one's algorithm changes.
///
/// Rep invariant: FunctionName is never null/empty; Arguments is never
/// null (an empty list is fine, e.g. a hypothetical zero-arg function, but
/// the list reference itself always exists).
/// </summary>
public sealed class FunctionCallNode : IExpressionNode
{
    public string FunctionName { get; }
    public IReadOnlyList<IExpressionNode> Arguments { get; }

    public FunctionCallNode(string functionName, IReadOnlyList<IExpressionNode> arguments)
    {
        FunctionName = functionName ?? throw new System.ArgumentNullException(nameof(functionName));
        Arguments = arguments ?? System.Array.Empty<IExpressionNode>();
    }

    public CellValue Evaluate(IEvaluationContext context) =>
        context.CallFunction(FunctionName, Arguments);

    public IEnumerable<string> GetCellReferences() =>
        Arguments.SelectMany(arg => arg.GetCellReferences());

    public override string ToString() =>
        $"{FunctionName}({string.Join(", ", Arguments.Select(a => a.ToString()))})";
}
