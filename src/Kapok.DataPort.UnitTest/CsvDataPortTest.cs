using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kapok.DataPort.Csv;
using Xunit;

namespace Kapok.DataPort.UnitTest
{
    public class SimpleLedgerAccountEntity
    {
        public string? LedgerAccountNum { get; set; }
        public string? Description { get; set; }
        public string? CurrencyCode { get; set; }
    }

    public class CsvDataPortTest
    {
        [Fact]
        public void CsvToEntity()
        {
            var targetTableCollection = new List<SimpleLedgerAccountEntity>();

            var tableDataPort = new TableDataPort<CsvDataPortSource, EntityDataPortTarget<SimpleLedgerAccountEntity>>(
                source: new CsvDataPortSource
                {
                    HasHeader = true,
                    ColumnSeparator = CsvHelper.LineSeparator.Comma,
                    StreamReader = new StreamReader(File.OpenRead("./CsvData_Accounts.csv"))
                },
                target: new DataPortEntityCollectionTarget<SimpleLedgerAccountEntity>
                {
                    TargetCollection = targetTableCollection
                });

            tableDataPort.ColumnMappings = new List<TableDataPortMap>
            {
                new TableDataPortMap
                {
                    SourceColumn = new DataPortColumn
                    {
                        Name = "Account No.",
                        Type = typeof(string)
                    },
                    TargetColumn = tableDataPort.Target.Schema.First(c => c.Name == nameof(SimpleLedgerAccountEntity.LedgerAccountNum))
                },
                new TableDataPortMap
                {
                    SourceColumn = new DataPortColumn
                    {
                        Name = "Description",
                        Type = typeof(string)
                    },
                    TargetColumn = tableDataPort.Target.Schema.First(c => c.Name == nameof(SimpleLedgerAccountEntity.Description))
                },
                new TableDataPortMap
                {
                    SourceColumn = new DataPortColumn
                    {
                        Name = "Currency",
                        Type = typeof(string)
                    },
                    TargetColumn = tableDataPort.Target.Schema.First(c => c.Name == nameof(SimpleLedgerAccountEntity.CurrencyCode))
                }
            };

            tableDataPort.Execute();

            Assert.Equal(8, targetTableCollection.Count);

            Assert.Equal("1110", targetTableCollection[0].LedgerAccountNum);
            Assert.Equal("Cash In Bank and On Hand", targetTableCollection[0].Description);
            Assert.Equal("USD", targetTableCollection[0].CurrencyCode);
            
            Assert.Equal("1310", targetTableCollection[1].LedgerAccountNum);
            Assert.Equal("Trade Accounts Receivable", targetTableCollection[1].Description);
            Assert.Null(targetTableCollection[1].CurrencyCode);

            Assert.Equal("1510", targetTableCollection[2].LedgerAccountNum);
            Assert.Equal("Prepaid Expense", targetTableCollection[2].Description);
            Assert.Null(targetTableCollection[2].CurrencyCode);
            
            Assert.Equal("1630", targetTableCollection[3].LedgerAccountNum);
            Assert.Equal("Machinery and Equipment", targetTableCollection[3].Description);
            Assert.Null(targetTableCollection[3].CurrencyCode);

            Assert.Equal("2110", targetTableCollection[4].LedgerAccountNum);
            Assert.Equal("Trade Accounts Payable", targetTableCollection[4].Description);
            Assert.Null(targetTableCollection[4].CurrencyCode);

            Assert.Equal("3110", targetTableCollection[5].LedgerAccountNum);
            Assert.Equal("Common Equity, Stock", targetTableCollection[5].Description);
            Assert.Null(targetTableCollection[5].CurrencyCode);

            Assert.Equal("4120", targetTableCollection[6].LedgerAccountNum);
            Assert.Equal("Revenue, Sales (over time)", targetTableCollection[6].Description);
            Assert.Null(targetTableCollection[6].CurrencyCode);

            Assert.Equal("5112", targetTableCollection[7].LedgerAccountNum);
            Assert.Equal("Cost Of Services Rendered", targetTableCollection[7].Description);
            Assert.Null(targetTableCollection[7].CurrencyCode);
        }
    }
}
