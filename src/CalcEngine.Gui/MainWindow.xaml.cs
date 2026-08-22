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

namespace CalcEngine.Gui;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private DataTable _spreadsheetTable = new DataTable();
    private FormulaParserService _parserService = new FormulaParserService();
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
        if (SpreadsheetGrid.CurrentCell.IsValid)
        {
            // get row and column index
            int columnIndex = SpreadsheetGrid.CurrentCell.Column.DisplayIndex;

            int rowIndex = SpreadsheetGrid.Items.IndexOf(SpreadsheetGrid.CurrentCell.Item);


            // ensure the indexes are not negative
            if (columnIndex >= 0 && rowIndex >= 0)
            {
                char columnLetter = (char)('A' + columnIndex);

                string cellAddress = $"{columnLetter}{rowIndex + 1}";

                CellAddressTextBox.Text = cellAddress;
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

            if(result.Success)
            {
                StatusTextBlock.Text = $"Valid Formula! Tree: {result.Tree}";

                StatusTextBlock.Foreground = Brushes.Green;
            }
            else
            {
                string errors = string.Join(';', result.Errors);

                StatusTextBlock.Text = $"Syntax Error: {errors}";

                StatusTextBlock.Foreground = Brushes.Red;
            }
        }
    }
}