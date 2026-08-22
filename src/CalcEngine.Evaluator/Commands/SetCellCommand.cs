using System;

namespace CalcEngine.Evaluator.Commands
{
    /// <summary>
    /// The command for "the user typed something new into a cell." It captures the
    /// cell's previous input the moment it executes (not when constructed -- the
    /// cell could change between construction and execution), so Undo() can restore
    /// exactly what was there before, whatever that was.
    /// </summary>
    public sealed class SetCellCommand : ICommand
    {
        private readonly ICellMutator _mutator;
        private readonly Values.CellAddress _address;
        private readonly string _newInput;
        private string? _previousInput;

        public string Description => $"Set {_address} = \"{_newInput}\"";

        public SetCellCommand(ICellMutator mutator, Values.CellAddress address, string newInput)
        {
            _mutator = mutator ?? throw new ArgumentNullException(nameof(mutator));
            _address = address;
            _newInput = newInput ?? throw new ArgumentNullException(nameof(newInput));
        }

        public void Execute()
        {
            _previousInput = _mutator.GetCellInput(_address);
            _mutator.SetCellInput(_address, _newInput);
        }

        public void Undo()
        {
            if (_previousInput is null)
                throw new InvalidOperationException("Cannot undo a SetCellCommand that has not been executed yet.");

            _mutator.SetCellInput(_address, _previousInput);
        }
    }
}
