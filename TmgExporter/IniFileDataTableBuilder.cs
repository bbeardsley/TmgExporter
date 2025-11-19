using System.Data;
using Microsoft.Extensions.Configuration;
using NLog;

namespace TmgExporter
{
    public class IniFileDataTableBuilder : IDataTableBuilder
    {
        private readonly string _iniFile;
        private readonly ILogger _logger;

        public IniFileDataTableBuilder(string iniFile)
            : this(iniFile, LogManager.GetLogger("IniFileDataTableBuilder"))
        {
        }

        public IniFileDataTableBuilder(string iniFile, ILogger logger)
        {
            _iniFile = iniFile;
            _logger = logger;
        }

        public DataTable BuildDataTable(TableInfo table)
        {
            var dataTable = new DataTable(table.OutputTableName);
            dataTable.Columns.Add(new DataColumn("category", typeof(string)) { MaxLength = 255 });
            dataTable.Columns.Add(new DataColumn("setting", typeof(string)) { MaxLength = 255 });
            dataTable.Columns.Add(new DataColumn("value", typeof(string)) { MaxLength = 255 });

            _logger.Info("Getting the {0} data...", table.OutputTableName);

            var configuration = new ConfigurationBuilder()
                .AddIniFile(_iniFile)
                .Build();

            foreach (var section in configuration.GetChildren())
            {
                var sectionName = section.Key;
                foreach (var setting in section.GetChildren())
                {
                    dataTable.Rows.Add(new object[] { sectionName, setting.Key, setting.Value });
                }
            }

            _logger.Info("Finished getting the {0} data.", table.OutputTableName);

            return dataTable;

        }
    }
}