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
}