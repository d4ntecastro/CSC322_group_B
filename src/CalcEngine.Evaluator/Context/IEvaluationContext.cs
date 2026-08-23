using System.Collections.Generic;
using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.Functions;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Context
{
    /// <summary>
    /// The "environment" object threaded through every node's Evaluate() call --
    /// this is the classic Interpreter-pattern Context. It bundles everything a node
    /// or function might need: reading other cells, reading a whole range, looking up
    /// a function by name, and recursively evaluating a child node.
    /// </summary>
    public interface IEvaluationContext
    {
        CellValue GetCellValue(CellAddress address);

        IEnumerable<CellValue> GetRangeValues(CellAddress start, CellAddress end);

        /// <summary>The Strategy/Factory registry of built-in functions (SUM, IF, ROUND, ...).</summary>
        FunctionRegistry Functions { get; }

        /// <summary>
        /// Evaluates any child node. Nodes and functions call this instead of
        /// node.Evaluate(context) directly so that all evaluation flows through one
        /// place -- useful if the Evaluator ever needs to add cross-cutting behaviour
        /// (evaluation-step counting, timeouts, tracing) without touching every node.
        /// </summary>
        CellValue Evaluate(IExpressionNode node);
    }
}
