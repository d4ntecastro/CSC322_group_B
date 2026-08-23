using System.Collections.Generic;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Ast
{
    /// <summary>
    /// A node representing a rectangular range, e.g. the "A1:A5" in =SUM(A1:A5).
    /// A range has no single value of its own -- Evaluate() returns #VALUE! if a range
    /// is used somewhere a single value was expected (e.g. =A1:A5+1). Functions that
    /// accept ranges (SUM, AVERAGE, MIN, MAX, COUNT, LOOKUP) special-case
    /// RangeReferenceNode and call GetValues() instead of Evaluate() -- see
    /// Functions/ArgumentHelper.cs.
    /// </summary>
    public sealed class RangeReferenceNode : IExpressionNode
    {
        public CellAddress Start { get; }
        public CellAddress End { get; }

        public RangeReferenceNode(CellAddress start, CellAddress end)
        {
            Start = start;
            End = end;
        }

        public CellValue Evaluate(IEvaluationContext context) => CellValue.Error(CellErrorType.InvalidValue);

        /// <summary>Every cell value in the range, row-major, delegated to the context so any grid backing works.</summary>
        public IEnumerable<CellValue> GetValues(IEvaluationContext context) => context.GetRangeValues(Start, End);
    }
}
