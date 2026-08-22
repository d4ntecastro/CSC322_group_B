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


namespace CalcEngine.Gui;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private DataTable _spreadsheetTable = new DataTable();
    private FormulaParserService _parserService = new FormulaParserService();

    // active cell information
    private int _selectedRow = 0;
    private int _selectedColumn = 0;

    public MainWindow()
    {
        InitializeComponent();
        InitializeSpreadsheet();
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
                    if(rowIndex<_spreadsheetTable.Rows.Count && columnIndex < _spreadsheetTable.Columns.Count)
                    {
                        FormulaTextBox.Text = _spreadsheetTable.Rows[rowIndex][columnIndex]?.ToString() ?? "";
                    }
                }
            }
        }
    }

    private void FormulaTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        // when user clicks Enter key
        if (e.Key == Key.Enter)
        {
            string formulaText = FormulaTextBox.Text;

            // parse the formulaText using CalcEngine.Grammar
            ParseResult result = _parserService.Parse(formulaText);

            if (result.Success && result.Tree != null)
            {
                // update status block view to green and the text to the result tree
                StatusTextBlock.Text = $"Valid Formula! Tree: {result.Tree}";

                StatusTextBlock.Foreground = Brushes.Green;

                // calculation
                var fakeContext = new FakeEvaluationContext();

                var evaluatedValue = result.Tree.Evaluate(fakeContext);

                System.Console.WriteLine(evaluatedValue.ToString());

                if (_selectedRow >= 0 && _selectedColumn >= 0)
                {
                    _spreadsheetTable.Rows[_selectedRow][_selectedColumn] = evaluatedValue.ToString();
                    SpreadsheetGrid.Items.Refresh();
                }

            }
            else
            {
                string errors = string.Join(';', result.Errors);

                StatusTextBlock.Text = $"Syntax Error: {errors}";

                StatusTextBlock.Foreground = Brushes.Red;
            }
        }
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




}