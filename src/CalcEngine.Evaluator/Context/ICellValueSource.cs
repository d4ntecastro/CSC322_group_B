using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Context
{
    /// <summary>
    /// The one method the Evaluator module needs from "the grid" -- whatever object
    /// actually stores cell contents (owned by Thomson's Graph module and/or Joel's
    /// GUI module). By depending on this tiny interface instead of a concrete grid
    /// class, the Evaluator can be built, tested, and demoed completely on its own
    /// before the Graph/GUI modules are ready -- see TestSupport/FakeCellValueSource.cs
    /// in the test project for the stand-in used throughout this module's own tests.
    ///
    /// At integration time, whatever class holds the real grid (or wraps the
    /// dependency graph) just needs to implement this one method.
    /// </summary>
    public interface ICellValueSource
    {
        /// <summary>
        /// The current value of the cell at <paramref name="address"/>. Must return
        /// CellValue.Empty for a cell that has never been set -- never throw for an
        /// out-of-range or unused address.
        /// </summary>
        CellValue GetValue(CellAddress address);
    }
}
