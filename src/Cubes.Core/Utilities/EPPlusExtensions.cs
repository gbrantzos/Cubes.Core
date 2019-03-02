using System;
using System.Collections.Generic;
using OfficeOpenXml;

namespace Cubes.Core.Utilities
{
    public static class EPPlusExtensions
    {
        public static byte[] ToExcelPackage<T>(this IEnumerable<T> list, ExcelFormattingSettings formattingSettings = null)
        {
            if (formattingSettings == null)
                formattingSettings = ExcelFormattingSettings.Default<T>();

            // Create Excel package
            using (var p = new ExcelPackage())
            {
                AddPage(p, formattingSettings.SheetName, list, formattingSettings);

                // Return as byte array
                return p.GetAsByteArray();
            }
        }

        public static byte[] ToExcelPackage<T>(this Dictionary<string,IEnumerable<T>> lists, ExcelFormattingSettings formattingSettings = null)
        {
            if (formattingSettings == null)
                formattingSettings = ExcelFormattingSettings.Default<T>();

            // Create Excel package
            using (var p = new ExcelPackage())
            {
                // Create a sheet
                var ws = p.Workbook.Worksheets.Add(formattingSettings.SheetName);
                foreach (var list in lists)
                    AddPage(p, list.Key, list.Value, formattingSettings);

                // Return as byte array
                return p.GetAsByteArray();
            }

        }

        private static void AddPage<T>(ExcelPackage package, string pageName, IEnumerable<T> data, ExcelFormattingSettings formattingSettings)
        {
            var type = typeof(T);
            var props = type.GetProperties();

            //Create a sheet
            var ws = package.Workbook.Worksheets.Add(pageName);

            // Default style
            ws.Cells.Style.Font.Size = formattingSettings.DefaultFont.Size;
            ws.Cells.Style.Font.Name = formattingSettings.DefaultFont.Family;
            ws.Cells.Style.Font.Bold = formattingSettings.DefaultFont.IsBold;
            ws.Cells.Style.Font.Italic = formattingSettings.DefaultFont.IsItalic;
            int iColumn = 0;


            // Add headers
            if (formattingSettings.HeaderSettings.AddHeaders)
            {
                //Header font settings
                ws.Cells[1, 1, 1, props.Length].Style.Font.Name = formattingSettings.HeaderSettings.Font.Family;
                ws.Cells[1, 1, 1, props.Length].Style.Font.Size = formattingSettings.HeaderSettings.Font.Size;
                ws.Cells[1, 1, 1, props.Length].Style.Font.Bold = formattingSettings.HeaderSettings.Font.IsBold;
                ws.Cells[1, 1, 1, props.Length].Style.Font.Italic = formattingSettings.HeaderSettings.Font.IsItalic;

                foreach (var prop in props)
                    ws.Cells[1, ++iColumn].Value = prop.Name;
            }

            // Add data
            int iRow = 2;
            foreach (var obj in data)
            {
                iColumn = 0;
                for (int i = 0; i < props.Length; i++)
                {
                    ws.Cells[iRow, ++iColumn].Value = props[i].GetValue(obj);
                    if (props[i].PropertyType.Equals(typeof(DateTime)))
                        ws.Cells[iRow, iColumn].Style.Numberformat.Format = formattingSettings.DateFormat;
                }
                iRow++;
            }

            // Fix widths
            if (formattingSettings.AutoFit)
                for (int i = 0; i < props.Length; i++)
                    ws.Column(i + 1).AutoFit();

            // Freeze headers
            ws.View.FreezePanes(2, 1);

        }
    }
}
