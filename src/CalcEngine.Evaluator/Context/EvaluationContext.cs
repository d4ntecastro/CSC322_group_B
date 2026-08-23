using System;
using System.Collections.Generic;
using CalcEngine.Evaluator.Ast;
using CalcEngine.Evaluator.Evaluation;
using CalcEngine.Evaluator.Functions;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.Context
{
    /// <summary>
    /// The concrete IEvaluationContext used everywhere outside of tests. It wires
    /// together the three collaborators evaluation needs: where cell values come
    /// from (ICellValueSource), how a node gets evaluated (IEvaluator), and which
    /// functions are available (FunctionRegistry).
    /// </summary>
    public sealed class EvaluationContext : IEvaluationContext
    {
        private readonly ICellValueSource _cellSource;
        private readonly IEvaluator _evaluator;

        public FunctionRegistry Functions { get; }

        public EvaluationContext(ICellValueSource cellSource, IEvaluator evaluator, FunctionRegistry functions)
        {
            _cellSource = cellSource ?? throw new ArgumentNullException(nameof(cellSource));
            _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
            Functions = functions ?? throw new ArgumentNullException(nameof(functions));
        }

        public CellValue GetCellValue(CellAddress address) => _cellSource.GetValue(address);

        public IEnumerable<CellValue> GetRangeValues(CellAddress start, CellAddress end)
        {
            var rowStart = Math.Min(start.Row, end.Row);
            var rowEnd = Math.Max(start.Row, end.Row);
            var colStart = Math.Min(start.Column, end.Column);
            var colEnd = Math.Max(start.Column, end.Column);

            for (var row = rowStart; row <= rowEnd; row++)
            {
                for (var col = colStart; col <= colEnd; col++)
                {
                    yield return GetCellValue(new CellAddress(row, col));
                }
            }
        }

        public CellValue Evaluate(IExpressionNode node) => _evaluator.Evaluate(node, this);
    }
}
