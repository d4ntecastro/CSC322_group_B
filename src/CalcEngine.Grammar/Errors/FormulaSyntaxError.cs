namespace CalcEngine.Grammar.Errors;

/// <summary>
/// Describes exactly one problem found while parsing a formula: where it is
/// and what's wrong. The spec asks for error messages that "tell the user
/// what is wrong and where it is wrong" — Line/Column/OffendingText are what
/// make that possible instead of a bare "syntax error".
/// </summary>
public sealed class FormulaSyntaxError
{
    /// <summary>1-based line number where the error occurred (formulas are
    /// almost always one line, but this stays correct if that ever changes).</summary>
    public int Line { get; }

    /// <summary>0-based column offset within the line.</summary>
    public int Column { get; }

    /// <summary>Human-readable description, e.g. "Expected ')' but found end of formula".</summary>
    public string Message { get; }

    /// <summary>The exact token text that triggered the error, if available (may be empty).</summary>
    public string OffendingText { get; }

    public FormulaSyntaxError(int line, int column, string message, string offendingText)
    {
        Line = line;
        Column = column;
        Message = message;
        OffendingText = offendingText ?? string.Empty;
    }

    public override string ToString() =>
        string.IsNullOrEmpty(OffendingText)
            ? $"Line {Line}, Column {Column}: {Message}"
            : $"Line {Line}, Column {Column}: {Message} (near '{OffendingText}')";
}
