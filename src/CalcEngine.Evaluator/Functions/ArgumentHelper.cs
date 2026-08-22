using System.Collections.Generic;
using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Functions
{
    /// <summary>
    /// Shared argument-handling logic used by the range/aggregate functions
    /// (SUM, AVERAGE, MIN, MAX, COUNT). Kept here instead of duplicated in every
    /// function class.
    /// </summary>
    public static class ArgumentHelper
    {
        /// <summary>
        /// Evaluates each argument node, expanding any RangeReferenceNode into all of
        /// the cell values it covers. A single-cell or literal argument contributes
        /// exactly one value; a range argument contributes one value per cell in it.
        /// This is why SUM(A1:A3, 10) sees four values, not two.
        /// </summary>
        public static IEnumerable<CellValue> FlattenValues(IReadOnlyList<IExpressionNode> arguments, IEvaluationContext context)
        {
            foreach (var argument in arguments)
            {
                if (argument is RangeReferenceNode range)
                {
                    foreach (var value in range.GetValues(context))
                        yield return value;
                }
                else
                {
                    yield return context.Evaluate(argument);
                }
            }
        }

        /// <summary>Returns the first error found among the given values, if any -- used so an aggregate function propagates the first error it hits rather than silently skipping it.</summary>
        public static bool TryFindFirstError(IEnumerable<CellValue> values, out CellValue error)
        {
            foreach (var value in values)
            {
                if (value.IsError)
                {
                    error = value;
                    return true;
                }
            }

            error = default;
            return false;
        }
    }
}
