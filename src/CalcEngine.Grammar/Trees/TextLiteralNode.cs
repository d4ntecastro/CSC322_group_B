using System.Collections.Generic;
using CalcEngine.Grammar.Values;

namespace CalcEngine.Grammar.Tree;

/// <summary>
/// A leaf node representing a quoted text literal, e.g. "high" in
/// =IF(B2>10, "high", "low").
///
/// Rep invariant: Value never contains the surrounding quote characters —
/// those are stripped once, at construction time (see
/// ExpressionTreeBuilder), so every consumer of this node gets the plain
/// text and never has to re-strip quotes itself.
/// </summary>
public sealed class TextLiteralNode : IExpressionNode
{
    public string Value { get; }

    public TextLiteralNode(string value) => Value = value ?? string.Empty;

    public CellValue Evaluate(IEvaluationContext context) => CellValue.Text(Value);

    public IEnumerable<string> GetCellReferences() => System.Array.Empty<string>();

    public override string ToString() => $"\"{Value}\"";
}
