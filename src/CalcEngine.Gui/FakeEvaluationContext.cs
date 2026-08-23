using System;
using System.Collections.Generic;
using CalcEngine.Grammar.Tree;
using CalcEngine.Grammar.Values;

namespace CalcEngine.Gui;

/// <summary>
/// A temporary fake evaluation context used by the GUI until the full Evaluator module is integrated.
/// </summary>
public class FakeEvaluationContext : IEvaluationContext
{

    private readonly Func<string, string> _getValue;

    public FakeEvaluationContext(Func<string, string> getValue)
    {
        _getValue = getValue;
    }

    public CellValue GetCellValue(string cellReference)
    {
        string rawValue = _getValue(cellReference);
        if (double.TryParse(rawValue, out double num)) return CellValue.Number(num);

        return string.IsNullOrEmpty(rawValue) ? CellValue.Number(0) : CellValue.Text(rawValue);
    }

    public IEnumerable<CellValue> GetRangeValues(string rangeReference)
    {
        yield return CellValue.Number(0);
    }

    public CellValue CallFunction(string functionName, IReadOnlyList<IExpressionNode> arguments)
    {
        return CellValue.Number(0);
    }
}
