using System.Data;
using System.IO;
using CsvHelper;
using NLog;

namespace TmgExporter
{
    public class CsvOutput : IOutput
    {
        private readonly ILogger _logger;

        public CsvOutput()
            : this(LogManager.GetLogger("CsvOutput"))
        {
        }

        public CsvOutput(ILogger logger)
        {
            _logger = logger;
        }

        public void Output(TableInfo table, DataTable data)
        {
            var outputFilename = string.Format("{0}.csv", data.TableName);

            _logger.Info("Writing data to {0}...", outputFilename);

            using (var stringWriter = new StringWriter())
            using (var csv = new CsvWriter(stringWriter))
            {
                foreach (DataColumn column in data.Columns)
                {
                    csv.WriteField(column.ColumnName);
                }
                csv.NextRecord();
                foreach (var row in data.AsEnumerable())
                {
                    foreach (var item in row.ItemArray)
                    {
                        csv.WriteField(item.ToString());
                    }
                    csv.NextRecord();
                }

                File.WriteAllText(outputFilename, stringWriter.ToString());
            }

            _logger.Info("Done writing data to {0}.", outputFilename);
        }
    }
}
