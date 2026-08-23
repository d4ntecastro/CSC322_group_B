using System.Collections.Generic;
using System.Linq;
using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Functions
{
    /// <summary>
    /// SUM(value_or_range, ...) - adds every numeric value across all arguments.
    /// Simplification vs. real Excel: only cells whose *kind* is already Number are
    /// counted -- text cells (even numeric-looking ones like "5") are skipped rather
    /// than coerced, matching how SUM treats a range in practice and keeping this
    /// function's behaviour predictable and easy to test.
    /// </summary>
    public sealed class SumFunction : IFunction
    {
        public string Name => "SUM";

        public CellValue Invoke(IReadOnlyList<IExpressionNode> arguments, IEvaluationContext context)
        {
            if (arguments.Count == 0) return CellValue.Error(CellErrorType.InvalidValue);

            var values = ArgumentHelper.FlattenValues(arguments, context).ToList();
            if (ArgumentHelper.TryFindFirstError(values, out var error)) return error;

            var total = values.Where(v => v.Kind == CellValueKind.Number).Sum(v => v.NumberValue);
            return CellValue.Number(total);
        }
    }
}
