using System;
using System.Data;
using System.IO;
using NLog;

namespace TmgExporter
{
    public class JsonOutput : IOutput
    {
        private readonly ILogger _logger;
        private readonly Func<DataTable, string> _jsonFunc;

        public JsonOutput(Func<DataTable, string> jsonFunc)
            : this(jsonFunc, LogManager.GetLogger("JsonOutput"))
        {
        }

        public JsonOutput(Func<DataTable, string> jsonFunc, ILogger logger)
        {
            if (jsonFunc == null)
                throw new ArgumentNullException("jsonFunc");

            _jsonFunc = jsonFunc;
            _logger = logger;
        }

        public void Output(TableInfo table, DataTable data)
        {
            var outputFilename = string.Format("{0}.json", data.TableName);

            _logger.Info("Writing data to {0}...", outputFilename);

            var json = _jsonFunc(data);
            if (!string.IsNullOrEmpty(json))
                File.WriteAllText(outputFilename, json);

            _logger.Info("Finished writing data to {0}.", outputFilename);
        }
    }
}