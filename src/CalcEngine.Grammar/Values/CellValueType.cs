namespace CalcEngine.Grammar.Values;

/// <summary>
/// The kind of value a <see cref="CellValue"/> represents.
///
/// The spec requires the evaluator to represent type errors, missing
/// references, and division-by-zero as VALUES (an Error result) rather
/// than as exceptions that escape to the client. This enum is what lets
/// every layer of the pipeline — expression tree, evaluator, GUI — treat
/// "this cell is broken" as ordinary data instead of a control-flow event.
/// </summary>
public enum CellValueType
{
    /// <summary>A numeric result, e.g. from 2 + 3 or SUM(B2:B10).</summary>
    Number,

    /// <summary>A text result, e.g. from a string literal or LOOKUP.</summary>
    Text,

    /// <summary>A boolean result, e.g. from a comparison like B2 &gt; 10.</summary>
    Boolean,

    /// <summary>
    /// An empty cell. Distinct from Number(0) or Text(""), since spreadsheet
    /// semantics treat a truly empty cell differently in some functions
    /// (e.g. AVERAGE skips empty cells but includes explicit zeros).
    /// </summary>
    Empty,

    /// <summary>
    /// An error result (e.g. #DIV/0!, #REF!, #VALUE!, #CYCLE!). Carries no
    /// further payload beyond the error code/message — see <see cref="CellValue.ErrorMessage"/>.
    /// </summary>
    Error
}
