using System;
using System.Collections.ObjectModel;
using System.Linq;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;
using CalcEngine.Grammar;

namespace CalcEngine.Gui.ViewModels
{
    public sealed class SpreadsheetViewModel
    {
        public ObservableCollection<CellViewModel> Cells { get; } = new();

        private readonly GuiCellValueSource _cellSource;
        private readonly IEvaluationContext _evalContext;
        private readonly GrammarToEvaluatorAdapter _adapter;
        private readonly FormulaParserService _parser = new();

        public int Rows { get; }
        public int Columns { get; }

        public SpreadsheetViewModel(int rows = 10, int columns = 10)
        {
            Rows = rows; Columns = columns;
            _cellSource = new GuiCellValueSource();
            _evalContext = new CalcEngine.Evaluator.Context.EvaluationContext(_cellSource, new CalcEngine.Evaluator.Evaluation.TreeWalkingEvaluator(), CalcEngine.Evaluator.Functions.FunctionRegistry.CreateDefault());
            _adapter = new GrammarToEvaluatorAdapter(_evalContext);

            for (var r = 0; r < rows; r++)
                for (var c = 0; c < columns; c++)
                    Cells.Add(new CellViewModel(new CellAddress(r, c)));
        }

        public CellViewModel? GetCell(string addressText)
        {
            try
            {
                var addr = GrammarToEvaluatorAdapter.ParseCellReference(addressText);
                return Cells.FirstOrDefault(c => c.Address.Row == addr.Row && c.Address.Column == addr.Column);
            }
            catch { return null; }
        }

        public void SetFormula(CellAddress address, string formula)
        {
            var cell = Cells.First(c => c.Address.Row == address.Row && c.Address.Column == address.Column);
            cell.Formula = formula;
            RecalculateAll();
        }

        public void RecalculateAll()
        {
            // Naive: evaluate each cell in row-major order; formulas may reference others
            foreach (var cell in Cells)
            {
                if (string.IsNullOrWhiteSpace(cell.Formula))
                {
                    _cellSource.Set(cell.Address, CalcEngine.Evaluator.Values.CellValue.Empty);
                    cell.Display = string.Empty;
                    continue;
                }

                var parseResult = _parser.Parse(cell.Formula);
                if (!parseResult.Success)
                {
                    cell.Display = "#PARSE";
                    _cellSource.Set(cell.Address, CalcEngine.Evaluator.Values.CellValue.Error(CalcEngine.Evaluator.Values.CellErrorType.InvalidValue));
                    continue;
                }

                var gVal = parseResult.Tree.Evaluate(_adapter);
                var evVal = gVal.Type switch
                {
                    CalcEngine.Grammar.Values.CellValueType.Number => CalcEngine.Evaluator.Values.CellValue.Number(gVal.NumberValue),
                    CalcEngine.Grammar.Values.CellValueType.Text => CalcEngine.Evaluator.Values.CellValue.Text(gVal.TextValue),
                    CalcEngine.Grammar.Values.CellValueType.Boolean => CalcEngine.Evaluator.Values.CellValue.Boolean(gVal.BooleanValue),
                    CalcEngine.Grammar.Values.CellValueType.Empty => CalcEngine.Evaluator.Values.CellValue.Empty,
                    _ => CalcEngine.Evaluator.Values.CellValue.Error(CalcEngine.Evaluator.Values.CellErrorType.InvalidValue)
                };

                _cellSource.Set(cell.Address, evVal);
                cell.Display = evVal.ToString();
            }
        }
    }
}
