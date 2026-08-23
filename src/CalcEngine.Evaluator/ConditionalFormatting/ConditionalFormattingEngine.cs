using System.Collections.Generic;
using System.Linq;
using CalcEngine.Evaluator.Context;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.ConditionalFormatting
{
    /// <summary>
    /// Holds the set of active conditional formatting rules and answers the GUI's
    /// only real question: "what style, if any, should this cell be drawn with right
    /// now?" When multiple rules match the same cell, the one with the lowest
    /// Priority value wins (ties keep whichever was found first).
    /// </summary>
    public sealed class ConditionalFormattingEngine
    {
        private readonly List<ConditionalFormatRule> _rules = new();

        public IReadOnlyList<ConditionalFormatRule> Rules => _rules;

        public void AddRule(ConditionalFormatRule rule) => _rules.Add(rule);

        /// <summary>Removes every rule with the given name; returns true if at least one was removed.</summary>
        public bool RemoveRule(string name) => _rules.RemoveAll(r => r.Name == name) > 0;

        public FormatStyle? GetStyleFor(CellAddress address, CellValue value, IEvaluationContext context)
        {
            ConditionalFormatRule? best = null;

            foreach (var rule in _rules)
            {
                if (!rule.AppliesTo(address)) continue;
                if (!rule.Condition.IsSatisfiedBy(value, context)) continue;
                if (best is null || rule.Priority < best.Priority) best = rule;
            }

            return best?.Style;
        }
    }
}
