namespace CalcEngine.Evaluator.Ast
{
    /// <summary>The operators UnaryOperationNode can apply to a single evaluated operand.</summary>
    public enum UnaryOperator
    {
        /// <summary>Arithmetic negation, e.g. =-A1.</summary>
        Negate,
        /// <summary>Percent conversion, e.g. =50% -> 0.5.</summary>
        Percent
    }
}
