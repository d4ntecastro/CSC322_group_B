using Antlr4.Runtime;
using CalcEngine.Grammar.Errors;
using CalcEngine.Grammar.Generated;

namespace CalcEngine.Grammar;

/// <summary>
/// The public entry point for this entire module. Everything else in this
/// namespace (the grammar, the ANTLR-generated lexer/parser, the visitor,
/// the tree node classes) is effectively private machinery — anyone
/// integrating with the Grammar module (Thomson's Graph module, or the
/// GUI) should only ever need to call FormulaParserService.Parse(...).
///
/// Usage:
///     var result = new FormulaParserService().Parse("=SUM(B2:B45)*0.3");
///     if (result.Success)
///     {
///         var dependsOn = result.Tree.GetCellReferences();
///         var value = result.Tree.Evaluate(someContext);
///     }
///     else
///     {
///         foreach (var error in result.Errors)
///             Console.WriteLine(error);   // "Line 1, Column 12: ..."
///     }
/// </summary>
public sealed class FormulaParserService
{
    /// <summary>
    /// Parses a single formula string (expected to start with '=', matching
    /// spreadsheet convention) into an expression tree.
    ///
    /// Never throws for malformed input — "malformed input is normal input"
    /// per the spec, so every possible parse failure is represented as
    /// ParseResult.Fail(errors) rather than a thrown exception escaping to
    /// the caller.
    /// </summary>
    public ParseResult Parse(string formulaText)
    {
        var inputStream = new AntlrInputStream(formulaText ?? string.Empty);
        var lexer = new FormulaLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new FormulaParser(tokenStream);

        // Replace ANTLR's default console-printing error handling with our
        // own listener that just collects errors as data (see
        // FormulaSyntaxErrorListener for why this matters).
        var errorListener = new FormulaSyntaxErrorListener();
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(errorListener);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(errorListener);

        var parseTree = parser.formula();

        if (errorListener.Errors.Count > 0)
            return ParseResult.Fail(errorListener.Errors);

        var builder = new ExpressionTreeBuilder();
        var expressionTree = builder.Visit(parseTree);

        return ParseResult.Ok(expressionTree);
    }
}
