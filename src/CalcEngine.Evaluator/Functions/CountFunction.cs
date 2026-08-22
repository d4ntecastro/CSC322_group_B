using System.Collections.Generic;
using System.Linq;
using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Functions
{
    /// <summary>COUNT(value_or_range, ...) - counts how many of the given values are numbers. Text, booleans, and empty cells are not counted, matching Excel's COUNT (as opposed to COUNTA).</summary>
    public sealed class CountFunction : IFunction
    {
        public string Name => "COUNT";

        public CellValue Invoke(IReadOnlyList<IExpressionNode> arguments, IEvaluationContext context)
        {
            if (arguments.Count == 0) return CellValue.Error(CellErrorType.InvalidValue);

            var values = ArgumentHelper.FlattenValues(arguments, context).ToList();
            if (ArgumentHelper.TryFindFirstError(values, out var error)) return error;

            return CellValue.Number(values.Count(v => v.Kind == CellValueKind.Number));
        }
    }
}
