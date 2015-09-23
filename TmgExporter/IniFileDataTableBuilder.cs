using System.Data;
using IniParser;
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

            var iniData = new FileIniDataParser().ReadFile(_iniFile);
            foreach (var section in iniData.Sections)
            {
                foreach (var key in section.Keys)
                {
                    dataTable.Rows.Add(new object[] { section.SectionName, key.KeyName, key.Value });
                }
            }

            _logger.Info("Finished getting the {0} data.", table.OutputTableName);

            return dataTable;

        }
    }
}