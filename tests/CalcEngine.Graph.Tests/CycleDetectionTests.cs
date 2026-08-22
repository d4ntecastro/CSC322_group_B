using CalcEngine.Graph;

namespace CalcEngine.Graph.Tests;

public class CycleDetectionTests
{
    [Test]
    public void Direct_self_reference_is_reported_as_a_one_step_cycle()
    {
        var graph = new DependencyGraph();

        var result = graph.SetDependencies("A1", new[] { "A1" });

        Assert.That(result.Success, Is.False);
        Assert.That(result.Cycle, Is.EqualTo(new[] { "A1", "A1" }));
    }

    [Test]
    public void Two_cell_cycle_reports_the_exact_path()
    {
        var graph = new DependencyGraph();
        graph.SetDependencies("A1", new[] { "B1" });

        var result = graph.SetDependencies("B1", new[] { "A1" });

        Assert.That(result.Success, Is.False);
        Assert.That(result.Cycle, Is.EqualTo(new[] { "B1", "A1", "B1" }));
    }

    [Test]
    public void Longer_cycle_reports_every_cell_on_the_path_in_order()
    {
        var graph = new DependencyGraph();
        // A1 -> B1 -> C1 -> D1, then close the loop D1 -> A1.
        graph.SetDependencies("A1", new[] { "B1" });
        graph.SetDependencies("B1", new[] { "C1" });
        graph.SetDependencies("C1", new[] { "D1" });

        var result = graph.SetDependencies("D1", new[] { "A1" });

        Assert.That(result.Success, Is.False);
        Assert.That(result.Cycle, Is.EqualTo(new[] { "D1", "A1", "B1", "C1", "D1" }));
    }

    [Test]
    public void Rejected_update_leaves_the_graph_completely_unchanged()
    {
        var graph = new DependencyGraph();
        graph.SetDependencies("A1", new[] { "B1" });
        graph.SetDependencies("B1", new[] { "C1" });

        var result = graph.SetDependencies("C1", new[] { "A1" });

        Assert.That(result.Success, Is.False);
        // The rejected edge C1 -> A1 must not have been committed.
        Assert.That(graph.GetDependencies("C1"), Is.Empty);
        Assert.That(graph.GetDependents("A1"), Is.Empty);
        // Everything set up before the rejected call is still intact.
        Assert.That(graph.GetDependencies("A1"), Is.EquivalentTo(new[] { "B1" }));
        Assert.That(graph.GetDependencies("B1"), Is.EquivalentTo(new[] { "C1" }));
    }

    [Test]
    public void A_shared_dependency_that_is_not_part_of_a_cycle_is_accepted()
    {
        var graph = new DependencyGraph();
        graph.SetDependencies("C1", new[] { "A1", "B1" });

        var result = graph.SetDependencies("D1", new[] { "A1", "B1" });

        Assert.That(result.Success, Is.True);
        Assert.That(result.Cycle, Is.Empty);
    }

    [Test]
    public void Diamond_shaped_dependencies_are_not_mistaken_for_a_cycle()
    {
        var graph = new DependencyGraph();
        // A1 depends on B1 and C1; both B1 and C1 depend on D1. No cycle.
        graph.SetDependencies("B1", new[] { "D1" });
        graph.SetDependencies("C1", new[] { "D1" });

        var result = graph.SetDependencies("A1", new[] { "B1", "C1" });

        Assert.That(result.Success, Is.True);
    }

    [Test]
    public void ToString_of_a_rejected_result_reads_as_a_cycle_error_message()
    {
        var graph = new DependencyGraph();
        graph.SetDependencies("A1", new[] { "B1" });

        var result = graph.SetDependencies("B1", new[] { "A1" });

        Assert.That(result.ToString(), Is.EqualTo("#CYCLE! (B1 -> A1 -> B1)"));
    }
}
