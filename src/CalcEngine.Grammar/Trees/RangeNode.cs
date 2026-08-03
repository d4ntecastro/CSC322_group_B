using System.Collections.Generic;
using CalcEngine.Grammar.Values;

namespace CalcEngine.Grammar.Tree;

/// <summary>
/// A leaf node representing a cell range, e.g. "B2:B45" as used in
/// =SUM(B2:B45). A range has no single scalar value of its own — it only
/// means something as an argument to an aggregate function — so Evaluate()
/// deliberately returns an error rather than silently picking one cell.
/// This surfaces a malformed formula like "=B2:B45+1" as a clear error
/// value instead of an ambiguous guess.
///
/// Rep invariant: RangeReference is stored exactly as written (e.g.
/// "B2:B45"); expanding it into individual cell references is the
/// evaluation context's job (GetRangeValues), not this node's — this node
/// only needs to know it IS a range, not how to enumerate one.
/// </summary>
public sealed class RangeNode : IExpressionNode
{
    public string RangeReference { get; }

    public RangeNode(string rangeReference) => RangeReference = rangeReference;

    public CellValue Evaluate(IEvaluationContext context) =>
        CellValue.Error("#VALUE! — a range can't be used directly, only inside a function like SUM(...)");

    /// <summary>
    /// For dependency-graph purposes, a range depends on every cell inside
    /// it. We ask the context to expand the range into concrete references
    /// rather than parsing "B2:B45" ourselves here, since column/row
    /// expansion logic belongs with whoever owns cell addressing, not with
    /// the expression tree.
    /// </summary>
    public IEnumerable<string> GetCellReferences()
    {
        yield return RangeReference;
    }

    public override string ToString() => RangeReference;
}
