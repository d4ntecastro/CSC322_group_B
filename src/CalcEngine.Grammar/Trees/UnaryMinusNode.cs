using System.Collections.Generic;
using CalcEngine.Grammar.Values;

namespace CalcEngine.Grammar.Tree;

/// <summary>
/// A composite node with exactly one child, representing negation, e.g.
/// "-B2" or "-(A1+A2)".
///
/// Rep invariant: Operand is never null (enforced by constructor).
/// </summary>
public sealed class UnaryMinusNode : IExpressionNode
{
    public IExpressionNode Operand { get; }

    public UnaryMinusNode(IExpressionNode operand)
    {
        Operand = operand ?? throw new System.ArgumentNullException(nameof(operand));
    }

    public CellValue Evaluate(IEvaluationContext context)
    {
        var value = Operand.Evaluate(context);

        if (value.IsError) return value; // propagate, don't mask the underlying error

        if (value.Type != CellValueType.Number)
            return CellValue.Error("#VALUE! — cannot negate a non-numeric value");

        return CellValue.Number(-value.NumberValue);
    }

    public IEnumerable<string> GetCellReferences() => Operand.GetCellReferences();

    public override string ToString() => $"-{Operand}";
}
