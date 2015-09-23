using System;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using NLog;

namespace TmgExporter
{
    public class DatabaseDataTableBuilder : IDataTableBuilder
    {
        private readonly ILogger _logger;
        private readonly OleDbConnection _conn;
        private bool _trimSpaces;

        public DatabaseDataTableBuilder(OleDbConnection conn)
            : this(conn, LogManager.GetLogger("DatabaseDataTableBuilder"))
        {
        }

        public DatabaseDataTableBuilder(OleDbConnection conn, ILogger logger)
        {
            _conn = conn;
            _logger = logger;
            _trimSpaces = true;
        }

        public DatabaseDataTableBuilder DontTrimSpaces()
        {
            _trimSpaces = false;
            return this;
        }

        public DataTable BuildDataTable(TableInfo table)
        {
            _logger.Info("Getting the {0} data...", table.OutputTableName);

            var dataTable = new DataTable(table.OutputTableName);
            using (var selectCommand = _conn.CreateCommand())
            {
                selectCommand.CommandText = BuildTableSelectSql(table);
                using (var dataAdapter = new OleDbDataAdapter(selectCommand))
                {
                    dataAdapter.Fill(dataTable);
                }
            }

            if (_trimSpaces)
                TrimSpaces(dataTable);

            _logger.Info("Finished getting the {0} data.", table.OutputTableName);

            return dataTable;
        }

        private static void TrimSpaces(DataTable table)
        {
            foreach (var row in table.AsEnumerable())
            {
                foreach (DataColumn column in table.Columns)
                {
                    if (column.DataType != typeof (string))
                        continue;

                    var val = row[column].ToString();
                    row[column] = val.Trim();
                }
            }
        }


        private static string BuildTableSelectSql(TableInfo table)
        {
            var sql = string.Format("SELECT {0} FROM {1};",
                string.Join(",", table.Columns.OrderBy(c => c.OrdinalPosition).Select(c => FormatColumn(table, c))),
                table.InputTableName);
            return sql;
        }

        private static string FormatColumn(TableInfo table, ColumnInfo column)
        {
            OleDbType dbType;
            if (table.IsDbTypeOverridden(column.Name, out dbType))
            {
                switch (dbType)
                {
                    case OleDbType.Binary:
                        return string.Format("CAST({0} as Blob) as {0}", column.Name);
                    default:
                        throw new InvalidOperationException(string.Format("Unsupported dbType {0} for column {1}", dbType, column.Name));
                }
            }
            return column.Name;
        }

    }
}