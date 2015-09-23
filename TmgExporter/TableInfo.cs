using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;

namespace TmgExporter
{
    [DebuggerDisplay("Key={Key},OutputTableName={OutputTableName},InputTableName={InputTableName}")]
    public class TableInfo
    {
        private readonly List<string> _ignoredColumns;
        private readonly List<ColumnInfo> _columns;
        private readonly List<IndexInfo> _indexes;
        private readonly List<List<string>> _uniqueColumns;
        private readonly Dictionary<string, OleDbType> _overriddenDbTypes;
        private readonly Dictionary<string, long> _overrideMaxLengths;

        public string Key { get; private set; }
        public string InputTableName { get; set; }
        public string OutputTableName { get; private set; }
        public string PrimaryKey { get; private set; }

        private IDataTableBuilder _dataTableBuilder;

        public TableInfo(string key, string name)
        {
            _dataTableBuilder = null;
            _uniqueColumns = new List<List<string>>();
            _indexes = new List<IndexInfo>();
            _columns = new List<ColumnInfo>();
            Key = key;
            OutputTableName = name;
            _ignoredColumns = new List<string> {"tt", "ispicked"};
            _overriddenDbTypes = new Dictionary<string, OleDbType>();
            _overrideMaxLengths = new Dictionary<string, long>();
        }

        public bool IsIgnoredColumn(string columnName)
        {
            return _ignoredColumns.Contains(columnName);
        }

        public bool IsDbTypeOverridden(string columnName, out OleDbType dbType)
        {
            return _overriddenDbTypes.TryGetValue(columnName, out dbType);
        }

        public TableInfo AddColumn(ColumnInfo columnInfo)
        {
            _columns.Add(columnInfo);
            return this;
        }

        public IEnumerable<ColumnInfo> Columns
        {
            get { return _columns; }
        }

        public TableInfo AddIndex(IndexInfo indexInfo)
        {
            _indexes.Add(indexInfo);
            return this;
        }

        public IEnumerable<IndexInfo>  Indexes
        {
            get { return _indexes; }
        }

        public TableInfo Tap(Action<TableInfo> action)
        {
            action(this);
            return this;
        }

        public TableInfo SetDataTableBuilder(IDataTableBuilder dataTableBuilder)
        {
            _dataTableBuilder = dataTableBuilder;
            return this;
        }

        public DataTable GetData()
        {
            return _dataTableBuilder.BuildDataTable(this);
        }

        public TableInfo SetPrimaryKey(string columnName)
        {
            PrimaryKey = columnName;
            return this;
        }

        public TableInfo IgnoreColumn(string column)
        {
            _ignoredColumns.Add(column);
            return this;
        }

        public TableInfo IgnoreColumns(IEnumerable<string> columns)
        {
            if (columns != null)
                _ignoredColumns.AddRange(columns);

            return this;
        }

        public TableInfo SetUniqueColumns(IEnumerable<string> columns)
        {
            _uniqueColumns.Add(columns.ToList());
            return this;
        }

        public bool IsUniqueColumns(IEnumerable<string> columns)
        {
            var list = columns.ToList();

            foreach (var uniqueColumns in _uniqueColumns)
            {
                if (uniqueColumns.Count != list.Count)
                    continue;

                if (uniqueColumns.All(list.Contains))
                    return true;
            }
            return false;
        }

        public TableInfo OverrideDbType(OleDbType dbType, IEnumerable<string> columns)
        {
            foreach (var column in columns)
                _overriddenDbTypes[column] = dbType;

            return this;
        }

        public TableInfo OverrideMaxLength(string columnName, long maxLength)
        {
            _overrideMaxLengths[columnName] = maxLength;
            return this;
        }

        public bool IsMaxLengthOverridden(string columnName, out long maxLength)
        {
            return _overrideMaxLengths.TryGetValue(columnName, out maxLength);
        }
    }
}
