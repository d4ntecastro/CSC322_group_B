namespace CalcEngine.Evaluator.Ast
{
    /// <summary>The operators BinaryOperationNode can apply between two evaluated operands.</summary>
    public enum BinaryOperator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Power,
        Equal,
        NotEqual,
        LessThan,
        LessOrEqual,
        GreaterThan,
        GreaterOrEqual,
        /// <summary>Text concatenation, e.g. ="Row "&1 -> "Row 1".</summary>
        Concatenate
    }
}
