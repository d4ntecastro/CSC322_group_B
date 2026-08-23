using System.Collections.Generic;
using CalcEngine.Evaluator;
using CalcEngine.Evaluator.Values;
using NUnit.Framework;
using CalcEngine.Evaluator.Commands;

namespace CalcEngine.Evaluator.Tests.CommandsTests
{
    [TestFixture]
    public class CommandManagerTests
    {
        /// <summary>A minimal in-memory ICellMutator so these tests don't need the real grid/graph.</summary>
        private sealed class FakeCellMutator : ICellMutator
        {
            private readonly Dictionary<CellAddress, string> _inputs = new();

            public string GetCellInput(CellAddress address) => _inputs.TryGetValue(address, out var v) ? v : string.Empty;

            public void SetCellInput(CellAddress address, string input) => _inputs[address] = input;
        }

        [Test]
        public void Do_ExecutesCommand_AndEnablesUndo()
        {
            var mutator = new FakeCellMutator();
            var manager = new CommandManager();
            var address = new CellAddress(0, 0);

            manager.Do(new SetCellCommand(mutator, address, "=SUM(A1:A2)"));

            Assert.That(mutator.GetCellInput(address), Is.EqualTo("=SUM(A1:A2)"));
            Assert.That(manager.CanUndo, Is.True);
            Assert.That(manager.CanRedo, Is.False);
        }

        [Test]
        public void Undo_RestoresPreviousInput()
        {
            var mutator = new FakeCellMutator();
            var manager = new CommandManager();
            var address = new CellAddress(0, 0);
            mutator.SetCellInput(address, "10");

            manager.Do(new SetCellCommand(mutator, address, "20"));
            manager.Undo();

            Assert.That(mutator.GetCellInput(address), Is.EqualTo("10"));
            Assert.That(manager.CanRedo, Is.True);
            Assert.That(manager.CanUndo, Is.False);
        }

        [Test]
        public void Redo_ReappliesUndoneCommand()
        {
            var mutator = new FakeCellMutator();
            var manager = new CommandManager();
            var address = new CellAddress(0, 0);

            manager.Do(new SetCellCommand(mutator, address, "99"));
            manager.Undo();
            manager.Redo();

            Assert.That(mutator.GetCellInput(address), Is.EqualTo("99"));
        }

        [Test]
        public void NewAction_AfterUndo_ClearsRedoHistory()
        {
            var mutator = new FakeCellMutator();
            var manager = new CommandManager();
            var address = new CellAddress(0, 0);

            manager.Do(new SetCellCommand(mutator, address, "1"));
            manager.Undo();
            manager.Do(new SetCellCommand(mutator, address, "2"));

            Assert.That(manager.CanRedo, Is.False);
        }

        [Test]
        public void ClearCellCommand_Undo_RestoresOriginalInput()
        {
            var mutator = new FakeCellMutator();
            var manager = new CommandManager();
            var address = new CellAddress(1, 1);
            mutator.SetCellInput(address, "hello");

            manager.Do(new ClearCellCommand(mutator, address));
            Assert.That(mutator.GetCellInput(address), Is.EqualTo(string.Empty));

            manager.Undo();
            Assert.That(mutator.GetCellInput(address), Is.EqualTo("hello"));
        }

        [Test]
        public void Undo_WithNothingToUndo_DoesNothing_AndDoesNotThrow()
        {
            var manager = new CommandManager();

            Assert.DoesNotThrow(() => manager.Undo());
            Assert.That(manager.CanUndo, Is.False);
        }

        [Test]
        public void Supports_AtLeastOneHundredConsecutiveUndos()
        {
            var mutator = new FakeCellMutator();
            var manager = new CommandManager();
            var address = new CellAddress(0, 0);

            for (var i = 1; i <= 120; i++)
                manager.Do(new SetCellCommand(mutator, address, i.ToString()));

            var undoCount = 0;
            while (manager.CanUndo && undoCount < 100)
            {
                manager.Undo();
                undoCount++;
            }

            Assert.That(undoCount, Is.EqualTo(100));
        }
    }
}
