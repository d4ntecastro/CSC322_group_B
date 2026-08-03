using System.Collections.Generic;
using CalcEngine.Grammar.Values;

namespace CalcEngine.Grammar.Tree;

/// <summary>
/// A leaf node representing a literal number, e.g. the "2" and "3" in "=2+3".
///
/// Rep invariant: Value is fixed at construction and never changes —
/// literals are immutable by nature (the number 2 doesn't depend on
/// anything, so it can't need re-evaluation).
/// </summary>
public sealed class NumberLiteralNode : IExpressionNode
{
    public double Value { get; }

    public NumberLiteralNode(double value) => Value = value;

    public CellValue Evaluate(IEvaluationContext context) => CellValue.Number(Value);

    // A literal depends on no cells, so it contributes nothing to the
    // dependency graph.
    public IEnumerable<string> GetCellReferences() => System.Array.Empty<string>();

    public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
