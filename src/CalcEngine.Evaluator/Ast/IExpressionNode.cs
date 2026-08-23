using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Ast
{
    /// <summary>
    /// The contract every expression-tree node implements -- this is the seam between
    /// Daniel's Grammar module (which builds the tree: Composite pattern) and
    /// Olarewaju's Evaluator module (which walks it: Interpreter pattern).
    ///
    /// Daniel's parser produces a tree of these nodes; the concrete node classes in this
    /// file are working reference implementations (and stand-ins for tests) that satisfy
    /// the same contract the real parser output must satisfy. As long as the parser
    /// produces IExpressionNode instances that implement Evaluate() correctly, the rest
    /// of this module (functions, undo/redo, conditional formatting) works unmodified
    /// against them -- that's the point of coding to the interface.
    ///
    /// Interpreter pattern: each node knows how to evaluate itself given a context.
    /// Composite pattern: a node's Evaluate() typically calls Evaluate() on its
    /// children (see BinaryOperationNode, FunctionCallNode), so "evaluate the whole
    /// tree" is just "evaluate the root".
    /// </summary>
    public interface IExpressionNode
    {
        /// <summary>
        /// Evaluates this node and everything beneath it. Must never throw for a
        /// malformed *formula* (wrong types, missing cells, etc.) -- those become
        /// CellValue.Error(...) results instead. A genuine bug throwing here is still
        /// caught by Evaluator.Evaluate as a last resort, but individual nodes should
        /// not rely on that.
        /// </summary>
        CellValue Evaluate(IEvaluationContext context);
    }
}
