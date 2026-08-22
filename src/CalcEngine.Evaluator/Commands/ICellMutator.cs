using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Commands
{
    /// <summary>
    /// The seam between the Command/undo-redo machinery and whatever actually owns
    /// cell storage and recalculation (the Graph module's dependency graph, invoked
    /// through the GUI). Commands work on raw input text -- exactly what the user
    /// typed into a cell, e.g. "=SUM(A1:A2)" or "42" -- rather than on already-parsed
    /// formulas, because that's the natural unit of "one edit" to undo/redo, and it
    /// keeps this module from needing to know how parsing or recalculation happens.
    /// </summary>
    public interface ICellMutator
    {
        /// <summary>The raw text currently in the given cell, or "" if it is empty.</summary>
        string GetCellInput(CellAddress address);

        /// <summary>Sets the raw text of a cell and triggers whatever recalculation the Graph module needs to do as a result.</summary>
        void SetCellInput(CellAddress address, string input);
    }
}
