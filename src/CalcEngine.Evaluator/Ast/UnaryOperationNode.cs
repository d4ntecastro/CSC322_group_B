using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Ast
{
    /// <summary>An internal node applying a unary operator to a single child, e.g. =-A1 or =50%.</summary>
    public sealed class UnaryOperationNode : IExpressionNode
    {
        private readonly UnaryOperator _operator;
        private readonly IExpressionNode _operand;

        public UnaryOperationNode(UnaryOperator @operator, IExpressionNode operand)
        {
            _operator = @operator;
            _operand = operand;
        }

        public CellValue Evaluate(IEvaluationContext context)
        {
            var operand = context.Evaluate(_operand);
            if (operand.IsError) return operand;

            if (!operand.TryCoerceToNumber(out var value))
                return CellValue.Error(CellErrorType.InvalidValue);

            return _operator switch
            {
                UnaryOperator.Negate => CellValue.Number(-value),
                UnaryOperator.Percent => CellValue.Number(value / 100.0),
                _ => CellValue.Error(CellErrorType.InvalidValue)
            };
        }
    }
}
