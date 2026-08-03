using System.Collections.Generic;
using CalcEngine.Grammar.Tree;

namespace CalcEngine.Grammar.Errors;

/// <summary>
/// The result of attempting to parse a formula string.
///
/// ABSTRACT DATA TYPE
/// -------------------
/// Abstraction function:
///     AF(rep) = either a successfully-built expression tree (Success == true,
///               Tree != null, Errors empty), or a non-empty list of syntax
///               errors describing everything wrong with the input
///               (Success == false, Tree == null).
///
/// Representation invariant:
///     Success == true  ⟺  Tree != null AND Errors.Count == 0
///     Success == false ⟺  Tree == null AND Errors.Count > 0
///
/// This exists so that "the formula the user typed is broken" is an ordinary
/// return value that the evaluator/GUI can branch on, never an exception
/// that has to be caught — matching the spec's requirement that malformed
/// input is normal input, not a crash.
/// </summary>
public sealed class ParseResult
{
    public bool Success { get; }
    public IExpressionNode? Tree { get; }
    public IReadOnlyList<FormulaSyntaxError> Errors { get; }

    private ParseResult(bool success, IExpressionNode tree, IReadOnlyList<FormulaSyntaxError> errors)
    {
        Success = success;
        Tree = tree;
        Errors = errors;
    }

    public static ParseResult Ok(IExpressionNode tree) =>
        new(true, tree, System.Array.Empty<FormulaSyntaxError>());

    public static ParseResult Fail(IReadOnlyList<FormulaSyntaxError> errors) =>
        new(false, null, errors);
}
