using System.Collections.Generic;
using CalcEngine.Grammar.Values;

namespace CalcEngine.Grammar.Tree;

/// <summary>
/// A leaf node representing a reference to another cell, e.g. "B2" in "=B2+1".
///
/// Rep invariant: CellReference is stored exactly as written in the formula
/// (e.g. "B2", not normalised to lowercase) — normalisation, if the group
/// wants it, should happen once centrally (e.g. in the dependency graph),
/// not silently inside this node, so the same string always round-trips
/// identically between GetCellReferences() and Evaluate().
/// </summary>
public sealed class CellReferenceNode : IExpressionNode
{
    public string CellReference { get; }

    public CellReferenceNode(string cellReference) => CellReference = cellReference;

    public CellValue Evaluate(IEvaluationContext context) => context.GetCellValue(CellReference);

    public IEnumerable<string> GetCellReferences()
    {
        yield return CellReference;
    }

    public override string ToString() => CellReference;
}
