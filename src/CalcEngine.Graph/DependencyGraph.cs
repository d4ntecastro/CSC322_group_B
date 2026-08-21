namespace CalcEngine.Graph;

/// <summary>
/// Tracks which cells depend on which other cells, and turns "cell X's raw
/// value or formula changed" into "here is the exact, correctly-ordered set
/// of cells that must be recalculated" — the one seam the Evaluator and GUI
/// modules need, without either of them re-implementing graph traversal.
///
/// ABSTRACT DATA TYPE
/// -------------------
/// Abstraction function:
///     AF(rep) = a directed graph whose nodes are cell references and whose
///               edge cell -> p ("cell depends on p") exists exactly when p
///               appears in the most recent SetDependencies(cell, ...) call
///               that was not rejected as a cycle.
///
/// Representation invariant:
///     - _dependsOn and _dependents have the same key set at all times
///       (every tracked cell appears in both, even with an empty edge set).
///     - p in _dependsOn[cell]  ⟺  cell in _dependents[p]  (edges are always
///       recorded on both sides, so "who do I depend on" and "who depends on
///       me" are both O(1) lookups instead of one being a linear scan).
///     - The graph is always acyclic: SetDependencies detects and rejects
///       any update that would introduce a cycle before committing it, so
///       every other method below (topological sort in particular) can
///       assume acyclicity and never has to fail because of one.
///
/// Cell references are normalised (trimmed, upper-cased) once, centrally,
/// here — see the rep-invariant note on CellReferenceNode explaining why
/// that normalisation belongs in the graph rather than in the parser: it
/// means "B2" and "b2" are always treated as the same node no matter which
/// casing a given formula happened to use.
///
/// This class is also the Subject half of the Observer pattern (see
/// ICellChangeObserver): PropagateChange(s) walks the affected cells in
/// dependency order and calls every subscribed observer once per cell.
/// </summary>
public sealed class DependencyGraph
{
    // Invariant: same key set in both dictionaries at all times (see class doc).
    private readonly Dictionary<string, HashSet<string>> _dependsOn = new();
    private readonly Dictionary<string, HashSet<string>> _dependents = new();
    private readonly List<ICellChangeObserver> _observers = new();

    public int CellCount => _dependsOn.Count;

    public IReadOnlyCollection<string> Cells => _dependsOn.Keys;

    public bool ContainsCell(string cellReference) =>
        cellReference is not null && _dependsOn.ContainsKey(Normalize(cellReference));

    /// <summary>
    /// Every cell that cellReference's formula directly reads from. Unknown
    /// cells return an empty collection rather than throwing — asking about
    /// a cell nobody has told the graph about yet is normal, not an error.
    /// </summary>
    public IReadOnlyCollection<string> GetDependencies(string cellReference)
    {
        var cell = Normalize(cellReference);
        return _dependsOn.TryGetValue(cell, out var deps) ? deps.ToArray() : Array.Empty<string>();
    }

    /// <summary>Every cell whose formula directly reads from cellReference.</summary>
    public IReadOnlyCollection<string> GetDependents(string cellReference)
    {
        var cell = Normalize(cellReference);
        return _dependents.TryGetValue(cell, out var deps) ? deps.ToArray() : Array.Empty<string>();
    }

    /// <summary>
    /// Replaces cellReference's full set of dependencies in one atomic step
    /// (call this once per parsed formula, with IExpressionNode.GetCellReferences()
    /// as dependsOn). If the new set would create a circular reference, the
    /// graph is left completely unchanged and the exact cycle is reported —
    /// see DependencyUpdateResult.
    /// </summary>
    public DependencyUpdateResult SetDependencies(string cellReference, IEnumerable<string> dependsOn)
    {
        if (dependsOn is null) throw new ArgumentNullException(nameof(dependsOn));

        var cell = Normalize(cellReference);
        var newDeps = dependsOn.Select(Normalize).Distinct().ToList();

        EnsureNode(cell);
        var oldDeps = _dependsOn[cell].ToList();

        ReplaceEdges(cell, newDeps);

        var cycle = FindCycleContaining(cell);
        if (cycle is not null)
        {
            ReplaceEdges(cell, oldDeps);
            return DependencyUpdateResult.CycleDetected(cycle);
        }

        return DependencyUpdateResult.Ok();
    }

    /// <summary>
    /// Drops cellReference from the graph entirely, severing edges in both
    /// directions. Returns false if the cell was never tracked. Cells that
    /// referenced it keep their own node but lose that one dependency edge —
    /// equivalent to the reference now pointing at a genuinely empty cell.
    /// </summary>
    public bool RemoveCell(string cellReference)
    {
        var cell = Normalize(cellReference);
        if (!_dependsOn.TryGetValue(cell, out var deps)) return false;

        foreach (var dep in deps)
            _dependents[dep].Remove(cell);

        foreach (var dependent in _dependents[cell])
            _dependsOn[dependent].Remove(cell);

        _dependsOn.Remove(cell);
        _dependents.Remove(cell);
        return true;
    }

    public void Subscribe(ICellChangeObserver observer)
    {
        if (observer is null) throw new ArgumentNullException(nameof(observer));
        if (!_observers.Contains(observer)) _observers.Add(observer);
    }

    public void Unsubscribe(ICellChangeObserver observer) => _observers.Remove(observer);

    /// <summary>Full recalculation order: every tracked cell, precedents before dependents.</summary>
    public IReadOnlyList<string> GetRecalculationOrder() => TopologicalSort(_dependsOn.Keys);

    /// <summary>
    /// Recalculation order restricted to changedCellReference itself plus
    /// every cell that transitively depends on it — the subset that
    /// actually needs recomputing after a single edit, in the order it must
    /// happen in.
    /// </summary>
    public IReadOnlyList<string> GetRecalculationOrder(string changedCellReference) =>
        GetRecalculationOrder(new[] { changedCellReference });

    public IReadOnlyList<string> GetRecalculationOrder(IEnumerable<string> changedCellReferences)
    {
        if (changedCellReferences is null) throw new ArgumentNullException(nameof(changedCellReferences));
        var affected = CollectTransitiveDependents(changedCellReferences.Select(Normalize));
        return TopologicalSort(affected);
    }

    /// <summary>
    /// Computes the recalculation order for changedCellReference and notifies
    /// every subscribed observer once per affected cell, in that order — the
    /// Observer-pattern change-propagation entry point. Returns the same
    /// order it notified with, so a caller that isn't an observer (e.g. a
    /// test) can still see exactly what happened.
    /// </summary>
    public IReadOnlyList<string> PropagateChange(string changedCellReference) =>
        PropagateChanges(new[] { changedCellReference });

    public IReadOnlyList<string> PropagateChanges(IEnumerable<string> changedCellReferences)
    {
        var order = GetRecalculationOrder(changedCellReferences);
        foreach (var cell in order)
            foreach (var observer in _observers)
                observer.OnCellInvalidated(cell);

        return order;
    }

    private void EnsureNode(string cell)
    {
        if (!_dependsOn.ContainsKey(cell)) _dependsOn[cell] = new HashSet<string>();
        if (!_dependents.ContainsKey(cell)) _dependents[cell] = new HashSet<string>();
    }

    private void ReplaceEdges(string cell, IEnumerable<string> newDependencies)
    {
        foreach (var oldDep in _dependsOn[cell])
            _dependents[oldDep].Remove(cell);

        var newSet = new HashSet<string>(newDependencies);
        _dependsOn[cell] = newSet;

        foreach (var dep in newSet)
        {
            EnsureNode(dep);
            _dependents[dep].Add(cell);
        }
    }

    /// <summary>
    /// Iterative DFS (no recursion, so a 10,000-cell dependency chain can't
    /// blow the call stack) starting at start, following "depends on" edges,
    /// looking for a path back to start. Only start's own outgoing edges
    /// just changed, so any new cycle must pass through it — we don't need
    /// to scan the whole graph, only start's reachable set.
    ///
    /// Returns the exact cycle (start, ..., start) if one exists, else null.
    /// </summary>
    private List<string>? FindCycleContaining(string start)
    {
        var visited = new HashSet<string> { start };
        var path = new List<string> { start };
        var frames = new Stack<IEnumerator<string>>();
        frames.Push(_dependsOn[start].GetEnumerator());

        while (frames.Count > 0)
        {
            var current = frames.Peek();
            if (current.MoveNext())
            {
                var next = current.Current;
                if (next == start)
                {
                    path.Add(next);
                    return path;
                }

                if (visited.Add(next))
                {
                    path.Add(next);
                    frames.Push(_dependsOn[next].GetEnumerator());
                }
            }
            else
            {
                frames.Pop();
                path.RemoveAt(path.Count - 1);
            }
        }

        return null;
    }

    /// <summary>BFS outward along "depends on" (reverse) edges from seeds — everything that would go stale if a seed's value changed.</summary>
    private HashSet<string> CollectTransitiveDependents(IEnumerable<string> seeds)
    {
        var affected = new HashSet<string>();
        var queue = new Queue<string>();

        foreach (var seed in seeds)
        {
            if (!_dependents.ContainsKey(seed)) continue;
            if (affected.Add(seed)) queue.Enqueue(seed);
        }

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            foreach (var dependent in _dependents[node])
            {
                if (affected.Add(dependent))
                    queue.Enqueue(dependent);
            }
        }

        return affected;
    }

    /// <summary>
    /// Kahn's algorithm restricted to the given node subset: repeatedly emit
    /// any node all of whose dependencies (within the subset) have already
    /// been emitted. Used both for the full-graph order (subset == every
    /// cell) and the scoped order (subset == one change's affected cells),
    /// so there's exactly one topological-sort implementation to get right.
    /// </summary>
    private List<string> TopologicalSort(IEnumerable<string> nodes)
    {
        var scope = nodes as HashSet<string> ?? new HashSet<string>(nodes);

        var inDegree = new Dictionary<string, int>(scope.Count);
        foreach (var node in scope)
            inDegree[node] = _dependsOn[node].Count(scope.Contains);

        var queue = new Queue<string>(scope.Where(n => inDegree[n] == 0));
        var order = new List<string>(scope.Count);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            order.Add(node);
            foreach (var dependent in _dependents[node])
            {
                if (!scope.Contains(dependent)) continue;
                if (--inDegree[dependent] == 0) queue.Enqueue(dependent);
            }
        }

        if (order.Count != scope.Count)
            throw new InvalidOperationException(
                "Dependency graph contains a cycle that SetDependencies should have rejected at edit time. " +
                "This is an internal invariant violation, not malformed user input.");

        return order;
    }

    private static string Normalize(string cellReference)
    {
        if (string.IsNullOrWhiteSpace(cellReference))
            throw new ArgumentException("Cell reference cannot be null or empty.", nameof(cellReference));

        return cellReference.Trim().ToUpperInvariant();
    }
}
