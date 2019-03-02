namespace Cubes.Core.Utilities
{
    public class ExcelFormattingSettings
    {
        public const string FONT_FAMILY = "Calibri";
        public const int FONT_SIZE = 11;

        public class Font
        {
            public string Family { get; set; }
            public int Size { get; set; }
            public bool IsBold { get; set; }
            public bool IsItalic { get; set; }
        }

        public class Header
        {
            public bool AddHeaders { get; set; }
            public Font Font { get; set; }
        }

        protected ExcelFormattingSettings() { }

        public string SheetName { get; set; }
        public Font DefaultFont { get; set; }
        public Header HeaderSettings { get; set; }
        public string DateFormat { get; set; }
        public bool AutoFit { get; set; }

        public static ExcelFormattingSettings Default(string sheetName) =>
            new ExcelFormattingSettings
            {
                SheetName = sheetName,
                DefaultFont = new Font
                {
                    Family = FONT_FAMILY,
                    Size = FONT_SIZE
                },
                HeaderSettings = new Header
                {
                    AddHeaders = true,
                    Font = new Font
                    {
                        Family = FONT_FAMILY,
                        Size = FONT_SIZE,
                        IsBold = true
                    }
                },
                DateFormat = "dd/MM/yyyy",
                AutoFit = true
            };
        public static ExcelFormattingSettings Default<T>() => Default(typeof(T).Name);

        public ExcelFormattingSettings Sanitize()
        {
            if (DefaultFont == null)
                DefaultFont = new Font
                {
                    Family = FONT_FAMILY,
                    Size = FONT_SIZE
                };
            DefaultFont.Family.IfNullOrEmpty(FONT_FAMILY);
            if (HeaderSettings.Font.Size == 0)
                DefaultFont.Size = FONT_SIZE;

            HeaderSettings.Font.Family.IfNullOrEmpty(DefaultFont.Family);
            if (HeaderSettings.Font.Size == 0)
                DefaultFont.Size = 11;

            return this;
        }
    }
}
