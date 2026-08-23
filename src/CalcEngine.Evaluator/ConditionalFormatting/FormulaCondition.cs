using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.ConditionalFormatting
{
    /// <summary>
    /// The general case: the rule's condition is an arbitrary formula (e.g. one that
    /// references other cells), and it's satisfied whenever that formula evaluates to
    /// a truthy, non-error value. This reuses the exact same IExpressionNode tree and
    /// IEvaluationContext used for ordinary cell formulas -- conditional formatting
    /// doesn't need its own mini evaluator.
    /// </summary>
    public sealed class FormulaCondition : IConditionalFormatCondition
    {
        private readonly IExpressionNode _formula;

        public FormulaCondition(IExpressionNode formula) => _formula = formula;

        public bool IsSatisfiedBy(CellValue value, IEvaluationContext context)
        {
            var result = context.Evaluate(_formula);
            return !result.IsError && result.TryCoerceToBoolean(out var flag) && flag;
        }
    }
}
