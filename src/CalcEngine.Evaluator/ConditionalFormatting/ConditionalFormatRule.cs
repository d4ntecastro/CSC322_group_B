using System;
using CalcEngine.Evaluator.Values;

namespace CalcEngine.Evaluator.ConditionalFormatting
{
    /// <summary>One named formatting rule: applies Style to any cell within [RangeStart, RangeEnd] whose value satisfies Condition. Priority breaks ties when multiple rules match the same cell -- lower number wins, mirroring how Excel lets you reorder conditional formatting rules.</summary>
    public sealed class ConditionalFormatRule
    {
        public string Name { get; }
        public CellAddress RangeStart { get; }
        public CellAddress RangeEnd { get; }
        public IConditionalFormatCondition Condition { get; }
        public FormatStyle Style { get; }
        public int Priority { get; }

        public ConditionalFormatRule(
            string name,
            CellAddress rangeStart,
            CellAddress rangeEnd,
            IConditionalFormatCondition condition,
            FormatStyle style,
            int priority = 0)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            RangeStart = rangeStart;
            RangeEnd = rangeEnd;
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Style = style;
            Priority = priority;
        }

        /// <summary>Whether the given address falls within this rule's rectangular range, regardless of which corner RangeStart/RangeEnd were given as.</summary>
        public bool AppliesTo(CellAddress address)
        {
            var rowMin = Math.Min(RangeStart.Row, RangeEnd.Row);
            var rowMax = Math.Max(RangeStart.Row, RangeEnd.Row);
            var colMin = Math.Min(RangeStart.Column, RangeEnd.Column);
            var colMax = Math.Max(RangeStart.Column, RangeEnd.Column);

            return address.Row >= rowMin && address.Row <= rowMax
                && address.Column >= colMin && address.Column <= colMax;
        }
    }
}
