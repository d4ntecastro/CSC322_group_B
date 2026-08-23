namespace CalcEngine.Evaluator.Values
{
    /// <summary>
    /// The discriminant for CellValue -- tells you which field of CellValue is meaningful.
    /// This is what makes CellValue behave like a tagged union / discriminated union,
    /// even though C# doesn't have one built in.
    /// </summary>
    public enum CellValueKind
    {
        /// <summary>The cell has never been given a value or formula.</summary>
        Empty,
        Number,
        Text,
        Boolean,
        /// <summary>Evaluation failed in a well-defined, spreadsheet-style way (see CellErrorType).</summary>
        Error
    }
}
