namespace CalcEngine.Graph;

/// <summary>
/// Observer role in the Observer pattern: anything that needs to react when
/// a cell's value becomes stale (its own value changed, or something it
/// transitively depends on changed) implements this. The real evaluator
/// subscribes so it knows which cells to recompute; the GUI could subscribe
/// too, e.g. to flash a "recalculating" indicator, without either one
/// needing to know how the other reacts.
///
/// DependencyGraph (the Subject) guarantees observers are notified in
/// dependency order — a cell's OnCellInvalidated always fires after every
/// cell it depends on has already fired — so an observer that recomputes
/// eagerly on each callback always sees fresh precedent values.
/// </summary>
public interface ICellChangeObserver
{
    /// <summary>
    /// Called once per affected cell, in recalculation order, during
    /// DependencyGraph.PropagateChange(s). cellReference is normalised
    /// (see DependencyGraph's normalisation rule).
    /// </summary>
    void OnCellInvalidated(string cellReference);
}
