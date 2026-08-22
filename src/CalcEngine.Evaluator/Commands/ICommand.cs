namespace CalcEngine.Evaluator.Commands
{
    /// <summary>The Command pattern's core interface: any reversible user action (right now, just editing or clearing a cell) implements this.</summary>
    public interface ICommand
    {
        /// <summary>A short human-readable label, useful for a GUI "Undo: Set A1" style menu item.</summary>
        string Description { get; }

        void Execute();

        /// <summary>Reverses exactly what Execute() did. Must only be called after Execute() has run at least once.</summary>
        void Undo();
    }
}
