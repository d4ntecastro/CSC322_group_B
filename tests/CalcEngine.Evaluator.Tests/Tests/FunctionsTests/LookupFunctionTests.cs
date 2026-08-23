using CalcEngine.Evaluator.Evaluation;
using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Functions;
using CalcEngine.Evaluator.Tests.TestSupport;
using CalcEngine.Evaluator.Values;
using NUnit.Framework;

namespace CalcEngine.Evaluator.Tests.FunctionsTests
{
    [TestFixture]
    public class LookupFunctionTests
    {
        private static (IEvaluationContext context, RangeReferenceNode lookupRange, RangeReferenceNode resultRange) BuildContext()
        {
            var source = new FakeCellValueSource();
            source.Set(new CellAddress(0, 0), CellValue.Text("apple"));
            source.Set(new CellAddress(1, 0), CellValue.Text("banana"));
            source.Set(new CellAddress(2, 0), CellValue.Text("cherry"));
            source.Set(new CellAddress(0, 1), CellValue.Number(10));
            source.Set(new CellAddress(1, 1), CellValue.Number(20));
            source.Set(new CellAddress(2, 1), CellValue.Number(30));

            var lookupRange = new RangeReferenceNode(new CellAddress(0, 0), new CellAddress(2, 0));
            var resultRange = new RangeReferenceNode(new CellAddress(0, 1), new CellAddress(2, 1));
            var context = new EvaluationContext(source, new TreeWalkingEvaluator(), FunctionRegistry.CreateDefault());

            return (context, lookupRange, resultRange);
        }

        [Test]
        public void Lookup_FindsMatchingRow_ReturnsCorrespondingResult()
        {
            var (context, lookupRange, resultRange) = BuildContext();

            var node = new FunctionCallNode("LOOKUP", new IExpressionNode[] { new TextLiteralNode("banana"), lookupRange, resultRange });

            Assert.That(context.Evaluate(node).NumberValue, Is.EqualTo(20));
        }

        [Test]
        public void Lookup_MatchIsCaseInsensitive()
        {
            var (context, lookupRange, resultRange) = BuildContext();

            var node = new FunctionCallNode("LOOKUP", new IExpressionNode[] { new TextLiteralNode("BANANA"), lookupRange, resultRange });

            Assert.That(context.Evaluate(node).NumberValue, Is.EqualTo(20));
        }

        [Test]
        public void Lookup_NoMatch_ReturnsNotAvailableError()
        {
            var (context, lookupRange, resultRange) = BuildContext();

            var node = new FunctionCallNode("LOOKUP", new IExpressionNode[] { new TextLiteralNode("durian"), lookupRange, resultRange });
            var result = context.Evaluate(node);

            Assert.That(result.IsError, Is.True);
            Assert.That(result.ErrorType, Is.EqualTo(CellErrorType.NotAvailable));
        }

        [Test]
        public void Lookup_MismatchedRangeSizes_ReturnsReferenceError()
        {
            var source = new FakeCellValueSource();
            var lookupRange = new RangeReferenceNode(new CellAddress(0, 0), new CellAddress(2, 0));
            var resultRange = new RangeReferenceNode(new CellAddress(0, 1), new CellAddress(1, 1)); // deliberately shorter
            var context = new EvaluationContext(source, new TreeWalkingEvaluator(), FunctionRegistry.CreateDefault());

            var node = new FunctionCallNode("LOOKUP", new IExpressionNode[] { new TextLiteralNode("apple"), lookupRange, resultRange });
            var result = context.Evaluate(node);

            Assert.That(result.IsError, Is.True);
            Assert.That(result.ErrorType, Is.EqualTo(CellErrorType.InvalidReference));
        }

        [Test]
        public void Lookup_SecondOrThirdArgumentNotARange_ReturnsReferenceError()
        {
            var context = new EvaluationContext(new FakeCellValueSource(), new TreeWalkingEvaluator(), FunctionRegistry.CreateDefault());

            var node = new FunctionCallNode("LOOKUP", new IExpressionNode[]
            {
                new TextLiteralNode("apple"),
                new NumberLiteralNode(1), // not a range
                new NumberLiteralNode(2)
            });

            var result = context.Evaluate(node);

            Assert.That(result.IsError, Is.True);
            Assert.That(result.ErrorType, Is.EqualTo(CellErrorType.InvalidReference));
        }
    }
}
