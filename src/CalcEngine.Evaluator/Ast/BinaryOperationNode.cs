using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Ast
{
    /// <summary>
    /// An internal (non-leaf) node applying a binary operator to two child nodes,
    /// e.g. =A1+B1 is BinaryOperationNode(Add, CellReferenceNode(A1), CellReferenceNode(B1)).
    /// This is the "Composite" half of the tree: evaluating this node means evaluating
    /// its two children first.
    ///
    /// Error handling: if either child evaluates to an error, that error is returned
    /// immediately without evaluating the other side or applying the operator -- this
    /// is what makes an error propagate all the way up a formula (e.g. =A1+B1 shows
    /// #DIV/0! if A1 does) instead of the evaluator crashing or silently treating the
    /// error as zero.
    /// </summary>
    public sealed class BinaryOperationNode : IExpressionNode
    {
        private readonly BinaryOperator _operator;
        private readonly IExpressionNode _left;
        private readonly IExpressionNode _right;

        public BinaryOperationNode(BinaryOperator @operator, IExpressionNode left, IExpressionNode right)
        {
            _operator = @operator;
            _left = left;
            _right = right;
        }

        public CellValue Evaluate(IEvaluationContext context)
        {
            var left = context.Evaluate(_left);
            if (left.IsError) return left;

            var right = context.Evaluate(_right);
            if (right.IsError) return right;

            // Concatenation works on the *displayed* text of both sides regardless of
            // their kind, e.g. ="Total: "&5 -> "Total: 5", so it doesn't need numeric coercion.
            if (_operator == BinaryOperator.Concatenate)
                return CellValue.Text(left.ToString() + right.ToString());

            if (!left.TryCoerceToNumber(out var l) || !right.TryCoerceToNumber(out var r))
                return CellValue.Error(CellErrorType.InvalidValue);

            switch (_operator)
            {
                case BinaryOperator.Add:
                    return CellValue.Number(l + r);
                case BinaryOperator.Subtract:
                    return CellValue.Number(l - r);
                case BinaryOperator.Multiply:
                    return CellValue.Number(l * r);
                case BinaryOperator.Divide:
                    if (r == 0) return CellValue.Error(CellErrorType.DivideByZero);
                    return CellValue.Number(l / r);
                case BinaryOperator.Power:
                    return CellValue.Number(System.Math.Pow(l, r));
                case BinaryOperator.Equal:
                    return CellValue.Boolean(System.Math.Abs(l - r) < 1e-9);
                case BinaryOperator.NotEqual:
                    return CellValue.Boolean(System.Math.Abs(l - r) >= 1e-9);
                case BinaryOperator.LessThan:
                    return CellValue.Boolean(l < r);
                case BinaryOperator.LessOrEqual:
                    return CellValue.Boolean(l <= r);
                case BinaryOperator.GreaterThan:
                    return CellValue.Boolean(l > r);
                case BinaryOperator.GreaterOrEqual:
                    return CellValue.Boolean(l >= r);
                default:
                    return CellValue.Error(CellErrorType.InvalidValue);
            }
        }
    }
}
