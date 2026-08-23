using System.Collections.Generic;
using System.Linq;
using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Functions
{
    /// <summary>AVERAGE(value_or_range, ...) - mean of the numeric values across all arguments. Averaging zero numeric values is undefined, so it reports #DIV/0!, matching real spreadsheets.</summary>
    public sealed class AverageFunction : IFunction
    {
        public string Name => "AVERAGE";

        public CellValue Invoke(IReadOnlyList<IExpressionNode> arguments, IEvaluationContext context)
        {
            if (arguments.Count == 0) return CellValue.Error(CellErrorType.InvalidValue);

            var values = ArgumentHelper.FlattenValues(arguments, context).ToList();
            if (ArgumentHelper.TryFindFirstError(values, out var error)) return error;

            var numbers = values.Where(v => v.Kind == CellValueKind.Number).Select(v => v.NumberValue).ToList();
            if (numbers.Count == 0) return CellValue.Error(CellErrorType.DivideByZero);

            return CellValue.Number(numbers.Average());
        }
    }
}
