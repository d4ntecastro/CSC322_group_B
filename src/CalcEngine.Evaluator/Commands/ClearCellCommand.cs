using System;

namespace CalcEngine.Evaluator.Commands
{
    /// <summary>The command for "the user pressed Delete on a cell." Kept as its own class (rather than SetCellCommand with input = "") since a GUI action like this deserves its own clear Description for the undo history.</summary>
    public sealed class ClearCellCommand : ICommand
    {
        private readonly ICellMutator _mutator;
        private readonly Values.CellAddress _address;
        private string? _previousInput;

        public string Description => $"Clear {_address}";

        public ClearCellCommand(ICellMutator mutator, Values.CellAddress address)
        {
            _mutator = mutator ?? throw new ArgumentNullException(nameof(mutator));
            _address = address;
        }

        public void Execute()
        {
            _previousInput = _mutator.GetCellInput(_address);
            _mutator.SetCellInput(_address, string.Empty);
        }

        public void Undo()
        {
            if (_previousInput is null)
                throw new InvalidOperationException("Cannot undo a ClearCellCommand that has not been executed yet.");

            _mutator.SetCellInput(_address, _previousInput);
        }
    }
}
