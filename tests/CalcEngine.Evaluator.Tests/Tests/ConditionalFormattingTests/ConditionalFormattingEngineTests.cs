using CalcEngine.Evaluator.Evaluation;
using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.ConditionalFormatting;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Functions;
using CalcEngine.Evaluator.Tests.TestSupport;
using CalcEngine.Evaluator.Values;
using NUnit.Framework;

namespace TestEvaluator.CalcEngine.Evaluator.Tests.ConditionalFormattingTests
{
    [TestFixture]
    public class ConditionalFormattingEngineTests
    {
        private static IEvaluationContext CreateContext() =>
            new EvaluationContext(new FakeCellValueSource(), new TreeWalkingEvaluator(), FunctionRegistry.CreateDefault());

        [Test]
        public void GreaterThanRule_AppliesStyle_WhenValueExceedsThreshold()
        {
            var engine = new ConditionalFormattingEngine();
            engine.AddRule(new ConditionalFormatRule(
                "HighValues",
                new CellAddress(0, 0), new CellAddress(4, 0),
                new ComparisonCondition(ComparisonOperator.GreaterThan, 100),
                new FormatStyle(backgroundColorHex: "#FF0000")));

            var result = engine.GetStyleFor(new CellAddress(2, 0), CellValue.Number(150), CreateContext());

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Value.BackgroundColorHex, Is.EqualTo("#FF0000"));
        }

        [Test]
        public void Rule_DoesNotApply_OutsideItsRange()
        {
            var engine = new ConditionalFormattingEngine();
            engine.AddRule(new ConditionalFormatRule(
                "HighValues",
                new CellAddress(0, 0), new CellAddress(4, 0),
                new ComparisonCondition(ComparisonOperator.GreaterThan, 100),
                new FormatStyle()));

            var result = engine.GetStyleFor(new CellAddress(0, 5), CellValue.Number(150), CreateContext());

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Rule_DoesNotApply_WhenConditionFails()
        {
            var engine = new ConditionalFormattingEngine();
            engine.AddRule(new ConditionalFormatRule(
                "HighValues",
                new CellAddress(0, 0), new CellAddress(4, 0),
                new ComparisonCondition(ComparisonOperator.GreaterThan, 100),
                new FormatStyle()));

            var result = engine.GetStyleFor(new CellAddress(1, 0), CellValue.Number(50), CreateContext());

            Assert.That(result, Is.Null);
        }

        [Test]
        public void ConflictingRules_LowerPriorityNumber_Wins()
        {
            var engine = new ConditionalFormattingEngine();
            engine.AddRule(new ConditionalFormatRule("Low", new CellAddress(0, 0), new CellAddress(0, 0),
                new ComparisonCondition(ComparisonOperator.GreaterThan, 0), new FormatStyle(backgroundColorHex: "#FFFF00"), priority: 5));
            engine.AddRule(new ConditionalFormatRule("High", new CellAddress(0, 0), new CellAddress(0, 0),
                new ComparisonCondition(ComparisonOperator.GreaterThan, 0), new FormatStyle(backgroundColorHex: "#00FF00"), priority: 1));

            var result = engine.GetStyleFor(new CellAddress(0, 0), CellValue.Number(1), CreateContext());

            Assert.That(result!.Value.BackgroundColorHex, Is.EqualTo("#00FF00"));
        }

        [Test]
        public void FormulaCondition_EvaluatesArbitraryBooleanExpression()
        {
            var engine = new ConditionalFormattingEngine();
            var condition = new FormulaCondition(new BinaryOperationNode(BinaryOperator.GreaterThan, new NumberLiteralNode(10), new NumberLiteralNode(5)));
            engine.AddRule(new ConditionalFormatRule("AlwaysTrueHere", new CellAddress(0, 0), new CellAddress(0, 0), condition, new FormatStyle(bold: true)));

            var result = engine.GetStyleFor(new CellAddress(0, 0), CellValue.Number(0), CreateContext());

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Value.Bold, Is.True);
        }

        [Test]
        public void RemoveRule_ByName_StopsItFromApplying()
        {
            var engine = new ConditionalFormattingEngine();
            engine.AddRule(new ConditionalFormatRule("HighValues", new CellAddress(0, 0), new CellAddress(4, 0),
                new ComparisonCondition(ComparisonOperator.GreaterThan, 100), new FormatStyle()));

            var removed = engine.RemoveRule("HighValues");
            var result = engine.GetStyleFor(new CellAddress(0, 0), CellValue.Number(150), CreateContext());

            Assert.That(removed, Is.True);
            Assert.That(result, Is.Null);
        }
    }
}
