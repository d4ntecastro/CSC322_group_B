using System.Globalization;

namespace CalcEngine.Grammar.Values;

/// <summary>
/// CellValue is the single result type that flows through the whole engine:
/// every expression node's Evaluate() call returns one of these, and every
/// function argument the evaluator sees is one of these.
///
/// ABSTRACT DATA TYPE
/// -------------------
/// Abstraction function:
///     AF(rep) = a tagged union representing exactly one of:
///               a number, a text string, a boolean, "empty", or an error.
///               Type is given by rep.Type; only the field matching that
///               type is meaningful.
///
/// Representation invariant:
///     - Type == Number   ⇒ NumberValue is set, TextValue/ErrorMessage unused.
///     - Type == Text     ⇒ TextValue is set (never null), others unused.
///     - Type == Boolean  ⇒ BooleanValue is set, others unused.
///     - Type == Empty    ⇒ no payload field is meaningful.
///     - Type == Error    ⇒ ErrorMessage is set (never null/empty), others unused.
///     - Exactly one of the above holds at any time — this is enforced by
///       making the constructor private and only exposing it through the
///       static factory methods below, so no caller can construct an
///       inconsistent CellValue (e.g. Type == Number but NumberValue unset).
///
/// This is deliberately a struct-like immutable value: once created, a
/// CellValue never changes, so it can be safely shared/cached across the
/// dependency graph without defensive copying.
/// </summary>
public sealed class CellValue
{
    public CellValueType Type { get; }
    public double NumberValue { get; }
    public string TextValue { get; }
    public bool BooleanValue { get; }
    public string ErrorMessage { get; }

    // Private constructor: the only way to build a CellValue is through the
    // factory methods below, which guarantees the representation invariant
    // above always holds.
    private CellValue(CellValueType type, double number = 0, string text = "", bool boolean = false, string error = "")
    {
        Type = type;
        NumberValue = number;
        TextValue = text;
        BooleanValue = boolean;
        ErrorMessage = error;
    }

    public static CellValue Number(double value) => new(CellValueType.Number, number: value);

    public static CellValue Text(string value) => new(CellValueType.Text, text: value ?? string.Empty);

    public static CellValue Boolean(bool value) => new(CellValueType.Boolean, boolean: value);

    public static readonly CellValue Empty = new(CellValueType.Empty);

    /// <summary>
    /// Builds an error result. Use the standard spreadsheet-style codes so the
    /// GUI (Joel's module) can recognise and flag them consistently:
    /// "#DIV/0!", "#VALUE!", "#REF!", "#CYCLE!", "#NAME?".
    /// </summary>
    public static CellValue Error(string message) => new(CellValueType.Error, error: message);

    public bool IsError => Type == CellValueType.Error;

    /// <summary>
    /// Renders the value the way a grid cell would display it. Errors and
    /// empty cells never throw here — this method is what makes it safe for
    /// the GUI to call ToString() on literally any CellValue without checking
    /// its type first.
    /// </summary>
    public override string ToString() => Type switch
    {
        CellValueType.Number => NumberValue.ToString(CultureInfo.InvariantCulture),
        CellValueType.Text => TextValue,
        CellValueType.Boolean => BooleanValue ? "TRUE" : "FALSE",
        CellValueType.Empty => string.Empty,
        CellValueType.Error => ErrorMessage,
        _ => string.Empty
    };
}
