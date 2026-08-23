using System.Globalization;

namespace CalcEngine.Evaluator.Values
{
    /// <summary>
    /// The single result type returned by every piece of evaluation in this module:
    /// every AST node, every built-in function, and the Evaluator's public entry point
    /// all return a CellValue. It behaves like a tagged union over
    /// {Empty, Number, Text, Boolean, Error} -- exactly one of the private fields is
    /// meaningful, selected by <see cref="Kind"/>.
    ///
    /// This is the core of the "error-value handling, not exceptions" requirement:
    /// a formula like =1/0 does not throw a DivideByZeroException anywhere in this
    /// module. It produces CellValue.Error(CellErrorType.DivideByZero), which then
    /// flows through parent nodes (SUM, +, etc.) exactly like any other value would.
    /// Only a genuine, unexpected bug should ever throw a real .NET exception out of
    /// this module -- see Evaluator.Evaluate for the safety net around that case.
    /// </summary>
    public readonly struct CellValue
    {
        public CellValueKind Kind { get; }

        private readonly double _number;
        private readonly string? _text;
        private readonly bool _boolean;
        private readonly CellErrorType _errorType;

        private CellValue(CellValueKind kind, double number, string? text, bool boolean, CellErrorType errorType)
        {
            Kind = kind;
            _number = number;
            _text = text;
            _boolean = boolean;
            _errorType = errorType;
        }

        public static CellValue Number(double value) => new(CellValueKind.Number, value, null, false, default);

        public static CellValue Text(string value) => new(CellValueKind.Text, 0, value, false, default);

        public static CellValue Boolean(bool value) => new(CellValueKind.Boolean, 0, null, value, default);

        public static CellValue Error(CellErrorType errorType) => new(CellValueKind.Error, 0, null, false, errorType);

        public static readonly CellValue Empty = new(CellValueKind.Empty, 0, null, false, default);

        public bool IsError => Kind == CellValueKind.Error;

        /// <summary>Only meaningful when Kind == Number.</summary>
        public double NumberValue => _number;

        /// <summary>Only meaningful when Kind == Text.</summary>
        public string TextValue => _text ?? string.Empty;

        /// <summary>Only meaningful when Kind == Boolean.</summary>
        public bool BooleanValue => _boolean;

        /// <summary>Only meaningful when Kind == Error.</summary>
        public CellErrorType ErrorType => _errorType;

        /// <summary>
        /// Attempts to view this value as a number, applying the same loose coercion
        /// rules a spreadsheet applies to a single, explicitly-supplied argument
        /// (e.g. ROUND("3.5", 0) should work). Returns false -- never throws -- if the
        /// value cannot reasonably be treated as a number.
        /// Deliberately NOT used for range/aggregate functions (SUM, AVERAGE, MIN, MAX,
        /// COUNT), which instead only look at cells that are already CellValueKind.Number
        /// -- see the comment in Functions/AggregateFunctionBase.cs for why.
        /// </summary>
        public bool TryCoerceToNumber(out double value)
        {
            switch (Kind)
            {
                case CellValueKind.Number:
                    value = _number;
                    return true;
                case CellValueKind.Boolean:
                    value = _boolean ? 1 : 0;
                    return true;
                case CellValueKind.Empty:
                    value = 0;
                    return true;
                case CellValueKind.Text:
                    return double.TryParse(_text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
                default:
                    value = 0;
                    return false;
            }
        }

        /// <summary>
        /// Attempts to view this value as a boolean. Numbers coerce as "nonzero is true"
        /// (matching C-family/spreadsheet convention); text only coerces if it is
        /// literally "TRUE" or "FALSE" (case-insensitive) -- an arbitrary string like
        /// "yes" is not a boolean and correctly fails here.
        /// </summary>
        public bool TryCoerceToBoolean(out bool value)
        {
            switch (Kind)
            {
                case CellValueKind.Boolean:
                    value = _boolean;
                    return true;
                case CellValueKind.Number:
                    value = _number != 0;
                    return true;
                case CellValueKind.Empty:
                    value = false;
                    return true;
                case CellValueKind.Text:
                    if (string.Equals(_text, "TRUE", System.StringComparison.OrdinalIgnoreCase)) { value = true; return true; }
                    if (string.Equals(_text, "FALSE", System.StringComparison.OrdinalIgnoreCase)) { value = false; return true; }
                    value = false;
                    return false;
                default:
                    value = false;
                    return false;
            }
        }

        /// <summary>The text a grid cell should display for this value.</summary>
        public override string ToString() => Kind switch
        {
            CellValueKind.Number => _number.ToString(CultureInfo.InvariantCulture),
            CellValueKind.Text => _text ?? string.Empty,
            CellValueKind.Boolean => _boolean ? "TRUE" : "FALSE",
            CellValueKind.Error => _errorType.ToDisplayString(),
            CellValueKind.Empty => string.Empty,
            _ => string.Empty
        };
    }
}
