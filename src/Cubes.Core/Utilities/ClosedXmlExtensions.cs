using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using ClosedXML.Excel;
using Cubes.Core.DataAccess;

namespace Cubes.Core.Utilities
{
    public static class ClosedXmlExtensions
    {
        public static DataTable ToDataTable(this IXLWorksheet ws, bool hasHeaderRow = true)
        {
            int cellIndex = 0;
            var dataTable = new DataTable();
            foreach (var cell in ws.Row(1).Cells(true))
            {
                dataTable.Columns.Add(hasHeaderRow ? cell.Value.ToString() : $"Column {++cellIndex}");
            }

            int rowIndex = 0;
            foreach(var row in ws.RowsUsed())
            {
                if (rowIndex++ == 0 && hasHeaderRow) continue;
                DataRow dataRow = dataTable.NewRow();
                cellIndex = 0;
                foreach (var cell in row.CellsUsed())
                {
                    dataRow[cellIndex++] = cell.Value;
                }
                dataTable.Rows.Add(dataRow);
                rowIndex++;
            }

            return dataTable;
        }

        public static byte[] ToExcelWorkbook<T>(this IEnumerable<T> list, ExcelFormattingSettings formattingSettings = null)
        {
            if (formattingSettings == null)
                formattingSettings = ExcelFormattingSettings.Default<T>();

            // Create Excel workbook
            using var wb = new XLWorkbook();
            AddPage(wb, formattingSettings.SheetName, list, formattingSettings);

            // Return as byte array
            return wb.GetAsByteArray();
        }

        public static byte[] ToExcelWorkbook(this IEnumerable<QueryResult> queryResults, ExcelFormattingSettings formattingSettings = null)
        {
            if (formattingSettings == null)
                formattingSettings = ExcelFormattingSettings.Default("Sheet1");

            // Create Excel package
            using var wb = new XLWorkbook();
            foreach (var result in queryResults)
                AddQueryResultPage(wb, result, formattingSettings);

            // Return as byte array
            return wb.GetAsByteArray();
        }

        private static void AddPage<T>(XLWorkbook workbook, string pageName, IEnumerable<T> data, ExcelFormattingSettings formattingSettings)
        {
            var type = typeof(T);
            var props = type.GetProperties();

            //Create a sheet
            var ws = workbook.Worksheets.Add(pageName);

            // Default style
            ws.Style.Font.FontSize = formattingSettings.DefaultFont.Size;
            ws.Style.Font.FontName = formattingSettings.DefaultFont.Family;
            ws.Style.Font.Bold     = formattingSettings.DefaultFont.IsBold;
            ws.Style.Font.Italic   = formattingSettings.DefaultFont.IsItalic;

            int iColumn = 1;
            int iRow = 1;
            // Add headers
            if (formattingSettings.HeaderSettings.AddHeaders)
            {
                //Header font settings
                ws.Row(iRow).Style.Font.FontName = formattingSettings.HeaderSettings.Font.Family;
                ws.Row(iRow).Style.Font.FontSize = formattingSettings.HeaderSettings.Font.Size;
                ws.Row(iRow).Style.Font.Bold     = formattingSettings.HeaderSettings.Font.IsBold;
                ws.Row(iRow).Style.Font.Italic   = formattingSettings.HeaderSettings.Font.IsItalic;

                foreach (var prop in props)
                {
                    var columnSettings = prop.GetCustomAttribute<ColumnSettingsAttribute>();
                    ws.Cell(iRow, iColumn++).Value = columnSettings == null ? prop.Name : columnSettings.Header;
                }

                iRow++;
            }

            // Add data
            foreach (var obj in data)
            {
                iColumn = 1;
                for (int i = 0; i < props.Length; i++)
                {
                    ws.Cell(iRow, iColumn).Value = props[i].GetValue(obj);
                    if (props[i].PropertyType == typeof(DateTime))
                        ws.Cell(iRow, iColumn).Style.DateFormat.Format = formattingSettings.DateFormat;
                    iColumn++;
                }
                iRow++;
            }

            // Column widths
            if (formattingSettings.AutoFit)
                ws.Columns().AdjustToContents();

            // Freeze headers
            if (formattingSettings.HeaderSettings.AddHeaders)
            {
                ws.RangeUsed().SetAutoFilter(true);
                ws.SheetView.Freeze(1, 0);
            }
        }

        private static void AddQueryResultPage(XLWorkbook wb, QueryResult result, ExcelFormattingSettings formattingSettings)
        {
            //Create a sheet
            var ws = wb.Worksheets.Add(result.Name);

            // Columns
            int length = result.Columns.Count();
            if (length == 0) return;

            // Default style
            ws.Style.Font.FontSize = formattingSettings.DefaultFont.Size;
            ws.Style.Font.FontName = formattingSettings.DefaultFont.Family;
            ws.Style.Font.Bold     = formattingSettings.DefaultFont.IsBold;
            ws.Style.Font.Italic   = formattingSettings.DefaultFont.IsItalic;

            int iColumn = 1;
            int iRow = 1;
            var columns = result.Columns.ToList();

            // Add headers
            if (formattingSettings.HeaderSettings.AddHeaders)
            {
                //Header font settings
                ws.Row(iRow).Style.Font.FontName = formattingSettings.HeaderSettings.Font.Family;
                ws.Row(iRow).Style.Font.FontSize = formattingSettings.HeaderSettings.Font.Size;
                ws.Row(iRow).Style.Font.Bold     = formattingSettings.HeaderSettings.Font.IsBold;
                ws.Row(iRow).Style.Font.Italic   = formattingSettings.HeaderSettings.Font.IsItalic;

                foreach (var column in result.Columns)
                {
                    var md = result.Metadata.Columns.FirstOrDefault(c => c.Name == column.Name);
                    ws.Cell(iRow, iColumn++).Value = md?.Label ?? column.Name;
                }

                iRow++;
            }

            // Add data
            foreach (IDictionary<string, object> obj in result.Data)
            {
                iColumn = 1;
                for (int i = 0; i < length; i++)
                {
                    ws.Cell(iRow, iColumn).Value = obj[columns[i].Name];
                    if (columns[i].ColumnType?.Equals(typeof(DateTime)) == true)
                        ws.Cell(iRow, iColumn).Style.DateFormat.Format = formattingSettings.DateFormat;
                    iColumn++;
                }
                iRow++;
            }

            // Check for totals
            if (result.Metadata.Columns.Any(c => c.HasTotals))
            {
                iRow++;
                ws.Cell(iRow, 1).Value =  result.Metadata.TotalsLabel;

                var totals = result.Metadata.Columns.Where(c => c.HasTotals);
                foreach (var total in totals)
                {
                    var columnIndex = columns
                        .FindIndex(c => c.Name.Equals(total.Name, StringComparison.OrdinalIgnoreCase)) + 1;
                    var columnName  = (char)('A' - 1 + columnIndex);
                    var startingRow = formattingSettings.HeaderSettings.AddHeaders ? 2 : 1;
                    ws.Cell(iRow, columnIndex).FormulaA1 = $"=SUM({columnName}{startingRow}:{columnName}{iRow -2})";
                }

                ws.Row(iRow).Style.Font.Bold = true;
            }

            // Check for fixed rows
            if (result.Metadata.FixedColumns > 0)
                ws.SheetView.FreezeColumns(result.Metadata.FixedColumns);

            // Column widths
            if (formattingSettings.AutoFit)
                ws.Columns().AdjustToContents();

            // Freeze headers
            if (formattingSettings.HeaderSettings.AddHeaders)
            {
                ws.RangeUsed().SetAutoFilter(true);
                ws.SheetView.FreezeRows(1);
            }
        }

        public static byte[] GetAsByteArray(this XLWorkbook workbook)
        {
            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);

            return memoryStream.ToArray();
        }
    }
}

// Most probably not needed, but left here for future reference

//public static byte[] ToExcelWorkbook<T>(this Dictionary<string, IEnumerable<T>> lists, ExcelFormattingSettings formattingSettings = null)
//{
//    if (formattingSettings == null)
//        formattingSettings = ExcelFormattingSettings.Default<T>();

//    // Create Excel
//    using var wb = new XLWorkbook();

//    // Create sheets
//    foreach (var list in lists)
//        AddPage(wb, list.Key, list.Value, formattingSettings);

//    // Return as byte array
//    return wb.GetAsByteArray();
//}
