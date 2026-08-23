using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Ast
{
    /// <summary>A leaf node holding a literal string, e.g. the "hello" in ="hello".</summary>
    public sealed class TextLiteralNode : IExpressionNode
    {
        public string Value { get; }

        public TextLiteralNode(string value) => Value = value;

        public CellValue Evaluate(IEvaluationContext context) => CellValue.Text(Value);
    }
}
