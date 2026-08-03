using System.Collections.Generic;
using Antlr4.Runtime;

namespace CalcEngine.Grammar.Errors;

/// <summary>
/// By default, ANTLR prints syntax errors straight to the console and keeps
/// going — not useful for an API that other code will call programmatically.
/// This listener replaces that default behaviour: it collects every error
/// into a list we can inspect afterwards, so a malformed formula becomes a
/// ParseResult.Fail(...) instead of console noise or a thrown exception.
///
/// One instance of this listener is created per Parse() call (see
/// FormulaParserService), so Errors never leaks state between formulas.
///
/// This implements two listener interfaces because ANTLR reports errors
/// differently at two different stages: BaseErrorListener handles parser
/// (token-level) errors, while IAntlrErrorListener&lt;int&gt; handles lexer
/// (raw character-level) errors that happen before tokens even exist.
/// </summary>
public sealed class FormulaSyntaxErrorListener : BaseErrorListener, IAntlrErrorListener<int>
{
    private readonly List<FormulaSyntaxError> _errors = new();

    public IReadOnlyList<FormulaSyntaxError> Errors => _errors;

    public override void SyntaxError(
        System.IO.TextWriter output,
        IRecognizer recognizer,
        IToken offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        _errors.Add(new FormulaSyntaxError(
            line,
            charPositionInLine,
            HumanizeMessage(msg),
            offendingSymbol?.Text ?? string.Empty));
    }

    void IAntlrErrorListener<int>.SyntaxError(
        System.IO.TextWriter output,
        IRecognizer recognizer,
        int offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        _errors.Add(new FormulaSyntaxError(line, charPositionInLine, HumanizeMessage(msg), string.Empty));
    }

    /// <summary>
    /// ANTLR's raw messages (e.g. "mismatched input '&lt;EOF&gt;' expecting ')'")
    /// are accurate but not friendly. This rewrites the common cases into
    /// plain language, per the spec's requirement for helpful error messages.
    /// Anything not recognised here is passed through unchanged rather than
    /// hidden, so no error is ever silently swallowed.
    /// </summary>
    private static string HumanizeMessage(string antlrMessage)
    {
        if (antlrMessage.Contains("mismatched input") && antlrMessage.Contains("<EOF>"))
            return "The formula ends unexpectedly — you may be missing a closing ')' or an operand.";

        if (antlrMessage.Contains("extraneous input"))
            return "There's an unexpected character or token here.";

        if (antlrMessage.Contains("no viable alternative"))
            return "This isn't a valid formula expression at this position.";

        return antlrMessage;
    }
}