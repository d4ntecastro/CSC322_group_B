// =============================================================================
// This file belongs in tests/CalcEngine.Grammar.Tests/, NOT in the Grammar
// project itself. It's included here as a starting point so your commit
// history shows tests before/alongside the implementation, per the brief's
// "test first" requirement. Add to it as you build out more of the grammar
// (functions, ranges, nested expressions, error cases, etc.).
// =============================================================================
using CalcEngine.Grammar;
using CalcEngine.Grammar.Values;
using NUnit.Framework;

namespace CalcEngine.Grammar.Tests;

[TestFixture]
public class FormulaParserServiceTests
{
    private FormulaParserService _parser;

    [SetUp]
    public void SetUp()
    {
        _parser = new FormulaParserService();
    }

    [Test]
    public void Parse_SimpleAddition_Succeeds()
    {
        var result = _parser.Parse("=2+3");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Tree.Evaluate(new FakeContext()).NumberValue, Is.EqualTo(5));
    }

    [Test]
    public void Parse_CellReference_ExtractsDependency()
    {
        var result = _parser.Parse("=B2+1");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Tree.GetCellReferences(), Does.Contain("B2"));
    }

    [Test]
    public void Parse_FunctionCall_ParsesArguments()
    {
        var result = _parser.Parse("=SUM(B2:B45)");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Tree.ToString(), Does.Contain("SUM"));
    }

    [Test]
    public void Parse_UnclosedParenthesis_ReturnsHelpfulError()
    {
        var result = _parser.Parse("=(2+3");

        Assert.That(result.Success, Is.False);
        Assert.That(result.Errors.Count, Is.GreaterThan(0));
    }

    [Test]
    public void Evaluate_DivisionByZero_ReturnsErrorValue_NotException()
    {
        var result = _parser.Parse("=5/0");

        Assert.That(result.Success, Is.True);
        var value = result.Tree.Evaluate(new FakeContext());
        Assert.That(value.IsError, Is.True);
        Assert.That(value.ErrorMessage, Does.Contain("DIV/0"));
    }

    /// <summary>
    /// A minimal stand-in for the real evaluation context, used so this
    /// module's tests never depend on Thomson's or Olarewaju's modules.
    /// Replace/extend as needed once the real context implementation exists.
    /// </summary>
    private sealed class FakeContext : CalcEngine.Grammar.Tree.IEvaluationContext
    {
        public CellValue GetCellValue(string cellReference) => CellValue.Number(10);

        public System.Collections.Generic.IEnumerable<CellValue> GetRangeValues(string rangeReference)
        {
            yield return CellValue.Number(1);
            yield return CellValue.Number(2);
        }

        public CellValue CallFunction(string functionName, System.Collections.Generic.IReadOnlyList<CalcEngine.Grammar.Tree.IExpressionNode> arguments) =>
            CellValue.Number(0);
    }
}
