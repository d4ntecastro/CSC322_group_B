using System.Collections.Generic;
using System.Linq;

namespace CalcEngine.Evaluator.Commands
{
    /// <summary>
    /// The Command pattern's Invoker: runs commands and keeps the undo/redo history.
    /// Two stacks, the standard textbook approach:
    ///  - undo stack: commands that have been done and can be undone, most recent on top.
    ///  - redo stack: commands that were just undone and can be redone, most recent on top.
    /// Doing any NEW command clears the redo stack -- once you branch off in a new
    /// direction, the old "future" (what redo would have replayed) no longer applies,
    /// exactly like undo/redo works in any text editor.
    ///
    /// Capacity defaults to 200, comfortably above the "100+ operations" requirement
    /// from the project brief, and is configurable via the constructor.
    /// </summary>
    public sealed class CommandManager
    {
        private readonly Stack<ICommand> _undoStack = new();
        private readonly Stack<ICommand> _redoStack = new();
        private readonly int _capacity;

        public CommandManager(int capacity = 200)
        {
            _capacity = capacity;
        }

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        /// <summary>Executes a new command and records it for undo. Any pending redo history is discarded.</summary>
        public void Do(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            TrimToCapacity();
            _redoStack.Clear();
        }

        public void Undo()
        {
            if (!CanUndo) return;

            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
        }

        public void Redo()
        {
            if (!CanRedo) return;

            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
        }

        /// <summary>
        /// Drops the single oldest undo entry once the stack exceeds capacity. Stack&lt;T&gt;
        /// doesn't support removing from the bottom directly, so this rebuilds it from
        /// an array snapshot -- acceptable since it only runs on the rare turn where
        /// capacity is actually exceeded, not on every Do().
        /// </summary>
        private void TrimToCapacity()
        {
            if (_undoStack.Count <= _capacity) return;

            var newestFirst = _undoStack.ToArray();
            _undoStack.Clear();
            for (var i = newestFirst.Length - 2; i >= 0; i--)
                _undoStack.Push(newestFirst[i]);
        }
    }
}
