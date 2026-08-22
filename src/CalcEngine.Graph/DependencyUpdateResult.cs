namespace CalcEngine.Graph;

/// <summary>
/// The result of attempting to set a cell's dependencies. Mirrors
/// CalcEngine.Grammar.Errors.ParseResult's shape deliberately: "the formula
/// the user typed would create a circular reference" is normal input the
/// caller branches on, never an exception it has to catch.
///
/// ABSTRACT DATA TYPE
/// -------------------
/// Abstraction function:
///     AF(rep) = either a successfully-applied edge update (Success == true,
///               Cycle empty), or a rejected update because it would have
///               introduced a cycle (Success == false, Cycle holds the exact
///               path that would have been created, first and last element
///               equal, e.g. ["C1", "A1", "B1", "C1"] for C1 -> A1 -> B1 -> C1).
///
/// Representation invariant:
///     Success == true  ⟺ Cycle.Count == 0
///     Success == false ⟺ Cycle.Count >= 2 AND Cycle[0] == Cycle[^1]
///
/// On a rejected update, DependencyGraph leaves the graph exactly as it was
/// before the call — the caller can retry with a different formula without
/// worrying about partial state.
/// </summary>
public sealed class DependencyUpdateResult
{
    public bool Success { get; }
    public IReadOnlyList<string> Cycle { get; }

    private DependencyUpdateResult(bool success, IReadOnlyList<string> cycle)
    {
        Success = success;
        Cycle = cycle;
    }

    public static DependencyUpdateResult Ok() => new(true, Array.Empty<string>());

    public static DependencyUpdateResult CycleDetected(IReadOnlyList<string> cycle) => new(false, cycle);

    /// <summary>
    /// A ready-to-use "#CYCLE!" message in the style CellValue.Error(...)
    /// expects, e.g. "#CYCLE! (C1 -> A1 -> B1 -> C1)".
    /// </summary>
    public override string ToString() =>
        Success ? "OK" : $"#CYCLE! ({string.Join(" -> ", Cycle)})";
}
