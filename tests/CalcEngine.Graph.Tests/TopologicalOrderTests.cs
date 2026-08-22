using CalcEngine.Graph;

namespace CalcEngine.Graph.Tests;

public class TopologicalOrderTests
{
    [Test]
    public void Full_order_places_every_dependency_before_its_dependent()
    {
        var graph = new DependencyGraph();
        graph.SetDependencies("C1", new[] { "B1" });
        graph.SetDependencies("B1", new[] { "A1" });
        graph.SetDependencies("D1", new[] { "B1", "C1" });

        var order = graph.GetRecalculationOrder();

        AssertRespectsDependencyOrder(graph, order);
        Assert.That(order, Is.EquivalentTo(new[] { "A1", "B1", "C1", "D1" }));
    }

    [Test]
    public void Full_order_includes_every_tracked_cell_exactly_once()
    {
        var graph = new DependencyGraph();
        graph.SetDependencies("C1", new[] { "B1", "A1" });
        graph.SetDependencies("D1", new[] { "A1" });

        var order = graph.GetRecalculationOrder();

        Assert.That(order.Count, Is.EqualTo(graph.CellCount));
        Assert.That(order.Distinct().Count(), Is.EqualTo(order.Count));
    }

    [Test]
    public void Scoped_order_for_a_single_change_only_includes_the_cell_and_its_transitive_dependents()
    {
        var graph = new DependencyGraph();
        // A1 <- B1 <- C1   (C1 depends on B1, B1 depends on A1)
        // D1 is unrelated.
        graph.SetDependencies("B1", new[] { "A1" });
        graph.SetDependencies("C1", new[] { "B1" });
        graph.SetDependencies("D1", new[] { "Z9" });

        var order = graph.GetRecalculationOrder("A1");

        Assert.That(order, Is.EquivalentTo(new[] { "A1", "B1", "C1" }));
    }

    [Test]
    public void Scoped_order_for_a_leaf_with_no_dependents_is_just_that_cell()
    {
        var graph = new DependencyGraph();
        graph.SetDependencies("B1", new[] { "A1" });

        var order = graph.GetRecalculationOrder("B1");

        Assert.That(order, Is.EqualTo(new[] { "B1" }));
    }

    [Test]
    public void Scoped_order_over_multiple_changed_cells_merges_their_affected_sets()
    {
        var graph = new DependencyGraph();
        graph.SetDependencies("B1", new[] { "A1" });
        graph.SetDependencies("D1", new[] { "C1" });

        var order = graph.GetRecalculationOrder(new[] { "A1", "C1" });

        Assert.That(order, Is.EquivalentTo(new[] { "A1", "B1", "C1", "D1" }));
    }

    [Test]
    public void Changing_a_cell_no_one_depends_on_yet_still_returns_that_cell_alone()
    {
        var graph = new DependencyGraph();

        var order = graph.GetRecalculationOrder("Z99");

        Assert.That(order, Is.EqualTo(new[] { "Z99" }));
    }

    private static void AssertRespectsDependencyOrder(DependencyGraph graph, IReadOnlyList<string> order)
    {
        var position = order.Select((cell, index) => (cell, index)).ToDictionary(x => x.cell, x => x.index);

        foreach (var cell in order)
            foreach (var dependency in graph.GetDependencies(cell))
                Assert.That(position[dependency], Is.LessThan(position[cell]),
                    $"{dependency} must be recalculated before {cell}");
    }
}
