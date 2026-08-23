namespace CalcEngine.Evaluator.Values
{
    /// <summary>
    /// The specific kind of evaluation failure. Modeled on classic spreadsheet error
    /// codes (Excel/Google Sheets) so the GUI can show something a user recognizes.
    /// This is the backbone of "errors are values, not exceptions" for the whole module:
    /// nothing in CalcEngine.Evaluator throws for a bad formula -- it returns
    /// CellValue.Error(someCellErrorType) instead, and that value flows through the
    /// rest of the computation exactly like a number would.
    /// </summary>
    public enum CellErrorType
    {
        /// <summary>Division by zero, e.g. =1/0.</summary>
        DivideByZero,

        /// <summary>An operand was the wrong type and could not be coerced, e.g. =1+"abc".</summary>
        InvalidValue,

        /// <summary>A cell or range reference doesn't make sense, e.g. mismatched LOOKUP range sizes.</summary>
        InvalidReference,

        /// <summary>A formula called a function name the FunctionRegistry doesn't know, e.g. =NOTAREALFN(1).</summary>
        NameNotFound,

        /// <summary>
        /// The dependency graph (Thomson's module) detected this cell is part of a cycle.
        /// The Evaluator doesn't detect cycles itself -- it just knows how to represent
        /// one as a value once the Graph module reports it.
        /// </summary>
        CircularReference,

        /// <summary>A lookup found no matching row, e.g. LOOKUP() with no match.</summary>
        NotAvailable
    }

    /// <summary>
    /// Maps each CellErrorType to the short string a user would recognize in a cell,
    /// e.g. "#DIV/0!". Kept separate from the enum itself so the enum stays a plain,
    /// serialization-friendly value type.
    /// </summary>
    public static class CellErrorTypeExtensions
    {
        public static string ToDisplayString(this CellErrorType errorType) => errorType switch
        {
            CellErrorType.DivideByZero => "#DIV/0!",
            CellErrorType.InvalidValue => "#VALUE!",
            CellErrorType.InvalidReference => "#REF!",
            CellErrorType.NameNotFound => "#NAME?",
            CellErrorType.CircularReference => "#CIRCULAR!",
            CellErrorType.NotAvailable => "#N/A",
            _ => "#ERROR!"
        };
    }
}
