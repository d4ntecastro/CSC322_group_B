using CalcEngine.Graph;

namespace CalcEngine.Graph.Tests;

public class DependencyGraphTests
{
    [Test]
    public void SetDependencies_creates_the_cell_and_every_dependency_it_names()
    {
        var graph = new DependencyGraph();

        graph.SetDependencies("A3", new[] { "A1", "A2" });

        Assert.That(graph.ContainsCell("A3"), Is.True);
        Assert.That(graph.ContainsCell("A1"), Is.True);
        Assert.That(graph.ContainsCell("A2"), Is.True);
        Assert.That(graph.GetDependencies("A3"), Is.EquivalentTo(new[] { "A1", "A2" }));
    }

    [Test]
    public void GetDependents_is_the_reverse_of_GetDependencies()
    {
        var graph = new DependencyGraph();
        graph.SetDependencies("A3", new[] { "A1", "A2" });

        Assert.That(graph.GetDependents("A1"), Is.EquivalentTo(new[] { "A3" }));
        Assert.That(graph.GetDependents("A2"), Is.EquivalentTo(new[] { "A3" }));
    }

    [Test]
    public void Cell_references_are_normalised_so_casing_does_not_create_duplicate_nodes()
    {
        var graph = new DependencyGraph();

        graph.SetDependencies("a3", new[] { "b1" });

        Assert.That(graph.ContainsCell("A3"), Is.True);
        Assert.That(graph.GetDependencies("A3"), Is.EquivalentTo(new[] { "B1" }));
        Assert.That(graph.CellCount, Is.EqualTo(2));
    }

    [Test]
    public void SetDependencies_replaces_the_previous_edge_set_rather_than_adding_to_it()
    {
        var graph = new DependencyGraph();
        graph.SetDependencies("A3", new[] { "A1" });

        graph.SetDependencies("A3", new[] { "A2" });

        Assert.That(graph.GetDependencies("A3"), Is.EquivalentTo(new[] { "A2" }));
        Assert.That(graph.GetDependents("A1"), Is.Empty);
    }

    [Test]
    public void Unknown_cells_return_empty_collections_instead_of_throwing()
    {
        var graph = new DependencyGraph();

        Assert.That(graph.GetDependencies("Z99"), Is.Empty);
        Assert.That(graph.GetDependents("Z99"), Is.Empty);
        Assert.That(graph.ContainsCell("Z99"), Is.False);
    }

    [Test]
    public void RemoveCell_severs_edges_in_both_directions()
    {
        var graph = new DependencyGraph();
        graph.SetDependencies("A3", new[] { "A1" });

        var removed = graph.RemoveCell("A3");

        Assert.That(removed, Is.True);
        Assert.That(graph.ContainsCell("A3"), Is.False);
        Assert.That(graph.GetDependents("A1"), Is.Empty);
    }

    [Test]
    public void RemoveCell_returns_false_for_a_cell_the_graph_never_tracked()
    {
        var graph = new DependencyGraph();

        Assert.That(graph.RemoveCell("Z99"), Is.False);
    }
}
