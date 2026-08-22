using System;
using System.Collections.Generic;
using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Functions
{
    /// <summary>
    /// ROUND(number, digits) - rounds to the given number of decimal places, away
    /// from zero on a tie (so 2.5 -> 3, not 2, matching how spreadsheets round rather
    /// than .NET's default banker's rounding). digits may be negative to round to
    /// tens/hundreds/etc., e.g. ROUND(1250, -2) -> 1300.
    /// </summary>
    public sealed class RoundFunction : IFunction
    {
        public string Name => "ROUND";

        public CellValue Invoke(IReadOnlyList<IExpressionNode> arguments, IEvaluationContext context)
        {
            if (arguments.Count != 2) return CellValue.Error(CellErrorType.InvalidValue);

            var numberValue = context.Evaluate(arguments[0]);
            if (numberValue.IsError) return numberValue;

            var digitsValue = context.Evaluate(arguments[1]);
            if (digitsValue.IsError) return digitsValue;

            if (!numberValue.TryCoerceToNumber(out var number) || !digitsValue.TryCoerceToNumber(out var digitsRaw))
                return CellValue.Error(CellErrorType.InvalidValue);

            var digits = (int)Math.Round(digitsRaw, MidpointRounding.AwayFromZero);
            if (digits < -15 || digits > 15) return CellValue.Error(CellErrorType.InvalidValue);

            var scale = Math.Pow(10, digits);
            var rounded = Math.Round(number * scale, MidpointRounding.AwayFromZero) / scale;
            return CellValue.Number(rounded);
        }
    }
}
