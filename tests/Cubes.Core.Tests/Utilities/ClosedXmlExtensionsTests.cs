using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Cubes.Core.DataAccess;
using Cubes.Core.Utilities;
using Xunit;

namespace Cubes.Core.Tests.Utilities
{
    public class ClosedXmlExtensionsTests
    {
        [Fact]
        public void Test_ToDataTable()
        {
            var assembly = typeof(ClosedXmlExtensionsTests).Assembly;
            var resourceName = "Cubes.Core.Tests.Utilities.Samples.Target Totals per company.xlsx";
            if (!assembly.GetManifestResourceNames().Contains(resourceName))
                throw new ArgumentException($"Resource '{resourceName}' does not exist!");

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var wb = new XLWorkbook(stream);

            var dt = wb.Worksheet(1).ToDataTable();

            Assert.NotNull(dt);
            Assert.Equal(
                "Περιγραφή,ItemCode,Quantity,Value,Papharm,Pharmex",
                $"{dt.Columns[0]},{dt.Columns[1]},{dt.Columns[2]},{dt.Columns[3]},{dt.Columns[4]},{dt.Columns[5]}");
            Assert.Equal(474, dt.Rows.Count);

            var row = dt.Rows[469];
            Assert.Equal("DR.B.WB 93600 ΜΠΙΜΠΕΡΟ ΠΛΑΣΤΙΚΟ OPTIONS+ (Φ.Λ.) 270ML (3 ΤΕΜ", row[0]);
            Assert.Equal(3358.46, row[3].SafeCast<double>());
        }

        [Fact]
        public void Test_ToExcelWorkbook()
        {
            var list = Enumerable
                .Range(1, 100)
                .Select(i => new Sample
                {
                    ID = i,
                    Label = $"Row number: {i}",
                    CreatedAt = DateTime.Now.AddDays(i).Date
                });

            var wb = list.ToExcelWorkbook();

            using var ms = new MemoryStream(wb);
            using var actualWb = new XLWorkbook(ms);

            Assert.Equal(1, actualWb.Worksheets.Count);
            Assert.Equal("Sample", actualWb.Worksheet(1).Name);
            Assert.Equal("Row number: 45", actualWb.Worksheet(1).Cell(46, 2).Value);

            // In case we need a sample file:
            // File.WriteAllBytes(@"C:\wrk_Temp\test.xlsx", wb);
        }

        [Fact]
        public void Test_ToExecleWorkbook_QueryResults()
        {
            var queryResult1 = new QueryResult
            {
                Name = "TestQueryResult",
                Data = Enumerable
                .Range(1, 100)
                .Select(i =>
                {
                    var s = new Sample
                    {
                        ID        = i,
                        Label     = $"Row number: {i}",
                        CreatedAt = DateTime.Now.AddDays(i).Date
                    };
                    return s.ToDynamic();
                })
                .ToList(),
                Columns = Sample.GetColumns()
            };

            var queryResult2 = new QueryResult
            {
                Name = "Details",
                Data = Enumerable
                .Range(1, 100)
                .Select(i =>
                {
                    var s = new
                    {
                        DetailID = i,
                        Text     = $"Detail row number: {i}",
                        Index    = null as string
                    };
                    return s.ToDynamic();
                })
                .ToList(),
                Columns = new List<QueryResult.Column>
                {
                    new QueryResult.Column{ Name = "DetailID", ColumnType = typeof(int) },
                    new QueryResult.Column{ Name = "Text", ColumnType = typeof(string) },
                    new QueryResult.Column{ Name = "Index", ColumnType = typeof(string) }
                }
            };

            var results = new List<QueryResult> { queryResult1, queryResult2 };
            var wb = results.ToExcelWorkbook();

            using var ms = new MemoryStream(wb);
            using var actualWb = new XLWorkbook(ms);

            Assert.Equal(2, actualWb.Worksheets.Count);
            Assert.Equal("TestQueryResult", actualWb.Worksheet(1).Name);
            Assert.Equal("Row number: 45", actualWb.Worksheet(1).Cell(46, 2).Value);
            Assert.Equal("Details", actualWb.Worksheet("Details").Name);
            Assert.Equal("Detail row number: 75", actualWb.Worksheet(2).Cell(76, 2).Value);

            // In case we need a sample file:
            // File.WriteAllBytes(@"C:\wrk_Temp\test.xlsx", wb);
        }

        internal class Sample
        {
            [ColumnSettings("DetailID")]
            public int ID { get; set; }
            public string Label { get; set; }
            public DateTime CreatedAt { get; set; }

            public static IEnumerable<QueryResult.Column> GetColumns()
            {
                return new List<QueryResult.Column>
                {
                    new QueryResult.Column{ Name = nameof(ID), ColumnType = typeof(int) },
                    new QueryResult.Column{ Name = nameof(Label), ColumnType = typeof(string) },
                    new QueryResult.Column{ Name = nameof(CreatedAt), ColumnType = typeof(DateTime) }
                };
            }
        }
    }
}
