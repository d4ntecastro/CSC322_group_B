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
using System.Data;
using CalcEngine.Grammar;
using CalcEngine.Grammar.Errors;
using CalcEngine.Grammar.Tree;
using Microsoft.Win32;
using System.IO;
using CalcEngine.Graph;
using CalcEngine.Evaluator;


namespace CalcEngine.Gui;


/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, ICellChangeObserver
{
    private DataTable _spreadsheetTable = new DataTable();
    private FormulaParserService _parserService = new FormulaParserService();

    private GuiCellValueSource _cellSource = new GuiCellValueSource();

    private CalcEngine.Evaluator.Context.IEvaluationContext _evalContext;

    private GrammarToEvaluatorAdapter _adapter;

    private readonly DependencyGraph _graph = new();

    private readonly Dictionary<string, string> _formulas = new();

    // active cell information
    private int _selectedRow = 0;
    private int _selectedColumn = 0;

    public MainWindow()
    {
        InitializeComponent();
        InitializeSpreadsheet();
        _graph.Subscribe(this);

        _evalContext = new CalcEngine.Evaluator.Context.EvaluationContext(
            _cellSource,
            new CalcEngine.Evaluator.Evaluation.TreeWalkingEvaluator(),
            CalcEngine.Evaluator.Functions.FunctionRegistry.CreateDefault()
        );

        _adapter = new GrammarToEvaluatorAdapter(_evalContext);
    }

    private void InitializeSpreadsheet()
    {
        for (char c = 'A'; c <= 'Z'; c++)
        {
            _spreadsheetTable.Columns.Add(c.ToString(), typeof(string));
        }

        for (int i = 1; i <= 50; i++)
        {
            _spreadsheetTable.Rows.Add(_spreadsheetTable.NewRow());
        }

        SpreadsheetGrid.ItemsSource = _spreadsheetTable.DefaultView;
    }

    private void SpreadsheetGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
    {
        if (e.AddedCells.Count > 0)
        {
            var cellInfo = e.AddedCells[0];


            if (cellInfo.IsValid && cellInfo.Column != null)
            {
                // get row and column index
                int columnIndex = SpreadsheetGrid.CurrentCell.Column.DisplayIndex;

                int rowIndex = SpreadsheetGrid.Items.IndexOf(cellInfo.Item);


                // ensure the indexes are not negative
                if (columnIndex >= 0 && rowIndex >= 0)
                {
                    // update active cell information
                    _selectedColumn = columnIndex;

                    _selectedRow = rowIndex;

                    // get cell address
                    char columnLetter = (char)('A' + columnIndex);

                    string cellAddress = $"{columnLetter}{rowIndex + 1}";

                    CellAddressTextBox.Text = cellAddress;

                    // show corresponding formula for the selected cell.
                    if (_formulas.TryGetValue(cellAddress, out string formula))
                    {
                        FormulaTextBox.Text = formula;
                    }
                    else if (rowIndex < _spreadsheetTable.Rows.Count && columnIndex < _spreadsheetTable.Columns.Count)
                    {
                        FormulaTextBox.Text = _spreadsheetTable.Rows[rowIndex][columnIndex]?.ToString() ?? "";
                    }
                }
            }
        }
    }

    private void ProcessCellInput(string cellAddress, string inputText, int rowIndex, int columnIndex)
    {
        _formulas[cellAddress] = inputText;

        ParseResult result = _parserService.Parse(inputText);

        if (result.Success && result.Tree != null)
        {


            var rawDependencies = result.Tree.GetCellReferences();

            var expandedDependencies = new List<string>();

            foreach (var dep in rawDependencies)
            {
                if (dep.Contains(":"))
                {
                    var (start, end) = GrammarToEvaluatorAdapter.ParseRange(dep);

                    for (int row = start.Row; row <= end.Row; row++)
                    {
                        for (int column = start.Column; column <= end.Column; column++)
                        {
                            expandedDependencies.Add($"{(char)('A' + column)}{row + 1}");
                        }
                    }
                }
                else
                {
                    expandedDependencies.Add(dep);
                }
            }

            var graphResult = _graph.SetDependencies(cellAddress, expandedDependencies);

            if (!graphResult.Success)
            {
                StatusTextBlock.Text = $"Cycle Error: {graphResult}";

                StatusTextBlock.Foreground = Brushes.Red;

                _spreadsheetTable.Rows[_selectedRow][_selectedColumn] = "#CYCLE!";

                SpreadsheetGrid.Items.Refresh();

                return;
            }
            // update status block view to green and the text to the result tree
            StatusTextBlock.Text = $"Valid Formula! Tree: {result.Tree}";

            StatusTextBlock.Foreground = Brushes.Green;

            _graph.PropagateChange(cellAddress);

        }
        else
        {
            _graph.SetDependencies(cellAddress, Array.Empty<string>());

            _spreadsheetTable.Rows[rowIndex][columnIndex] = inputText;

            // SpreadsheetGrid.Items.Refresh();

            var address = GrammarToEvaluatorAdapter.ParseCellReference(cellAddress);

            if (double.TryParse(inputText, out double num))
            {
                _cellSource.Set(address, CalcEngine.Evaluator.Values.CellValue.Number(num));
            }
            else
            {
                _cellSource.Set(address, CalcEngine.Evaluator.Values.CellValue.Text(inputText));

            }


            _graph.PropagateChange(cellAddress);

            StatusTextBlock.Text = $"Raw Value Entered";

            StatusTextBlock.Foreground = Brushes.Black;
        }
    }

    private void FormulaTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        // when user clicks Enter key
        if (e.Key == Key.Enter)
        {
            if (_selectedRow < 0 || _selectedColumn < 0) return;

            string formulaText = FormulaTextBox.Text;

            string cellAddress = $"{(char)('A' + _selectedColumn)}{_selectedRow + 1}";

            ProcessCellInput(cellAddress, formulaText, _selectedRow, _selectedColumn);
        }
    }

    private void GraphButton_Click(object sender, RoutedEventArgs e)
    {
        var order = _graph.GetRecalculationOrder();

        string orderMessage = string.Join(" -> ", order);

        MessageBox.Show($"Full Recalculation Order:\n\n{orderMessage}", "Dependency Graph Snapshot", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void SpreadsheetGrid_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        e.Row.Header = (e.Row.GetIndex() + 1).ToString();
    }

    private void ClearGridValues()
    {
        // Loop through all rows and columns in the table and clear them
        foreach (DataRow row in _spreadsheetTable.Rows)
        {
            for (int i = 0; i < _spreadsheetTable.Columns.Count; i++)
            {
                row[i] = string.Empty;
            }
        }

        // Instantly refresh the visual spreadsheet grid
        SpreadsheetGrid.Items.Refresh();
    }

    private void NewButton_Click(object sender, RoutedEventArgs e)
    {
        ClearGridValues();

        StatusTextBlock.Text = "New Spreadsheet Created";

        StatusTextBlock.Foreground = Brushes.Black;
    }


    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        ClearGridValues();
        StatusTextBlock.Text = "Spreadsheet cleared.";

        StatusTextBlock.Foreground = Brushes.Black;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveFileDialog dialog = new SaveFileDialog
        {
            Filter = "CSV Files (*.csv)|All Files (*.*)|*.*",
            DefaultExt = "csv"
        };

        if (dialog.ShowDialog() == true)
        {
            using (StreamWriter writer = new StreamWriter(dialog.FileName))
            {
                var headerNames = _spreadsheetTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName);

                writer.WriteLine(string.Join(",", headerNames));

                foreach (DataRow row in _spreadsheetTable.Rows)
                {
                    var fields = row.ItemArray.Select(field => field?.ToString() ?? "");

                    writer.WriteLine(string.Join(",", fields));
                }
            }

            StatusTextBlock.Text = $"File saved successfully to {dialog.FileName}";

            StatusTextBlock.Foreground = Brushes.Green;
        }
    }

    private void OpenButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new OpenFileDialog
        {
            Filter = "CSV Files (*.csv)|All Files (*.*)|*.*",
        };

        if (dialog.ShowDialog() == true)
        {
            ClearGridValues();

            string[] lines = File.ReadAllLines(dialog.FileName);

            int startLine = (lines.Length > 0 && lines[0].StartsWith("A,")) ? 1 : 0;

            for (int readingline = 0; readingline < lines.Length; readingline++)
            {
                string[] values = lines[readingline].Split(',');

                for (int character = 0; character < 26; character++)
                {
                    _spreadsheetTable.Rows[readingline - startLine][character] = values[character];
                }

                SpreadsheetGrid.Items.Refresh();


            }

            StatusTextBlock.Text = $"File loaded from {dialog.FileName}";
        }
    }

    private string GetCellValueFromGrid(string cellAddress)
    {
        if (string.IsNullOrEmpty(cellAddress)) return "";
        char columnLetter = cellAddress[0];

        if (!int.TryParse(cellAddress.Substring(1), out int rowNumber)) return "";

        int columnIndex = columnLetter - 'A';
        int rowIndex = rowNumber - 1;

        if (rowIndex >= 0 && rowIndex < _spreadsheetTable.Rows.Count && columnIndex >= 0 && columnIndex < _spreadsheetTable.Columns.Count)
        {
            return _spreadsheetTable.Rows[rowIndex][columnIndex]?.ToString() ?? "";
        }
        return "";
    }

    public void OnCellInvalidated(string cellReference)
    {
        if (!_formulas.TryGetValue(cellReference, out string formulaText)) return;

        ParseResult result = _parserService.Parse(formulaText);

        if (result.Success && result.Tree != null)
        {

            var evaluatedValue = result.Tree.Evaluate(_adapter);

            int columnIndex = cellReference[0] - 'A';

            int rowIndex = int.Parse(cellReference.Substring(1)) - 1;

            if (rowIndex >= 0 && rowIndex < _spreadsheetTable.Rows.Count && columnIndex >= 0 && columnIndex < _spreadsheetTable.Columns.Count)
            {
                _spreadsheetTable.Rows[rowIndex][columnIndex] = evaluatedValue.ToString();
                // SpreadsheetGrid.Items.Refresh();

                var address = GrammarToEvaluatorAdapter.ParseCellReference(cellReference);

                if (evaluatedValue.Type == CalcEngine.Grammar.Values.CellValueType.Number)
                {
                    _cellSource.Set(address, CalcEngine.Evaluator.Values.CellValue.Number(evaluatedValue.NumberValue));
                }
                else
                {
                    _cellSource.Set(address, CalcEngine.Evaluator.Values.CellValue.Text(evaluatedValue.ToString()));
                }
            }
        }
    }

    private void SpreadsheetGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit)
        {
            if (e.EditingElement is TextBox textBox)
            {
                string newText = textBox.Text;

                int columnIndex = e.Column.DisplayIndex;

                int rowIndex = e.Row.GetIndex();

                string cellAddress = $"{(char)('A' + columnIndex)}{rowIndex + 1}";

                // e.Cancel = true;

                // SpreadsheetGrid.CancelEdit();

                Dispatcher.InvokeAsync(() => { ProcessCellInput(cellAddress, newText, rowIndex, columnIndex); });
            }
        }
    }
}