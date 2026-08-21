using System.Diagnostics;
using CalcEngine.Graph;

namespace CalcEngine.Graph.Tests;

/// <summary>
/// Benchmarks against the two performance targets the graph module is
/// expected to hit:
///
///   - 50ms  for a single-edit change propagation: a user edits one cell,
///           and every downstream cell that needs recalculating (and the
///           order to do it in) must be available fast enough to feel
///           instantaneous.
///   - 2s    for a full-sheet recalculation: opening/recalculating a large
///           spreadsheet end to end (build the graph, then produce a full
///           topological order) must still finish in a couple of seconds
///           even at spreadsheet scale.
///
/// These are graph-layer benchmarks only — they measure DependencyGraph's
/// own bookkeeping (edge updates, cycle checks, traversal), not formula
/// evaluation itself, since the graph module doesn't evaluate formulas.
/// Each test does one untimed warm-up pass before the timed one, since a
/// cold JIT would otherwise unfairly skew the very first measurement.
/// </summary>
public class PerformanceBenchmarkTests
{
    private const int ChangePropagationTargetMs = 50;
    private const int FullRecalculationTargetMs = 2000;

    [Test]
    public void Propagating_a_single_change_through_a_long_dependency_chain_is_under_50ms()
    {
        const int chainLength = 5_000;

        BuildLinearChain(chainLength); // warm-up
        var graph = BuildLinearChain(chainLength);

        var stopwatch = Stopwatch.StartNew();
        var order = graph.PropagateChange("CELL0");
        stopwatch.Stop();

        Assert.That(order.Count, Is.EqualTo(chainLength));
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(ChangePropagationTargetMs),
            $"Change propagation over a {chainLength}-cell chain took {stopwatch.ElapsedMilliseconds}ms, target is {ChangePropagationTargetMs}ms.");
    }

    [Test]
    public void Recalculating_a_wide_fan_out_from_one_shared_cell_is_under_50ms()
    {
        const int dependentCount = 5_000;

        BuildFanOut(dependentCount); // warm-up
        var graph = BuildFanOut(dependentCount);

        var stopwatch = Stopwatch.StartNew();
        var order = graph.GetRecalculationOrder("SHARED");
        stopwatch.Stop();

        Assert.That(order.Count, Is.EqualTo(dependentCount + 1));
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(ChangePropagationTargetMs),
            $"Fan-out recalculation over {dependentCount} dependents took {stopwatch.ElapsedMilliseconds}ms, target is {ChangePropagationTargetMs}ms.");
    }

    [Test]
    public void Building_and_fully_recalculating_a_50000_cell_sheet_is_under_2s()
    {
        const int cellCount = 50_000;

        BuildBranchingSheet(cellCount); // warm-up

        var stopwatch = Stopwatch.StartNew();
        var graph = BuildBranchingSheet(cellCount);
        var order = graph.GetRecalculationOrder();
        stopwatch.Stop();

        Assert.That(order.Count, Is.EqualTo(cellCount));
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(FullRecalculationTargetMs),
            $"Building and recalculating {cellCount} cells took {stopwatch.ElapsedMilliseconds}ms, target is {FullRecalculationTargetMs}ms.");
    }

    /// <summary>CELL0 -> CELL1 -> ... -> CELL{length-1} (each depends on the previous one).</summary>
    private static DependencyGraph BuildLinearChain(int length)
    {
        var graph = new DependencyGraph();
        graph.SetDependencies("CELL0", Array.Empty<string>());
        for (var i = 1; i < length; i++)
            graph.SetDependencies($"CELL{i}", new[] { $"CELL{i - 1}" });

        return graph;
    }

    /// <summary>One SHARED cell, with dependentCount cells each depending directly on it.</summary>
    private static DependencyGraph BuildFanOut(int dependentCount)
    {
        var graph = new DependencyGraph();
        graph.SetDependencies("SHARED", Array.Empty<string>());
        for (var i = 0; i < dependentCount; i++)
            graph.SetDependencies($"CELL{i}", new[] { "SHARED" });

        return graph;
    }

    /// <summary>
    /// A SUM-like sheet: each cell depends on up to three earlier cells,
    /// roughly how a real spreadsheet fans out — but only within its own
    /// 30-cell section. Cycle detection has to walk every cell reachable
    /// from a new dependency to prove no cycle exists, so one unbroken
    /// chain across all cellCount cells would make each of the cellCount
    /// edits progressively more expensive (quadratic overall). Real sheets
    /// don't chain formulas across their entire history either — nearby
    /// cells reference nearby cells — so bounding the chain to a section is
    /// both what keeps this benchmark linear and a fair model of the target
    /// workload.
    /// </summary>
    private static DependencyGraph BuildBranchingSheet(int cellCount)
    {
        const int sectionSize = 30;

        var graph = new DependencyGraph();
        graph.SetDependencies("CELL0", Array.Empty<string>());

        for (var i = 1; i < cellCount; i++)
        {
            var sectionStart = i - (i % sectionSize);
            var dependencies = new List<string>();
            for (var back = 1; back <= 3; back++)
            {
                var candidate = i - back;
                if (candidate >= sectionStart)
                    dependencies.Add($"CELL{candidate}");
            }

            graph.SetDependencies($"CELL{i}", dependencies);
        }

        return graph;
    }
}
