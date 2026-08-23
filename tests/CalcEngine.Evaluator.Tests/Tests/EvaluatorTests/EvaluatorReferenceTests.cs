using CalcEngine.Evaluator.Evaluation;
using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Functions;
using CalcEngine.Evaluator.Values;
using NUnit.Framework;
using CalcEngine.Evaluator.Tests.TestSupport;

namespace CalcEngine.Evaluator.Tests.EvaluatorTests
{
    [TestFixture]
    public class EvaluatorReferenceTests
    {
        private static IEvaluationContext CreateContext(FakeCellValueSource source) =>
            new EvaluationContext(source, new TreeWalkingEvaluator(), FunctionRegistry.CreateDefault());

        [Test]
        public void CellReference_ReadsValueFromSource()
        {
            var source = new FakeCellValueSource();
            source.Set(new CellAddress(0, 0), CellValue.Number(9));
            var context = CreateContext(source);

            var result = context.Evaluate(new CellReferenceNode(new CellAddress(0, 0)));

            Assert.That(result.NumberValue, Is.EqualTo(9));
        }

        [Test]
        public void EmptyCellReference_EvaluatesToEmptyNotError()
        {
            var context = CreateContext(new FakeCellValueSource());

            var result = context.Evaluate(new CellReferenceNode(new CellAddress(5, 5)));

            Assert.That(result.Kind, Is.EqualTo(CellValueKind.Empty));
            Assert.That(result.IsError, Is.False);
        }

        [Test]
        public void RangeReference_UsedDirectlyOutsideFunction_ReturnsValueError()
        {
            var context = CreateContext(new FakeCellValueSource());
            var node = new RangeReferenceNode(new CellAddress(0, 0), new CellAddress(0, 2));

            var result = context.Evaluate(node);

            Assert.That(result.IsError, Is.True);
            Assert.That(result.ErrorType, Is.EqualTo(CellErrorType.InvalidValue));
        }

        [Test]
        public void GetRangeValues_CoversFullRectangle_RegardlessOfCornerOrder()
        {
            var source = new FakeCellValueSource();
            source.Set(new CellAddress(0, 0), CellValue.Number(1));
            source.Set(new CellAddress(0, 1), CellValue.Number(2));
            source.Set(new CellAddress(1, 0), CellValue.Number(3));
            source.Set(new CellAddress(1, 1), CellValue.Number(4));
            var context = CreateContext(source);

            // Deliberately given "backwards" (bottom-right to top-left) to prove the
            // range normalizes corner order the way a user dragging a selection would expect.
            var range = new RangeReferenceNode(new CellAddress(1, 1), new CellAddress(0, 0));

            var values = context.GetRangeValues(range.Start, range.End);
            var total = 0.0;
            foreach (var v in values) total += v.NumberValue;

            Assert.That(total, Is.EqualTo(10));
        }
    }
}
