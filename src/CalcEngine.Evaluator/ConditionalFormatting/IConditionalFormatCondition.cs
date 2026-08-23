using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.ConditionalFormatting
{
    /// <summary>
    /// The Strategy interface for "should this rule's style apply to this cell?" --
    /// mirrors IFunction's role in the function library. ComparisonCondition covers
    /// the common case (value &gt; 100, etc); FormulaCondition covers the general
    /// case of an arbitrary user-written boolean formula, reusing the same
    /// IExpressionNode/IEvaluationContext machinery as everything else in this module.
    /// </summary>
    public interface IConditionalFormatCondition
    {
        bool IsSatisfiedBy(CellValue value, IEvaluationContext context);
    }
}
