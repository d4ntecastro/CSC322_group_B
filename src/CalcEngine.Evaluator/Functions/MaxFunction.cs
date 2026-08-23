using System.Collections.Generic;
using System.Linq;
using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Functions
{
    /// <summary>MAX(value_or_range, ...) - largest numeric value across all arguments. Mirrors MinFunction; see its comments for the empty-selection behaviour.</summary>
    public sealed class MaxFunction : IFunction
    {
        public string Name => "MAX";

        public CellValue Invoke(IReadOnlyList<IExpressionNode> arguments, IEvaluationContext context)
        {
            if (arguments.Count == 0) return CellValue.Error(CellErrorType.InvalidValue);

            var values = ArgumentHelper.FlattenValues(arguments, context).ToList();
            if (ArgumentHelper.TryFindFirstError(values, out var error)) return error;

            var numbers = values.Where(v => v.Kind == CellValueKind.Number).Select(v => v.NumberValue).ToList();
            return CellValue.Number(numbers.Count == 0 ? 0 : numbers.Max());
        }
    }
}
