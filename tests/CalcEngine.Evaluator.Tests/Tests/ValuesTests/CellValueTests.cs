using CalcEngine.Evaluator.Values;
using NUnit.Framework;

namespace TestEvaluator.CalcEngine.Evaluator.Tests.ValuesTests
{
    [TestFixture]
    public class CellValueTests
    {
        [Test]
        public void Number_StoresValue_AndReportsCorrectKind()
        {
            var value = CellValue.Number(3.5);

            Assert.That(value.Kind, Is.EqualTo(CellValueKind.Number));
            Assert.That(value.NumberValue, Is.EqualTo(3.5));
            Assert.That(value.IsError, Is.False);
        }

        [Test]
        public void Error_CarriesErrorType_AndDisplaysSpreadsheetStyleText()
        {
            var value = CellValue.Error(CellErrorType.DivideByZero);

            Assert.That(value.IsError, Is.True);
            Assert.That(value.ErrorType, Is.EqualTo(CellErrorType.DivideByZero));
            Assert.That(value.ToString(), Is.EqualTo("#DIV/0!"));
        }

        [Test]
        public void Empty_HasEmptyKind_AndIsNotAnError()
        {
            Assert.That(CellValue.Empty.Kind, Is.EqualTo(CellValueKind.Empty));
            Assert.That(CellValue.Empty.IsError, Is.False);
        }

        [Test]
        public void TryCoerceToNumber_ParsesNumericText()
        {
            var value = CellValue.Text("42.5");

            Assert.That(value.TryCoerceToNumber(out var number), Is.True);
            Assert.That(number, Is.EqualTo(42.5));
        }

        [Test]
        public void TryCoerceToNumber_FailsForNonNumericText()
        {
            var value = CellValue.Text("hello");

            Assert.That(value.TryCoerceToNumber(out _), Is.False);
        }

        [Test]
        public void TryCoerceToNumber_TreatsBooleanAsOneOrZero()
        {
            Assert.That(CellValue.Boolean(true).TryCoerceToNumber(out var trueNumber), Is.True);
            Assert.That(trueNumber, Is.EqualTo(1));

            Assert.That(CellValue.Boolean(false).TryCoerceToNumber(out var falseNumber), Is.True);
            Assert.That(falseNumber, Is.EqualTo(0));
        }

        [Test]
        public void TryCoerceToBoolean_TreatsNonZeroNumberAsTrue()
        {
            Assert.That(CellValue.Number(7).TryCoerceToBoolean(out var flag), Is.True);
            Assert.That(flag, Is.True);
        }

        [Test]
        public void TryCoerceToBoolean_TreatsZeroAsFalse()
        {
            Assert.That(CellValue.Number(0).TryCoerceToBoolean(out var flag), Is.True);
            Assert.That(flag, Is.False);
        }

        [Test]
        public void TryCoerceToBoolean_FailsForArbitraryText()
        {
            Assert.That(CellValue.Text("yes").TryCoerceToBoolean(out _), Is.False);
        }
    }
}
