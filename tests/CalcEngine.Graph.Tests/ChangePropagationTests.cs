using CalcEngine.Graph;

namespace CalcEngine.Graph.Tests;

public class ChangePropagationTests
{
    private sealed class RecordingObserver : ICellChangeObserver
    {
        public List<string> Invalidated { get; } = new();

        public void OnCellInvalidated(string cellReference) => Invalidated.Add(cellReference);
    }

    [Test]
    public void PropagateChange_notifies_subscribed_observers_for_every_affected_cell()
    {
        var graph = new DependencyGraph();
        graph.SetDependencies("B1", new[] { "A1" });
        graph.SetDependencies("C1", new[] { "B1" });
        var observer = new RecordingObserver();
        graph.Subscribe(observer);

        graph.PropagateChange("A1");

        Assert.That(observer.Invalidated, Is.EqualTo(new[] { "A1", "B1", "C1" }));
    }

    [Test]
    public void PropagateChange_notifies_in_dependency_order_even_for_multiple_observers()
    {
        var graph = new DependencyGraph();
        graph.SetDependencies("B1", new[] { "A1" });
        graph.SetDependencies("C1", new[] { "B1" });
        var first = new RecordingObserver();
        var second = new RecordingObserver();
        graph.Subscribe(first);
        graph.Subscribe(second);

        graph.PropagateChange("A1");

        Assert.That(first.Invalidated, Is.EqualTo(new[] { "A1", "B1", "C1" }));
        Assert.That(second.Invalidated, Is.EqualTo(new[] { "A1", "B1", "C1" }));
    }

    [Test]
    public void PropagateChange_does_not_notify_cells_outside_the_affected_subgraph()
    {
        var graph = new DependencyGraph();
        graph.SetDependencies("B1", new[] { "A1" });
        graph.SetDependencies("D1", new[] { "C1" }); // unrelated chain
        var observer = new RecordingObserver();
        graph.Subscribe(observer);

        graph.PropagateChange("A1");

        Assert.That(observer.Invalidated, Does.Not.Contain("C1"));
        Assert.That(observer.Invalidated, Does.Not.Contain("D1"));
    }

    [Test]
    public void Unsubscribe_stops_further_notifications()
    {
        var graph = new DependencyGraph();
        graph.SetDependencies("B1", new[] { "A1" });
        var observer = new RecordingObserver();
        graph.Subscribe(observer);
        graph.Unsubscribe(observer);

        graph.PropagateChange("A1");

        Assert.That(observer.Invalidated, Is.Empty);
    }

    [Test]
    public void PropagateChanges_merges_multiple_simultaneous_edits_into_one_ordered_pass()
    {
        var graph = new DependencyGraph();
        graph.SetDependencies("B1", new[] { "A1" });
        graph.SetDependencies("D1", new[] { "C1" });
        var observer = new RecordingObserver();
        graph.Subscribe(observer);

        graph.PropagateChanges(new[] { "A1", "C1" });

        Assert.That(observer.Invalidated, Is.EquivalentTo(new[] { "A1", "B1", "C1", "D1" }));
    }

    [Test]
    public void PropagateChange_returns_the_same_order_it_notified_observers_with()
    {
        var graph = new DependencyGraph();
        graph.SetDependencies("B1", new[] { "A1" });

        var order = graph.PropagateChange("A1");

        Assert.That(order, Is.EqualTo(new[] { "A1", "B1" }));
    }
}
