using Kapok.DataPort.Csv;
using Kapok.DataPort.Entity;
using Xunit;

namespace Kapok.DataPort.UnitTest;

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

        var tableDataPort = new TableDataPort<CsvDataPortSource, EntityCollectionDataPortTarget<SimpleLedgerAccountEntity>>(
            source: new CsvDataPortSource
            {
                HasHeader = true,
                ColumnSeparator = CsvHelper.LineSeparator.Comma,
                StreamReader = new StreamReader(File.OpenRead("./CsvData_Accounts.csv"))
            },
            target: new EntityCollectionDataPortTarget<SimpleLedgerAccountEntity>
            {
                TargetCollection = targetTableCollection
            });

        tableDataPort.ColumnMappings = new List<TableDataPortMap>
        {
            new()
            {
                SourceColumn = new DataPortColumn
                {
                    Name = "Account No.",
                    Type = typeof(string),
                    Required = false
                },
                TargetColumn = tableDataPort.Target.Schema.First(c => Equals(c.Name, nameof(SimpleLedgerAccountEntity.LedgerAccountNum)))
            },
            new()
            {
                SourceColumn = new DataPortColumn
                {
                    Name = "Description",
                    Type = typeof(string),
                    Required = false
                },
                TargetColumn = tableDataPort.Target.Schema.First(c => Equals(c.Name, nameof(SimpleLedgerAccountEntity.Description)))
            },
            new()
            {
                SourceColumn = new DataPortColumn
                {
                    Name = "Currency",
                    Type = typeof(string),
                    Required = false
                },
                TargetColumn = tableDataPort.Target.Schema.First(c => Equals(c.Name, nameof(SimpleLedgerAccountEntity.CurrencyCode)))
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

    [Fact]
    public void EntityToCsv()
    {
        const string TargetFileName = "./CsvData_Accounts_new.csv";

        var entities = new List<SimpleLedgerAccountEntity>
        {
            new() { LedgerAccountNum = "1110", Description = "Cash In Bank and On Hand", CurrencyCode = "USD" },
            new() { LedgerAccountNum = "1310", Description = "Trade Accounts Receivable", CurrencyCode = null },
            new() { LedgerAccountNum = "1510", Description = "Prepaid Expense", CurrencyCode = null },
            new() { LedgerAccountNum = "1630", Description = "Machinery and Equipment", CurrencyCode = null },
            new() { LedgerAccountNum = "2110", Description = "Trade Accounts Payable", CurrencyCode = null },
            new() { LedgerAccountNum = "3110", Description = "Common Equity, Stock", CurrencyCode = null },
            new() { LedgerAccountNum = "4120", Description = "Revenue, Sales (over time)", CurrencyCode = null },
            new() { LedgerAccountNum = "5112", Description = "Cost Of Services Rendered", CurrencyCode = null },
        };

        StreamWriter? streamWriter = null;
        try
        {
            streamWriter = new StreamWriter(File.OpenWrite(TargetFileName));

            var tableDataPort =
                new TableDataPort<EntityEnumeratorDataPortSource<SimpleLedgerAccountEntity>, CsvDataPortTarget>(
                    source: new EntityEnumeratorDataPortSource<SimpleLedgerAccountEntity>
                    {
                        SourceEnumerable = entities
                    },
                    target: new CsvDataPortTarget
                    {
                        WriteWithHeader = true,
                        ColumnSeparator = CsvHelper.LineSeparator.Comma,
                        StreamWriter = streamWriter,
                        Schema = new[]
                        {
                            new DataPortColumn
                            {
                                Name = "Account No.",
                                Type = typeof(string),
                                Required = false
                            },
                            new DataPortColumn
                            {
                                Name = "Description",
                                Type = typeof(string),
                                Required = false
                            },
                            new DataPortColumn
                            {
                                Name = "Currency",
                                Type = typeof(string),
                                Required = false
                            },
                        }
                    });

            var sourceSchema = tableDataPort.Source.ReadSchema();

            tableDataPort.ColumnMappings = new List<TableDataPortMap>
            {
                new()
                {
                    SourceColumn =
                        sourceSchema.First(c => Equals(c.Name, nameof(SimpleLedgerAccountEntity.LedgerAccountNum))),
                    TargetColumn = tableDataPort.Target.Schema.First(c => Equals(c.Name, "Account No."))

                },
                new()
                {
                    SourceColumn = sourceSchema.First(c => Equals(c.Name, nameof(SimpleLedgerAccountEntity.Description))),
                    TargetColumn = tableDataPort.Target.Schema.First(c => Equals(c.Name, "Description"))
                },
                new()
                {
                    SourceColumn = sourceSchema.First(c => Equals(c.Name, nameof(SimpleLedgerAccountEntity.CurrencyCode))),
                    TargetColumn = tableDataPort.Target.Schema.First(c => Equals(c.Name,"Currency"))
                }
            };

            try
            {
                tableDataPort.Execute();

                streamWriter.Dispose();
                streamWriter = null;
                
                Assert.True(File.Exists(TargetFileName));
            }
            finally
            {
                // clean up the test
                if (File.Exists(TargetFileName))
                    File.Delete(TargetFileName);
            }
        }
        finally
        {
            streamWriter?.Dispose();
        }
    }
}