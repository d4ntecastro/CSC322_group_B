using System;

namespace CalcEngine.Evaluator.Values
{
    /// <summary>
    /// A zero-based (row, column) coordinate identifying a single cell in the grid.
    /// This is the shared coordinate type the Evaluator, Graph, and GUI modules all
    /// pass between each other, so it deliberately has no dependency on any other module.
    /// Row 0 / Column 0 corresponds to spreadsheet cell "A1".
    /// </summary>
    public readonly struct CellAddress : IEquatable<CellAddress>
    {
        public int Row { get; }
        public int Column { get; }

        public CellAddress(int row, int column)
        {
            if (row < 0) throw new ArgumentOutOfRangeException(nameof(row), "Row cannot be negative.");
            if (column < 0) throw new ArgumentOutOfRangeException(nameof(column), "Column cannot be negative.");
            Row = row;
            Column = column;
        }

        public bool Equals(CellAddress other) => Row == other.Row && Column == other.Column;

        public override bool Equals(object? obj) => obj is CellAddress other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Row, Column);

        /// <summary>
        /// Renders the address the way a user would type it, e.g. (0,0) -> "A1", (0,27) -> "AB1".
        /// Handy for error messages and undo/redo command descriptions.
        /// </summary>
        public override string ToString() => $"{ColumnToLetters(Column)}{Row + 1}";

        private static string ColumnToLetters(int column)
        {
            var letters = string.Empty;
            var n = column;
            do
            {
                letters = (char)('A' + (n % 26)) + letters;
                n = n / 26 - 1;
            } while (n >= 0);
            return letters;
        }

        public static bool operator ==(CellAddress left, CellAddress right) => left.Equals(right);
        public static bool operator !=(CellAddress left, CellAddress right) => !left.Equals(right);
    }
}
