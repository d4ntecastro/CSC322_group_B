using CalcEngine.Evaluator.Evaluation;
using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Functions;
using CalcEngine.Evaluator.Tests.TestSupport;
using CalcEngine.Evaluator.Values;
using NUnit.Framework;

namespace TestEvaluator.CalcEngine.Evaluator.Tests.FunctionsTests
{
    [TestFixture]
    public class RoundFunctionTests
    {
        private static IEvaluationContext CreateContext() =>
            new EvaluationContext(new FakeCellValueSource(), new TreeWalkingEvaluator(), FunctionRegistry.CreateDefault());

        [Test]
        public void Round_ToTwoDecimalPlaces_RoundsAwayFromZero()
        {
            var node = new FunctionCallNode("ROUND", new IExpressionNode[] { new NumberLiteralNode(3.14159), new NumberLiteralNode(2) });

            Assert.That(CreateContext().Evaluate(node).NumberValue, Is.EqualTo(3.14));
        }

        [Test]
        public void Round_OnHalfwayValue_RoundsAwayFromZero_NotToEven()
        {
            var node = new FunctionCallNode("ROUND", new IExpressionNode[] { new NumberLiteralNode(2.5), new NumberLiteralNode(0) });

            // .NET's default Math.Round uses banker's rounding (2.5 -> 2); spreadsheets
            // round halves away from zero (2.5 -> 3). This test locks in the spreadsheet behaviour.
            Assert.That(CreateContext().Evaluate(node).NumberValue, Is.EqualTo(3));
        }

        [Test]
        public void Round_WithNegativeDigits_RoundsToTensOrHundreds()
        {
            var node = new FunctionCallNode("ROUND", new IExpressionNode[] { new NumberLiteralNode(1250), new NumberLiteralNode(-2) });

            Assert.That(CreateContext().Evaluate(node).NumberValue, Is.EqualTo(1300));
        }

        [Test]
        public void Round_WithZeroDigits_RoundsToNearestInteger()
        {
            var node = new FunctionCallNode("ROUND", new IExpressionNode[] { new NumberLiteralNode(7.6), new NumberLiteralNode(0) });

            Assert.That(CreateContext().Evaluate(node).NumberValue, Is.EqualTo(8));
        }

        [Test]
        public void Round_WithWrongArgumentCount_ReturnsValueError()
        {
            var node = new FunctionCallNode("ROUND", new IExpressionNode[] { new NumberLiteralNode(1) });

            var result = CreateContext().Evaluate(node);

            Assert.That(result.IsError, Is.True);
            Assert.That(result.ErrorType, Is.EqualTo(CellErrorType.InvalidValue));
        }

        [Test]
        public void Round_WithNonNumericArgument_ReturnsValueError()
        {
            var node = new FunctionCallNode("ROUND", new IExpressionNode[] { new TextLiteralNode("abc"), new NumberLiteralNode(2) });

            var result = CreateContext().Evaluate(node);

            Assert.That(result.IsError, Is.True);
            Assert.That(result.ErrorType, Is.EqualTo(CellErrorType.InvalidValue));
        }
    }
}
