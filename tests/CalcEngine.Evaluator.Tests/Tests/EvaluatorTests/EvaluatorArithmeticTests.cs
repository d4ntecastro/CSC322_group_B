using System;
using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Evaluation;
using CalcEngine.Evaluator.Functions;
using CalcEngine.Evaluator.Tests.TestSupport;
using CalcEngine.Evaluator.Values;
using NUnit.Framework;

namespace CalcEngine.Evaluator.Tests.EvaluatorTests
{
    [TestFixture]
    public class EvaluatorArithmeticTests
    {
        private static IEvaluationContext CreateContext() =>
            new EvaluationContext(new FakeCellValueSource(), new TreeWalkingEvaluator(), FunctionRegistry.CreateDefault());

        [Test]
        public void Add_TwoNumberLiterals_ReturnsSum()
        {
            var node = new BinaryOperationNode(BinaryOperator.Add, new NumberLiteralNode(2), new NumberLiteralNode(3));

            var result = CreateContext().Evaluate(node);

            Assert.That(result.Kind, Is.EqualTo(CellValueKind.Number));
            Assert.That(result.NumberValue, Is.EqualTo(5));
        }

        [Test]
        public void Subtract_Multiply_Power_ProduceExpectedResults()
        {
            var context = CreateContext();

            var subtract = new BinaryOperationNode(BinaryOperator.Subtract, new NumberLiteralNode(10), new NumberLiteralNode(4));
            var multiply = new BinaryOperationNode(BinaryOperator.Multiply, new NumberLiteralNode(6), new NumberLiteralNode(7));
            var power = new BinaryOperationNode(BinaryOperator.Power, new NumberLiteralNode(2), new NumberLiteralNode(10));

            Assert.That(context.Evaluate(subtract).NumberValue, Is.EqualTo(6));
            Assert.That(context.Evaluate(multiply).NumberValue, Is.EqualTo(42));
            Assert.That(context.Evaluate(power).NumberValue, Is.EqualTo(1024));
        }

        [Test]
        public void Divide_ByZero_ReturnsDivideByZeroError()
        {
            var node = new BinaryOperationNode(BinaryOperator.Divide, new NumberLiteralNode(10), new NumberLiteralNode(0));

            var result = CreateContext().Evaluate(node);

            Assert.That(result.IsError, Is.True);
            Assert.That(result.ErrorType, Is.EqualTo(CellErrorType.DivideByZero));
        }

        [Test]
        public void ErrorPropagation_LeftOperandError_ShortCircuitsToThatError()
        {
            // An unknown function name evaluates to a #NAME? error -- this proves that
            // error propagates up through a surrounding binary operation.
            var errorNode = new FunctionCallNode("MISSING", Array.Empty<IExpressionNode>());
            var node = new BinaryOperationNode(BinaryOperator.Add, errorNode, new NumberLiteralNode(1));

            var result = CreateContext().Evaluate(node);

            Assert.That(result.IsError, Is.True);
            Assert.That(result.ErrorType, Is.EqualTo(CellErrorType.NameNotFound));
        }

        [Test]
        public void Comparison_ReturnsBooleanValue()
        {
            var node = new BinaryOperationNode(BinaryOperator.GreaterThan, new NumberLiteralNode(5), new NumberLiteralNode(3));

            var result = CreateContext().Evaluate(node);

            Assert.That(result.Kind, Is.EqualTo(CellValueKind.Boolean));
            Assert.That(result.BooleanValue, Is.True);
        }

        [Test]
        public void Concatenate_JoinsDisplayTextOfBothSides()
        {
            var node = new BinaryOperationNode(BinaryOperator.Concatenate, new TextLiteralNode("Row "), new NumberLiteralNode(1));

            var result = CreateContext().Evaluate(node);

            Assert.That(result.Kind, Is.EqualTo(CellValueKind.Text));
            Assert.That(result.TextValue, Is.EqualTo("Row 1"));
        }

        [Test]
        public void UnaryNegate_FlipsSign()
        {
            var node = new UnaryOperationNode(UnaryOperator.Negate, new NumberLiteralNode(4));

            Assert.That(CreateContext().Evaluate(node).NumberValue, Is.EqualTo(-4));
        }

        [Test]
        public void UnaryPercent_DividesByOneHundred()
        {
            var node = new UnaryOperationNode(UnaryOperator.Percent, new NumberLiteralNode(50));

            Assert.That(CreateContext().Evaluate(node).NumberValue, Is.EqualTo(0.5));
        }

        [Test]
        public void Add_TextThatIsNotNumeric_ReturnsValueError()
        {
            var node = new BinaryOperationNode(BinaryOperator.Add, new TextLiteralNode("abc"), new NumberLiteralNode(1));

            var result = CreateContext().Evaluate(node);

            Assert.That(result.IsError, Is.True);
            Assert.That(result.ErrorType, Is.EqualTo(CellErrorType.InvalidValue));
        }

        [Test]
        public void Evaluate_NullRoot_ReturnsValueErrorInsteadOfThrowing()
        {
            var result = new TreeWalkingEvaluator().Evaluate(null!, CreateContext());

            Assert.That(result.IsError, Is.True);
        }
    }
}
