using System.Data;
using System.IO;
using NLog;

namespace TmgExporter
{
    public class XmlOutput : IOutput
    {
        private readonly ILogger _logger;

        public XmlOutput()
            : this(LogManager.GetLogger("XmlOutput"))
        {
        }

        public XmlOutput(ILogger logger)
        {
            _logger = logger;
        }

        public void Output(TableInfo table, DataTable data)
        {
            var outputFilename = string.Format("{0}.xml", table.OutputTableName);

            _logger.Info("Writing data to {0}...", outputFilename);

            using (var writer = new StringWriter())
            {
                data.WriteXml(writer, XmlWriteMode.IgnoreSchema, false);

                File.WriteAllText(outputFilename, writer.ToString());
            }
            _logger.Info("Finished writing data to {0}.", outputFilename);

        }
    }
}
