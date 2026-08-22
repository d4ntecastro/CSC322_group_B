using System.Collections.Generic;
using System.Linq;
using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Functions
{
    /// <summary>MIN(value_or_range, ...) - smallest numeric value across all arguments. An all-empty/all-text selection has no minimum, so it returns 0, matching Excel's MIN.</summary>
    public sealed class MinFunction : IFunction
    {
        public string Name => "MIN";

        public CellValue Invoke(IReadOnlyList<IExpressionNode> arguments, IEvaluationContext context)
        {
            if (arguments.Count == 0) return CellValue.Error(CellErrorType.InvalidValue);

            var values = ArgumentHelper.FlattenValues(arguments, context).ToList();
            if (ArgumentHelper.TryFindFirstError(values, out var error)) return error;

            var numbers = values.Where(v => v.Kind == CellValueKind.Number).Select(v => v.NumberValue).ToList();
            return CellValue.Number(numbers.Count == 0 ? 0 : numbers.Min());
        }
    }
}
