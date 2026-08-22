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
    public class IfFunctionTests
    {
        private static IEvaluationContext CreateContext() =>
            new EvaluationContext(new FakeCellValueSource(), new TreeWalkingEvaluator(), FunctionRegistry.CreateDefault());

        [Test]
        public void If_TrueCondition_ReturnsTrueBranch()
        {
            var node = new FunctionCallNode("IF", new IExpressionNode[]
            {
                new NumberLiteralNode(1),
                new NumberLiteralNode(100),
                new NumberLiteralNode(200)
            });

            Assert.That(CreateContext().Evaluate(node).NumberValue, Is.EqualTo(100));
        }

        [Test]
        public void If_FalseCondition_ReturnsFalseBranch()
        {
            var node = new FunctionCallNode("IF", new IExpressionNode[]
            {
                new NumberLiteralNode(0),
                new NumberLiteralNode(100),
                new NumberLiteralNode(200)
            });

            Assert.That(CreateContext().Evaluate(node).NumberValue, Is.EqualTo(200));
        }

        [Test]
        public void If_FalseConditionWithNoFalseBranch_ReturnsBooleanFalse()
        {
            var node = new FunctionCallNode("IF", new IExpressionNode[]
            {
                new NumberLiteralNode(0),
                new NumberLiteralNode(100)
            });

            var result = CreateContext().Evaluate(node);

            Assert.That(result.Kind, Is.EqualTo(CellValueKind.Boolean));
            Assert.That(result.BooleanValue, Is.False);
        }

        [Test]
        public void If_DoesNotEvaluateTheBranchItDidNotTake()
        {
            var context = CreateContext();
            var untakenBranch = new ThrowingNode();

            var node = new FunctionCallNode("IF", new IExpressionNode[]
            {
                new NumberLiteralNode(1),
                new NumberLiteralNode(42),
                untakenBranch
            });

            var result = context.Evaluate(node);

            Assert.That(result.NumberValue, Is.EqualTo(42));
            Assert.That(untakenBranch.WasEvaluated, Is.False);
        }

        [Test]
        public void If_ErrorInCondition_PropagatesWithoutEvaluatingEitherBranch()
        {
            var context = CreateContext();
            var trueBranch = new ThrowingNode();
            var falseBranch = new ThrowingNode();

            var node = new FunctionCallNode("IF", new IExpressionNode[]
            {
                new FunctionCallNode("MISSING", Array.Empty<IExpressionNode>()), // -> #NAME?
                trueBranch,
                falseBranch
            });

            var result = context.Evaluate(node);

            Assert.That(result.IsError, Is.True);
            Assert.That(result.ErrorType, Is.EqualTo(CellErrorType.NameNotFound));
            Assert.That(trueBranch.WasEvaluated, Is.False);
            Assert.That(falseBranch.WasEvaluated, Is.False);
        }

        [Test]
        public void If_WrongArgumentCount_ReturnsValueError()
        {
            var node = new FunctionCallNode("IF", new IExpressionNode[] { new NumberLiteralNode(1) });

            var result = CreateContext().Evaluate(node);

            Assert.That(result.IsError, Is.True);
            Assert.That(result.ErrorType, Is.EqualTo(CellErrorType.InvalidValue));
        }

        /// <summary>Test double that records whether it was ever evaluated and throws if it is -- used to prove IF's lazy branch evaluation without relying on a real division-by-zero side effect.</summary>
        private sealed class ThrowingNode : IExpressionNode
        {
            public bool WasEvaluated { get; private set; }

            public CellValue Evaluate(IEvaluationContext context)
            {
                WasEvaluated = true;
                throw new InvalidOperationException("This branch should never be evaluated.");
            }
        }
    }
}
