using System;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.ConditionalFormatting
{
    /// <summary>The common case: "highlight this cell if its value is &gt; 100" and similar. Non-numeric cell values never satisfy a comparison condition.</summary>
    public sealed class ComparisonCondition : IConditionalFormatCondition
    {
        private readonly ComparisonOperator _operator;
        private readonly double _threshold;
        private readonly double _thresholdUpper;

        public ComparisonCondition(ComparisonOperator @operator, double threshold, double thresholdUpper = 0)
        {
            _operator = @operator;
            _threshold = threshold;
            _thresholdUpper = thresholdUpper;
        }

        public bool IsSatisfiedBy(CellValue value, IEvaluationContext context)
        {
            if (!value.TryCoerceToNumber(out var number)) return false;

            return _operator switch
            {
                ComparisonOperator.GreaterThan => number > _threshold,
                ComparisonOperator.LessThan => number < _threshold,
                ComparisonOperator.GreaterOrEqual => number >= _threshold,
                ComparisonOperator.LessOrEqual => number <= _threshold,
                ComparisonOperator.Equal => Math.Abs(number - _threshold) < 1e-9,
                ComparisonOperator.NotEqual => Math.Abs(number - _threshold) >= 1e-9,
                ComparisonOperator.Between => number >= _threshold && number <= _thresholdUpper,
                _ => false
            };
        }
    }
}
