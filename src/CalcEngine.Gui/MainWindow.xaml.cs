using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CalcEngine.Grammar;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Gui;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly FormulaParserService _parser = new();
    private readonly GuiCellValueSource _cellSource;
    private readonly CalcEngine.Evaluator.Context.IEvaluationContext _evalContext;
    private readonly GrammarToEvaluatorAdapter _adapter;
    private readonly CalcEngine.Gui.ViewModels.SpreadsheetViewModel _sheet;

    public MainWindow()
    {
        InitializeComponent();

        _cellSource = new GuiCellValueSource();
        _evalContext = new CalcEngine.Evaluator.Context.EvaluationContext(_cellSource, new CalcEngine.Evaluator.Evaluation.TreeWalkingEvaluator(), CalcEngine.Evaluator.Functions.FunctionRegistry.CreateDefault());
        _adapter = new GrammarToEvaluatorAdapter(_evalContext);
        _sheet = new CalcEngine.Gui.ViewModels.SpreadsheetViewModel(10, 10);
        DataContext = _sheet;

        CellsGrid.SelectionChanged += CellsGrid_SelectionChanged;
    }

    private void CellsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CellsGrid.SelectedItem is CalcEngine.Gui.ViewModels.CellViewModel cell)
        {
            (this.FindName("AddressBox") as TextBox)!.Text = cell.AddressText;
            (this.FindName("FormulaBar") as TextBox)!.Text = cell.Formula;
        }
    }

    private void EvaluateButton_Click(object sender, RoutedEventArgs e)
    {
        var formula = (this.FindName("FormulaBar") as TextBox)?.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(formula))
        {
            (this.FindName("OutputText") as TextBlock)!.Text = "Enter a formula (e.g. =2+2)";
            return;
        }

        var parseResult = _parser.Parse(formula);
        if (!parseResult.Success)
        {
            (this.FindName("OutputText") as TextBlock)!.Text = "Parse error: " + string.Join("; ", parseResult.Errors);
            return;
        }

        var value = parseResult.Tree.Evaluate(_adapter);
        if (value.IsError)
            (this.FindName("OutputText") as TextBlock)!.Text = "Error: " + value.ErrorMessage;
        else
            (this.FindName("OutputText") as TextBlock)!.Text = value.Type == CalcEngine.Grammar.Values.CellValueType.Number ? value.NumberValue.ToString() : value.ToString();
    }

    private void SetCellButton_Click(object sender, RoutedEventArgs e)
    {
        var addrText = (this.FindName("AddressBox") as TextBox)?.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(addrText))
        {
            (this.FindName("OutputText") as TextBlock)!.Text = "Enter an address like A1.";
            return;
        }

        CalcEngine.Evaluator.Values.CellAddress address;
        try
        {
            address = GrammarToEvaluatorAdapter.ParseCellReference(addrText);
        }
        catch (Exception ex)
        {
            (this.FindName("OutputText") as TextBlock)!.Text = "Invalid address: " + ex.Message;
            return;
        }

        var formula = (this.FindName("FormulaBar") as TextBox)?.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(formula))
        {
            (this.FindName("OutputText") as TextBlock)!.Text = "Enter a formula to set.";
            return;
        }

        var parseResult = _parser.Parse(formula);
        if (!parseResult.Success)
        {
            (this.FindName("OutputText") as TextBlock)!.Text = "Parse error: " + string.Join("; ", parseResult.Errors);
            return;
        }

        // Evaluate grammar tree using adapter and convert to evaluator CellValue to store.
        var gValue = parseResult.Tree.Evaluate(_adapter);
        var evValue = ConvertGrammarToEvaluator(gValue);
        _cellSource.Set(address, evValue);

        (this.FindName("OutputText") as TextBlock)!.Text = $"Stored {evValue} at {address}";
    }

    private static CalcEngine.Evaluator.Values.CellValue ConvertGrammarToEvaluator(CalcEngine.Grammar.Values.CellValue g)
    {
        return g.Type switch
        {
            CalcEngine.Grammar.Values.CellValueType.Number => CalcEngine.Evaluator.Values.CellValue.Number(g.NumberValue),
            CalcEngine.Grammar.Values.CellValueType.Text => CalcEngine.Evaluator.Values.CellValue.Text(g.TextValue),
            CalcEngine.Grammar.Values.CellValueType.Boolean => CalcEngine.Evaluator.Values.CellValue.Boolean(g.BooleanValue),
            CalcEngine.Grammar.Values.CellValueType.Empty => CalcEngine.Evaluator.Values.CellValue.Empty,
            _ => CalcEngine.Evaluator.Values.CellValue.Error(CalcEngine.Evaluator.Values.CellErrorType.InvalidValue)
        };
    }

    // Minimal evaluation context for demo purposes.
    private sealed class FakeContext : CalcEngine.Grammar.Tree.IEvaluationContext
    {
        public CalcEngine.Grammar.Values.CellValue GetCellValue(string cellReference) => CalcEngine.Grammar.Values.CellValue.Number(10);

        public System.Collections.Generic.IEnumerable<CalcEngine.Grammar.Values.CellValue> GetRangeValues(string rangeReference)
        {
            yield return CalcEngine.Grammar.Values.CellValue.Number(1);
            yield return CalcEngine.Grammar.Values.CellValue.Number(2);
        }

        public CalcEngine.Grammar.Values.CellValue CallFunction(string functionName, System.Collections.Generic.IReadOnlyList<CalcEngine.Grammar.Tree.IExpressionNode> arguments) =>
            CalcEngine.Grammar.Values.CellValue.Number(0);
    }
}
