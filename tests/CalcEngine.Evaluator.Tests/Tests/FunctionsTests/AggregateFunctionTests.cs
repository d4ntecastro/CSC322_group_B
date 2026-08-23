using CalcEngine.Evaluator.Evaluation;
using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Functions;
using CalcEngine.Evaluator.Values;
using NUnit.Framework;
using CalcEngine.Evaluator.Tests.TestSupport;

namespace CalcEngine.Evaluator.Tests.FunctionsTests
{
    [TestFixture]
    public class AggregateFunctionTests
    {
        private static IEvaluationContext CreateContext(FakeCellValueSource source) =>
            new EvaluationContext(source, new TreeWalkingEvaluator(), FunctionRegistry.CreateDefault());

        [Test]
        public void Sum_OverRange_AddsAllNumbers()
        {
            var source = new FakeCellValueSource();
            source.Set(new CellAddress(0, 0), CellValue.Number(2));
            source.Set(new CellAddress(1, 0), CellValue.Number(4));
            source.Set(new CellAddress(2, 0), CellValue.Number(6));
            var range = new RangeReferenceNode(new CellAddress(0, 0), new CellAddress(2, 0));
            var context = CreateContext(source);

            var node = new FunctionCallNode("SUM", new IExpressionNode[] { range });

            Assert.That(context.Evaluate(node).NumberValue, Is.EqualTo(12));
        }

        [Test]
        public void Sum_MixesRangeAndLiteralArguments()
        {
            var source = new FakeCellValueSource();
            source.Set(new CellAddress(0, 0), CellValue.Number(2));
            source.Set(new CellAddress(1, 0), CellValue.Number(4));
            var range = new RangeReferenceNode(new CellAddress(0, 0), new CellAddress(1, 0));
            var context = CreateContext(source);

            var node = new FunctionCallNode("SUM", new IExpressionNode[] { range, new NumberLiteralNode(10) });

            Assert.That(context.Evaluate(node).NumberValue, Is.EqualTo(16));
        }

        [Test]
        public void Average_OverRange_DividesByCount()
        {
            var source = new FakeCellValueSource();
            source.Set(new CellAddress(0, 0), CellValue.Number(2));
            source.Set(new CellAddress(1, 0), CellValue.Number(4));
            source.Set(new CellAddress(2, 0), CellValue.Number(6));
            var range = new RangeReferenceNode(new CellAddress(0, 0), new CellAddress(2, 0));
            var context = CreateContext(source);

            var node = new FunctionCallNode("AVERAGE", new IExpressionNode[] { range });

            Assert.That(context.Evaluate(node).NumberValue, Is.EqualTo(4));
        }

        [Test]
        public void Average_OfEmptyRange_ReturnsDivideByZeroError()
        {
            var range = new RangeReferenceNode(new CellAddress(0, 0), new CellAddress(2, 0));
            var context = CreateContext(new FakeCellValueSource());

            var node = new FunctionCallNode("AVERAGE", new IExpressionNode[] { range });
            var result = context.Evaluate(node);

            Assert.That(result.IsError, Is.True);
            Assert.That(result.ErrorType, Is.EqualTo(CellErrorType.DivideByZero));
        }

        [Test]
        public void Min_And_Max_OverRange_ReturnExtremes()
        {
            var source = new FakeCellValueSource();
            source.Set(new CellAddress(0, 0), CellValue.Number(2));
            source.Set(new CellAddress(1, 0), CellValue.Number(4));
            source.Set(new CellAddress(2, 0), CellValue.Number(6));
            var range = new RangeReferenceNode(new CellAddress(0, 0), new CellAddress(2, 0));
            var context = CreateContext(source);

            var minResult = context.Evaluate(new FunctionCallNode("MIN", new IExpressionNode[] { range }));
            var maxResult = context.Evaluate(new FunctionCallNode("MAX", new IExpressionNode[] { range }));

            Assert.That(minResult.NumberValue, Is.EqualTo(2));
            Assert.That(maxResult.NumberValue, Is.EqualTo(6));
        }

        [Test]
        public void Count_OverRange_CountsNumericCellsOnly()
        {
            var source = new FakeCellValueSource();
            source.Set(new CellAddress(0, 0), CellValue.Number(1));
            source.Set(new CellAddress(1, 0), CellValue.Text("skip me"));
            source.Set(new CellAddress(2, 0), CellValue.Number(3));
            var range = new RangeReferenceNode(new CellAddress(0, 0), new CellAddress(2, 0));
            var context = CreateContext(source);

            var node = new FunctionCallNode("COUNT", new IExpressionNode[] { range });

            Assert.That(context.Evaluate(node).NumberValue, Is.EqualTo(2));
        }

        [Test]
        public void Sum_PropagatesErrorFromWithinRange()
        {
            var source = new FakeCellValueSource();
            source.Set(new CellAddress(0, 0), CellValue.Number(1));
            source.Set(new CellAddress(1, 0), CellValue.Error(CellErrorType.DivideByZero));
            var range = new RangeReferenceNode(new CellAddress(0, 0), new CellAddress(1, 0));
            var context = CreateContext(source);

            var node = new FunctionCallNode("SUM", new IExpressionNode[] { range });
            var result = context.Evaluate(node);

            Assert.That(result.IsError, Is.True);
            Assert.That(result.ErrorType, Is.EqualTo(CellErrorType.DivideByZero));
        }

        [Test]
        public void UnknownFunctionName_ReturnsNameNotFoundError()
        {
            var context = CreateContext(new FakeCellValueSource());

            var node = new FunctionCallNode("NOTAREALFUNCTION", new IExpressionNode[] { new NumberLiteralNode(1) });
            var result = context.Evaluate(node);

            Assert.That(result.IsError, Is.True);
            Assert.That(result.ErrorType, Is.EqualTo(CellErrorType.NameNotFound));
        }
    }
}
