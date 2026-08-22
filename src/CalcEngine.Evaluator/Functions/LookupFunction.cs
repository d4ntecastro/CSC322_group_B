using System;
using System.Collections.Generic;
using System.Linq;
using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Functions
{
    /// <summary>
    /// LOOKUP(lookupValue, lookupRange, resultRange) - finds the first cell in
    /// lookupRange equal to lookupValue and returns the value at the same position
    /// in resultRange. Simplified from Excel's real LOOKUP (which assumes a sorted
    /// range and does a binary-ish scan) to a plain first-match linear scan, which is
    /// easier to reason about and test correctly for a class project, and is exactly
    /// what most course specs mean by "LOOKUP".
    /// Both range arguments must be literal ranges of the same size, or this returns
    /// #REF!. A lookup with no match returns #N/A, matching spreadsheet convention.
    /// </summary>
    public sealed class LookupFunction : IFunction
    {
        public string Name => "LOOKUP";

        public CellValue Invoke(IReadOnlyList<IExpressionNode> arguments, IEvaluationContext context)
        {
            if (arguments.Count != 3) return CellValue.Error(CellErrorType.InvalidValue);

            var lookupValue = context.Evaluate(arguments[0]);
            if (lookupValue.IsError) return lookupValue;

            if (arguments[1] is not RangeReferenceNode lookupRange || arguments[2] is not RangeReferenceNode resultRange)
                return CellValue.Error(CellErrorType.InvalidReference);

            var lookupValues = lookupRange.GetValues(context).ToList();
            var resultValues = resultRange.GetValues(context).ToList();

            if (lookupValues.Count != resultValues.Count) return CellValue.Error(CellErrorType.InvalidReference);

            for (var i = 0; i < lookupValues.Count; i++)
            {
                if (lookupValues[i].IsError) return lookupValues[i];
                if (ValuesMatch(lookupValues[i], lookupValue)) return resultValues[i];
            }

            return CellValue.Error(CellErrorType.NotAvailable);
        }

        private static bool ValuesMatch(CellValue a, CellValue b)
        {
            if (a.Kind == CellValueKind.Number && b.Kind == CellValueKind.Number)
                return Math.Abs(a.NumberValue - b.NumberValue) < 1e-9;

            if (a.Kind == CellValueKind.Text && b.Kind == CellValueKind.Text)
                return string.Equals(a.TextValue, b.TextValue, StringComparison.OrdinalIgnoreCase);

            return string.Equals(a.ToString(), b.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
