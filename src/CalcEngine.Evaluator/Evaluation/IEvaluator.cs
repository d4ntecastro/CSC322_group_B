using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Evaluation
{
    /// <summary>The Evaluator module's single public entry point: "given a formula's expression tree, produce its value."</summary>
    public interface IEvaluator
    {
        CellValue Evaluate(IExpressionNode root, IEvaluationContext context);
    }
}
