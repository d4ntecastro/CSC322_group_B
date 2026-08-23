namespace CalcEngine.Evaluator.ConditionalFormatting
{
    /// <summary>The comparisons a ComparisonCondition can test a cell's numeric value against.</summary>
    public enum ComparisonOperator
    {
        GreaterThan,
        LessThan,
        GreaterOrEqual,
        LessOrEqual,
        Equal,
        NotEqual,
        /// <summary>Inclusive range check using both Threshold and ThresholdUpper.</summary>
        Between
    }
}
