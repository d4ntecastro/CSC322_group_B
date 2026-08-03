using System.Collections.Generic;
using CalcEngine.Grammar.Values;

namespace CalcEngine.Grammar.Tree;

/// <summary>
/// The Component in the Composite pattern, and simultaneously the
/// AbstractExpression in the Interpreter pattern: every node in a parsed
/// formula — literal, cell reference, operator, or function call — is one
/// of these, and every node knows how to evaluate itself.
///
/// ABSTRACT DATA TYPE
/// -------------------
/// Abstraction function:
///     AF(node) = the value the sub-formula rooted at this node produces,
///                as a function of whatever cell values it depends on.
///                A leaf node (number/text/cell ref) abstracts a single
///                value or lookup; a composite node (binary op, function
///                call) abstracts "combine my children's values this way".
///
/// Representation invariant (applies to every implementing class):
///     - A node's children (if any) are themselves valid IExpressionNode
///       instances — never null.
///     - Evaluate() never throws for a well-formed tree. Domain errors
///       (division by zero, wrong argument type, missing cell) are
///       reported as CellValue.Error(...), never as a thrown exception —
///       this is what lets a single bad formula fail gracefully instead
///       of crashing the whole recalculation pass.
///
/// Composite pattern: composite nodes (BinaryOperationNode,
/// FunctionCallNode) hold child IExpressionNode references and delegate to
/// them; leaf nodes (NumberLiteralNode, CellReferenceNode) hold no children.
/// Both are used identically by anything holding an IExpressionNode
/// reference — the caller never needs to know which kind it has.
///
/// Interpreter pattern: Evaluate(context) is literally "interpret this node
/// in the given context", where context supplies the only external
/// information a node needs (other cells' values, function implementations).
/// </summary>
public interface IExpressionNode
{
    /// <summary>
    /// Computes this node's value. For a leaf, this is direct (a literal
    /// value, or a lookup via context). For a composite node, this means
    /// evaluating each child first, then combining their results.
    /// </summary>
    CellValue Evaluate(IEvaluationContext context);

    /// <summary>
    /// Returns every cell this node (including all its children) reads from,
    /// e.g. a BinaryOperationNode for "B2 + C3" returns { "B2", "C3" }, and a
    /// RangeNode for "B2:B45" returns every cell in that range.
    ///
    /// This is the hook the dependency graph module (Thomson's Graph
    /// project) uses to build its edges: after parsing a formula, the graph
    /// calls GetCellReferences() on the resulting tree to know which cells
    /// this formula depends on, without needing to know anything about
    /// expression trees itself.
    /// </summary>
    IEnumerable<string> GetCellReferences();
}
