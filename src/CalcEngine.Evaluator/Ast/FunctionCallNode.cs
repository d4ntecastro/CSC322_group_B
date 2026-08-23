using System.Collections.Generic;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Ast
{
    /// <summary>
    /// An internal node representing a function call, e.g. =SUM(A1:A3, 10).
    /// Notice its children (Arguments) are NOT evaluated here -- that decision is
    /// delegated entirely to the matched IFunction, because some functions need to
    /// choose which arguments to evaluate at all (IF only evaluates the branch it
    /// takes) or need raw range nodes instead of a single value (SUM needs to flatten
    /// a range, not evaluate it as one value). See Functions/ArgumentHelper.cs.
    /// </summary>
    public sealed class FunctionCallNode : IExpressionNode
    {
        public string Name { get; }
        public IReadOnlyList<IExpressionNode> Arguments { get; }

        public FunctionCallNode(string name, IReadOnlyList<IExpressionNode> arguments)
        {
            Name = name;
            Arguments = arguments;
        }

        public CellValue Evaluate(IEvaluationContext context)
        {
            var function = context.Functions.Resolve(Name);
            if (function is null) return CellValue.Error(CellErrorType.NameNotFound);
            return function.Invoke(Arguments, context);
        }
    }
}
