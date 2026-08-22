using System.Collections.Generic;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace TestEvaluator.CalcEngine.Evaluator.Tests.TestSupport
{
    /// <summary>
    /// A minimal in-memory ICellValueSource used across the Evaluator test suite so
    /// these tests never need the real Graph or GUI modules to exist -- exactly the
    /// point of coding the Evaluator against ICellValueSource instead of a concrete
    /// grid class. Set() populates a cell; unset cells read as CellValue.Empty.
    /// </summary>
    public sealed class FakeCellValueSource : ICellValueSource
    {
        private readonly Dictionary<CellAddress, CellValue> _values = new();

        public void Set(CellAddress address, CellValue value) => _values[address] = value;

        public CellValue GetValue(CellAddress address) =>
            _values.TryGetValue(address, out var value) ? value : CellValue.Empty;
    }
}
