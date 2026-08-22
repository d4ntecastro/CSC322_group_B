namespace CalcEngine.Evaluator.ConditionalFormatting
{
    /// <summary>The visual style applied to a cell when a conditional formatting rule matches. Kept intentionally small (colors + bold) -- the GUI module is free to render these however it wants; this module just decides WHICH cells get WHICH style.</summary>
    public readonly struct FormatStyle
    {
        public string BackgroundColorHex { get; }
        public string FontColorHex { get; }
        public bool Bold { get; }

        public FormatStyle(string backgroundColorHex = "#FFFFFF", string fontColorHex = "#000000", bool bold = false)
        {
            BackgroundColorHex = backgroundColorHex;
            FontColorHex = fontColorHex;
            Bold = bold;
        }
    }
}
