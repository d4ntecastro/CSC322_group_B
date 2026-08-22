using System;
using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Evaluation
{
    /// <summary>
    /// The tree-walking evaluator. In practice, most of the "walking" happens inside
    /// each node's own Evaluate() method (that's the Interpreter pattern: nodes walk
    /// their own children). This class is the top-level entry point plus a safety net.
    ///
    /// The safety net matters: the whole module promises "malformed formulas produce
    /// an error CellValue, not an exception." Individual nodes and functions are
    /// written to uphold that promise directly (see BinaryOperationNode, IfFunction,
    /// etc.), but if an unexpected .NET exception ever does escape from somewhere --
    /// a bug, an edge case nobody thought of, a stack overflow from a pathological
    /// tree -- this catch block converts it into CellValue.Error(InvalidValue) instead
    /// of crashing the whole application. It should be viewed as a last resort, not a
    /// substitute for individual nodes handling their own error cases properly.
    /// </summary>
    public sealed class TreeWalkingEvaluator : IEvaluator
    {
        public CellValue Evaluate(IExpressionNode root, IEvaluationContext context)
        {
            if (root is null) return CellValue.Error(CellErrorType.InvalidValue);

            try
            {
                return root.Evaluate(context);
            }
            catch (StackOverflowException)
            {
                // Cannot actually be caught by the CLR, but documents the intent:
                // a pathologically deep/self-referential tree should ideally be
                // caught by the Graph module's cycle detection before it ever
                // reaches here.
                throw;
            }
            catch (Exception)
            {
                return CellValue.Error(CellErrorType.InvalidValue);
            }
        }
    }
}
