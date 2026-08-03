using System.Collections.Generic;
using CalcEngine.Grammar.Values;

namespace CalcEngine.Grammar.Tree;

/// <summary>
/// The one seam between the Grammar/Tree module and the rest of the engine.
/// Expression nodes never talk to the dependency graph or the function
/// library directly — they only ever call through this interface. That
/// keeps this module (Daniel's) fully independent of how Thomson's graph
/// or Olarewaju's evaluator/function-library are implemented: any class
/// that implements IEvaluationContext can drive this expression tree,
/// including a lightweight fake used purely for unit testing the tree in
/// isolation, with no dependency graph or real functions involved at all.
/// </summary>
public interface IEvaluationContext
{
    /// <summary>
    /// Returns the current value of a single cell, e.g. "B2". Implementations
    /// (the real evaluator) return CellValue.Empty for a blank cell, never
    /// throw for an out-of-range/unknown reference — an unknown cell is a
    /// CellValue.Error("#REF!") result instead.
    /// </summary>
    CellValue GetCellValue(string cellReference);

    /// <summary>
    /// Returns every cell in a range, e.g. "B2:B45", in reading order.
    /// Used by aggregate functions (SUM, AVERAGE, COUNT, ...) via
    /// CallFunction below — a RangeNode itself does not attempt to reduce
    /// these to a single value, since only the function knows how.
    /// </summary>
    IEnumerable<CellValue> GetRangeValues(string rangeReference);

    /// <summary>
    /// Invokes a named function (SUM, AVERAGE, IF, ROUND, LOOKUP, ...) with
    /// its already-parsed argument nodes. The function library itself
    /// (Strategy/Factory pattern) lives in Olarewaju's Evaluator module —
    /// this expression tree only needs to know that "calling a function by
    /// name" is possible, not how any individual function is computed.
    /// Returns CellValue.Error("#NAME?") for an unrecognised function name.
    /// </summary>
    CellValue CallFunction(string functionName, IReadOnlyList<IExpressionNode> arguments);
}
