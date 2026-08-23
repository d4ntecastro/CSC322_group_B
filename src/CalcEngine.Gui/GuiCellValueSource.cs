using System.Collections.Generic;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Gui
{
    /// <summary>
    /// Minimal in-memory ICellValueSource used by the GUI for demo purposes.
    /// Stores evaluated CellValue instances keyed by CellAddress.
    /// </summary>
    public sealed class GuiCellValueSource : ICellValueSource
    {
        private readonly Dictionary<CellAddress, CellValue> _values = new();

        public void Set(CellAddress address, CellValue value) => _values[address] = value;

        public CellValue GetValue(CellAddress address) => _values.TryGetValue(address, out var v) ? v : CellValue.Empty;
    }
}
