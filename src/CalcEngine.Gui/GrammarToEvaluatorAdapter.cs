using System;
using System.Collections.Generic;
using System.Linq;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;
using EvalAst = CalcEngine.Evaluator.Ast;
using GrammarTree = CalcEngine.Grammar.Tree;
using GrammarValues = CalcEngine.Grammar.Values;

namespace CalcEngine.Gui
{
    /// <summary>
    /// Adapts the Evaluator's context and functions so the Grammar tree can
    /// be evaluated unchanged. This implements Grammar.Tree.IEvaluationContext
    /// by delegating to an Evaluator.Context.IEvaluationContext and converting
    /// values back and forth.
    /// </summary>
    public sealed class GrammarToEvaluatorAdapter : GrammarTree.IEvaluationContext
    {
        private readonly IEvaluationContext _evalContext;

        public GrammarToEvaluatorAdapter(IEvaluationContext evalContext)
        {
            _evalContext = evalContext ?? throw new ArgumentNullException(nameof(evalContext));
        }

        public GrammarValues.CellValue GetCellValue(string cellReference)
        {
            var addr = ParseCellReference(cellReference);
            var ev = _evalContext.GetCellValue(addr);
            return ConvertEvaluatorToGrammar(ev);
        }

        public IEnumerable<GrammarValues.CellValue> GetRangeValues(string rangeReference)
        {
            (var start, var end) = ParseRange(rangeReference);
            foreach (var v in _evalContext.GetRangeValues(start, end))
                yield return ConvertEvaluatorToGrammar(v);
        }

        public GrammarValues.CellValue CallFunction(string functionName, IReadOnlyList<GrammarTree.IExpressionNode> arguments)
        {
            var function = _evalContext.Functions.Resolve(functionName);
            if (function is null)
                return GrammarValues.CellValue.Error("#NAME?");

            // Wrap grammar argument nodes as evaluator AST nodes
            var wrappedArgs = arguments.Select(arg => new GrammarArgWrapper(arg, this)).ToList<CalcEngine.Evaluator.Ast.IExpressionNode>();

            var result = function.Invoke(wrappedArgs, _evalContext);
            return ConvertEvaluatorToGrammar(result);
        }

        private static GrammarValues.CellValue ConvertEvaluatorToGrammar(CellValue ev)
        {
            if (ev.Kind == CellValueKind.Number) return GrammarValues.CellValue.Number(ev.NumberValue);
            if (ev.Kind == CellValueKind.Text) return GrammarValues.CellValue.Text(ev.TextValue);
            if (ev.Kind == CellValueKind.Boolean) return GrammarValues.CellValue.Boolean(ev.BooleanValue);
            if (ev.Kind == CellValueKind.Empty) return GrammarValues.CellValue.Empty;
            // Error
            return GrammarValues.CellValue.Error(ev.ErrorType.ToDisplayString());
        }

        public static CellAddress ParseCellReference(string text)
        {
            // Expect format Letters+Digits e.g. A1, AB12
            if (string.IsNullOrEmpty(text)) throw new FormatException("Empty cell reference");

            var i = 0;
            while (i < text.Length && char.IsLetter(text[i])) i++;
            var colLetters = text.Substring(0, i).ToUpperInvariant();
            var rowPart = text.Substring(i);

            if (colLetters.Length == 0 || rowPart.Length == 0) throw new FormatException($"Invalid cell reference '{text}'");

            int col = 0;
            foreach (var ch in colLetters)
            {
                col = col * 26 + (ch - 'A' + 1);
            }
            col -= 1; // zero-based

            if (!int.TryParse(rowPart, out var rowOneBased)) throw new FormatException($"Invalid cell reference '{text}'");
            var row = rowOneBased - 1;

            return new CellAddress(row, col);
        }

        public static (CellAddress start, CellAddress end) ParseRange(string text)
        {
            var parts = text.Split(':');
            if (parts.Length != 2) throw new FormatException($"Invalid range '{text}'");
            return (ParseCellReference(parts[0]), ParseCellReference(parts[1]));
        }

        /// <summary>
        /// Helper wrapper that presents a Grammar expression node as an
        /// evaluator AST node by asking the grammar node to evaluate against
        /// this adapter and converting the resulting value.
        /// </summary>
        private sealed class GrammarArgWrapper : EvalAst.IExpressionNode
        {
            private readonly GrammarTree.IExpressionNode _grammar;
            private readonly GrammarToEvaluatorAdapter _adapter;

            public GrammarArgWrapper(GrammarTree.IExpressionNode grammar, GrammarToEvaluatorAdapter adapter)
            {
                _grammar = grammar;
                _adapter = adapter;
            }

            public CellValue Evaluate(IEvaluationContext context)
            {
                // Evaluate grammar node against adapter, convert to evaluator.CellValue
                var g = _grammar.Evaluate(_adapter);
                // Convert Grammar.CellValue -> Evaluator.CellValue
                return ConvertGrammarToEvaluator(g);
            }

            private static CellValue ConvertGrammarToEvaluator(GrammarValues.CellValue g)
            {
                switch (g.Type)
                {
                    case CalcEngine.Grammar.Values.CellValueType.Number:
                        return CellValue.Number(g.NumberValue);
                    case CalcEngine.Grammar.Values.CellValueType.Text:
                        return CellValue.Text(g.TextValue);
                    case CalcEngine.Grammar.Values.CellValueType.Boolean:
                        return CellValue.Boolean(g.BooleanValue);
                    case CalcEngine.Grammar.Values.CellValueType.Empty:
                        return CellValue.Empty;
                    case CalcEngine.Grammar.Values.CellValueType.Error:
                        // Map textual error back to a best-effort CellErrorType -- keep as InvalidValue
                        return CellValue.Error(CellErrorType.InvalidValue);
                    default:
                        return CellValue.Error(CellErrorType.InvalidValue);
                }
            }
        }
    }
}
