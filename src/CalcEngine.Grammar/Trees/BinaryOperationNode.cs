using System.Collections.Generic;
using System.Linq;
using CalcEngine.Grammar.Values;

namespace CalcEngine.Grammar.Tree;

/// <summary>
/// The set of binary operators the grammar recognises. Arithmetic and
/// comparison operators are handled by the same node type since both share
/// the same shape (two operands, one symbol, one combining rule) — the
/// only difference is what "combine" means, handled in the switch below.
/// </summary>
public enum BinaryOperator
{
    Add, Subtract, Multiply, Divide, Power,
    Equal, NotEqual, LessThan, GreaterThan, LessOrEqual, GreaterOrEqual
}

/// <summary>
/// A composite node with two children, representing one binary operation,
/// e.g. "B2 + C3" or "B2 > 10".
///
/// Rep invariant: Left and Right are never null (enforced by constructor).
/// Operator is fixed at construction — a node never changes what operation
/// it performs, only re-evaluates its (possibly changed) operands.
/// </summary>
public sealed class BinaryOperationNode : IExpressionNode
{
    public IExpressionNode Left { get; }
    public IExpressionNode Right { get; }
    public BinaryOperator Operator { get; }

    public BinaryOperationNode(IExpressionNode left, BinaryOperator op, IExpressionNode right)
    {
        Left = left ?? throw new System.ArgumentNullException(nameof(left));
        Right = right ?? throw new System.ArgumentNullException(nameof(right));
        Operator = op;
    }

    public CellValue Evaluate(IEvaluationContext context)
    {
        var left = Left.Evaluate(context);
        if (left.IsError) return left;

        var right = Right.Evaluate(context);
        if (right.IsError) return right;

        return Operator switch
        {
            BinaryOperator.Add => Arithmetic(left, right, (a, b) => a + b),
            BinaryOperator.Subtract => Arithmetic(left, right, (a, b) => a - b),
            BinaryOperator.Multiply => Arithmetic(left, right, (a, b) => a * b),
            BinaryOperator.Divide => Divide(left, right),
            BinaryOperator.Power => Arithmetic(left, right, System.Math.Pow),

            BinaryOperator.Equal => Compare(left, right, (a, b) => a == b, (a, b) => a == b),
            BinaryOperator.NotEqual => Compare(left, right, (a, b) => a != b, (a, b) => a != b),
            BinaryOperator.LessThan => NumericCompare(left, right, (a, b) => a < b),
            BinaryOperator.GreaterThan => NumericCompare(left, right, (a, b) => a > b),
            BinaryOperator.LessOrEqual => NumericCompare(left, right, (a, b) => a <= b),
            BinaryOperator.GreaterOrEqual => NumericCompare(left, right, (a, b) => a >= b),

            _ => CellValue.Error("#VALUE! — unrecognised operator")
        };
    }

    private static CellValue Arithmetic(CellValue left, CellValue right, System.Func<double, double, double> op)
    {
        if (left.Type != CellValueType.Number || right.Type != CellValueType.Number)
            return CellValue.Error("#VALUE! — arithmetic requires numeric operands");

        return CellValue.Number(op(left.NumberValue, right.NumberValue));
    }

    private static CellValue Divide(CellValue left, CellValue right)
    {
        if (left.Type != CellValueType.Number || right.Type != CellValueType.Number)
            return CellValue.Error("#VALUE! — arithmetic requires numeric operands");

        if (right.NumberValue == 0)
            return CellValue.Error("#DIV/0!");

        return CellValue.Number(left.NumberValue / right.NumberValue);
    }

    private static CellValue NumericCompare(CellValue left, CellValue right, System.Func<double, double, bool> op)
    {
        if (left.Type != CellValueType.Number || right.Type != CellValueType.Number)
            return CellValue.Error("#VALUE! — comparison requires numeric operands");

        return CellValue.Boolean(op(left.NumberValue, right.NumberValue));
    }

    /// <summary>
    /// Equality/inequality is allowed across matching types (number-to-number
    /// or text-to-text); comparing a number to text is a type error rather
    /// than silently returning false, per the spec's requirement to surface
    /// type errors explicitly.
    /// </summary>
    private static CellValue Compare(
        CellValue left, CellValue right,
        System.Func<double, double, bool> numberOp,
        System.Func<string, string, bool> textOp)
    {
        if (left.Type == CellValueType.Number && right.Type == CellValueType.Number)
            return CellValue.Boolean(numberOp(left.NumberValue, right.NumberValue));

        if (left.Type == CellValueType.Text && right.Type == CellValueType.Text)
            return CellValue.Boolean(textOp(left.TextValue, right.TextValue));

        return CellValue.Error("#VALUE! — cannot compare values of different types");
    }

    public IEnumerable<string> GetCellReferences() =>
        Left.GetCellReferences().Concat(Right.GetCellReferences());

    public override string ToString() => $"({Left} {Operator} {Right})";
}
