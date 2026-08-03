using System.Globalization;
using System.Linq;
using CalcEngine.Grammar.Generated;
using CalcEngine.Grammar.Tree;

namespace CalcEngine.Grammar;

/// <summary>
/// Converts an ANTLR parse tree (built directly from Formula.g4) into our
/// own IExpressionNode tree.
///
/// Why a separate tree at all, instead of just using ANTLR's parse tree
/// everywhere? Two reasons:
///   1. ANTLR's generated context classes are shaped by grammar rules, not
///      by evaluation semantics — walking them directly to "evaluate a
///      formula" would tangle grammar structure with business logic.
///   2. Our own IExpressionNode tree is what Thomson's graph and
///      Olarewaju's evaluator depend on — they should never need to
///      reference ANTLR's generated types at all. This class is exactly
///      the seam that keeps ANTLR itself as an implementation detail of
///      the Grammar module alone.
///
/// This class implements the Visitor half of translating parse tree to
/// expression tree (using ANTLR's generated FormulaBaseVisitor&lt;T&gt;),
/// while the resulting IExpressionNode tree itself is what implements the
/// Composite + Interpreter patterns the project brief asks for.
/// </summary>
public sealed class ExpressionTreeBuilder : FormulaBaseVisitor<IExpressionNode>
{
    public override IExpressionNode VisitFormula(FormulaParser.FormulaContext context) =>
        Visit(context.expr());

    public override IExpressionNode VisitParenExpr(FormulaParser.ParenExprContext context) =>
        Visit(context.expr());

    public override IExpressionNode VisitUnaryMinusExpr(FormulaParser.UnaryMinusExprContext context) =>
        new UnaryMinusNode(Visit(context.expr()));

    public override IExpressionNode VisitPowerExpr(FormulaParser.PowerExprContext context) =>
        new BinaryOperationNode(Visit(context.expr(0)), BinaryOperator.Power, Visit(context.expr(1)));

    public override IExpressionNode VisitMulDivExpr(FormulaParser.MulDivExprContext context)
    {
        var op = context.op.Text == "*" ? BinaryOperator.Multiply : BinaryOperator.Divide;
        return new BinaryOperationNode(Visit(context.expr(0)), op, Visit(context.expr(1)));
    }

    public override IExpressionNode VisitAddSubExpr(FormulaParser.AddSubExprContext context)
    {
        var op = context.op.Text == "+" ? BinaryOperator.Add : BinaryOperator.Subtract;
        return new BinaryOperationNode(Visit(context.expr(0)), op, Visit(context.expr(1)));
    }

    public override IExpressionNode VisitComparisonExpr(FormulaParser.ComparisonExprContext context)
    {
        var op = context.op.Text switch
        {
            "=" => BinaryOperator.Equal,
            "<>" => BinaryOperator.NotEqual,
            "<=" => BinaryOperator.LessOrEqual,
            ">=" => BinaryOperator.GreaterOrEqual,
            "<" => BinaryOperator.LessThan,
            ">" => BinaryOperator.GreaterThan,
            _ => throw new System.InvalidOperationException($"Unrecognised comparison operator '{context.op.Text}'")
        };
        return new BinaryOperationNode(Visit(context.expr(0)), op, Visit(context.expr(1)));
    }

    public override IExpressionNode VisitFunctionCallExpr(FormulaParser.FunctionCallExprContext context)
    {
        var functionName = context.IDENTIFIER().GetText();
        var argumentNodes = context.argList()?.expr()
            .Select(Visit)
            .ToList() ?? new System.Collections.Generic.List<IExpressionNode>();

        return new FunctionCallNode(functionName, argumentNodes);
    }

    public override IExpressionNode VisitRangeExpr(FormulaParser.RangeExprContext context) =>
        new RangeNode(context.GetText());

    public override IExpressionNode VisitCellRefExpr(FormulaParser.CellRefExprContext context) =>
        new CellReferenceNode(context.GetText());

    public override IExpressionNode VisitNumberExpr(FormulaParser.NumberExprContext context) =>
        new NumberLiteralNode(double.Parse(context.GetText(), CultureInfo.InvariantCulture));

    public override IExpressionNode VisitStringExpr(FormulaParser.StringExprContext context)
    {
        var raw = context.GetText();               // includes surrounding quotes, e.g. "\"high\""
        var unquoted = raw.Substring(1, raw.Length - 2);
        return new TextLiteralNode(unquoted);
    }
}
