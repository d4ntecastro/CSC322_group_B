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
    public CellValue GetCellValue(string cellReference)
    {
        return CellValue.Number(0);
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
