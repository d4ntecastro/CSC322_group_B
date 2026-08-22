using System.Collections.Generic;
using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Functions
{
    /// <summary>
    /// The Strategy interface for a built-in spreadsheet function. Each function
    /// receives the raw, un-evaluated argument nodes (not values) plus the context,
    /// and decides for itself how and whether to evaluate each one -- this is what
    /// lets IF evaluate only the branch it takes, and lets SUM/AVERAGE/etc. flatten
    /// range arguments instead of trying to evaluate a whole range as one value.
    /// </summary>
    public interface IFunction
    {
        /// <summary>The name a formula uses to call this function, e.g. "SUM". Matched case-insensitively by FunctionRegistry.</summary>
        string Name { get; }

        CellValue Invoke(IReadOnlyList<IExpressionNode> arguments, IEvaluationContext context);
    }
}
