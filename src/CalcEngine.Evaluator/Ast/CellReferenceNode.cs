using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Ast
{
    /// <summary>
    /// A leaf node referencing a single other cell, e.g. the "A1" in =A1+1.
    /// Evaluating it just asks the context (backed by the Graph/GUI module's grid) for
    /// that cell's current value -- this node never talks to the grid directly, which
    /// keeps the Evaluator module decoupled from however the grid is actually stored.
    /// </summary>
    public sealed class CellReferenceNode : IExpressionNode
    {
        public CellAddress Address { get; }

        public CellReferenceNode(CellAddress address) => Address = address;

        public CellValue Evaluate(IEvaluationContext context) => context.GetCellValue(Address);
    }
}
