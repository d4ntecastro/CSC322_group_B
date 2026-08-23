using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Ast
{
    /// <summary>A leaf node holding a literal number parsed straight out of the formula text, e.g. the "2" in =2+3.</summary>
    public sealed class NumberLiteralNode : IExpressionNode
    {
        public double Value { get; }

        public NumberLiteralNode(double value) => Value = value;

        public CellValue Evaluate(IEvaluationContext context) => CellValue.Number(Value);
    }
}
