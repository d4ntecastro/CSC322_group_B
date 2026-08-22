using System.Collections.Generic;
using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Functions
{
    /// <summary>
    /// IF(condition, valueIfTrue, [valueIfFalse]) - evaluates the condition, then
    /// evaluates and returns ONLY the branch actually taken. This laziness matters:
    /// =IF(A1&lt;&gt;0, 10/A1, 0) must not blow up with #DIV/0! just because the false
    /// branch happens to divide by A1 too -- it should never even be evaluated when
    /// A1 = 0. See FunctionsTests/IfFunctionTests.cs for a test that proves this.
    /// The false branch is optional, matching Excel; when omitted and the condition
    /// is false, IF returns FALSE.
    /// </summary>
    public sealed class IfFunction : IFunction
    {
        public string Name => "IF";

        public CellValue Invoke(IReadOnlyList<IExpressionNode> arguments, IEvaluationContext context)
        {
            if (arguments.Count < 2 || arguments.Count > 3) return CellValue.Error(CellErrorType.InvalidValue);

            var condition = context.Evaluate(arguments[0]);
            if (condition.IsError) return condition;

            if (!condition.TryCoerceToBoolean(out var flag)) return CellValue.Error(CellErrorType.InvalidValue);

            if (flag) return context.Evaluate(arguments[1]);

            return arguments.Count == 3 ? context.Evaluate(arguments[2]) : CellValue.Boolean(false);
        }
    }
}
