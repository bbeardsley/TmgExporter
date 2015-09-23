using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using NLog;

namespace TmgExporter
{
    /// <summary>
    /// Microsoft OLE DB Provider for Visual FoxPro 9.0  required
    /// https://www.microsoft.com/en-us/download/details.aspx?id=14839
    /// </summary>
    public class TmgProject
    {
        private const string ProjectSettingsName = "ProjectSettings";
        private readonly string _projectFile;
        private readonly ILogger _logger;
        private readonly IDataTableBuilder _iniFileDataTableBuilder;

        public TmgProject(string projectFile)
            : this(projectFile, new IniFileDataTableBuilder(projectFile), LogManager.GetLogger("TmgProject"))
        {
        }

        public TmgProject(string projectFile, IDataTableBuilder iniFileDataTableBuilder, ILogger logger)
        {
            _logger = logger;
            _iniFileDataTableBuilder = iniFileDataTableBuilder;

            ValidateProjectFile(projectFile);

            _projectFile = projectFile;
        }

        public string ProjectFile
        {
            get { return _projectFile; }
        }

        private IEnumerable<TableInfo> GetDefaultTableInfos()
        {
            var tableInfo = new List<TableInfo>
                {
                    new TableInfo("$", "People")
                        .SetPrimaryKey("per_no")
                        .IgnoreColumn("scbuff")
                        .SetUniqueColumns(new[] { "dsid", "ref_id"})
                        .SetUniqueColumns(new[] { "dsid", "per_no"}),
                    new TableInfo("a", "SourceTypes"),
                    new TableInfo("b", "FocusGroupMembers"),
                    new TableInfo("c", "Flags")
                        .SetPrimaryKey("flagid"),
                    new TableInfo("d", "DataSets")
                        .IgnoreColumns(new[] { "dsp", "dsp2", "host"})
                        .SetPrimaryKey("dsid"),
                    new TableInfo("dna", "Dna")
                        .SetPrimaryKey("id_dna"),
                    new TableInfo("e", "EventWitnesses"),
                    new TableInfo("f", "ParentChildRelationships")
                        .SetPrimaryKey("recno"),
                    new TableInfo("g", "Events")
                        .SetPrimaryKey("recno"),
                    new TableInfo("i", "Exhibits")
                        .SetPrimaryKey("idexhibit")
                        .OverrideMaxLength("afilename", 512)
                        .OverrideMaxLength("vfilename", 512)
                        .OverrideMaxLength("caption", 255)
                        .OverrideMaxLength("descript", 512)
                        .OverrideDbType(OleDbType.Binary, new[] { "image", "audio", "video", "thumb"}),
                    new TableInfo("k", "TimelineLocks"),
                    new TableInfo("l", "ResearchLogs")
                        .OverrideMaxLength("task", 512)
                        .OverrideMaxLength("keywords", 512),
                    new TableInfo("m", "Sources")
                        .SetPrimaryKey("majnum")
                        .IgnoreColumns(new[] {"firstcd", "status"})
                        .OverrideMaxLength("title", 255),
                    new TableInfo("n", "Names")
                        .SetPrimaryKey("recno"),
                    new TableInfo("nd", "NameDictionary")
                        .SetPrimaryKey("uid")
                        .OverrideMaxLength("value", 255),
                    new TableInfo("npt", "NamePartTypes")
                        .SetPrimaryKey("id"),
                    new TableInfo("npv", "NamePartValues"),
                    new TableInfo("o", "FocusGroups")
                        .SetPrimaryKey("groupnum"),
                    new TableInfo("p", "Places")
                        .SetPrimaryKey("recno"),
                    new TableInfo("pd", "PlaceDictionary")
                        .SetPrimaryKey("uid")
                        .OverrideMaxLength("value", 255),
                    new TableInfo("ppt", "PlacePartTypes")
                        .SetPrimaryKey("id"),
                    new TableInfo("ppv", "PlacePartValues"),
                    new TableInfo("r", "Repositories")
                        .SetPrimaryKey("recno"),
                    new TableInfo("s", "SourceCitations")
                        .SetPrimaryKey("recno"),
                    new TableInfo("st", "Styles")
                        .SetPrimaryKey("styleid")
                        .OverrideMaxLength("st_display", 512),
                    new TableInfo("t", "TagTypes")
                        .SetPrimaryKey("etypenum")
                        .IgnoreColumn("isreport"),
                    new TableInfo("u", "SourceElements"),
                    new TableInfo("w", "RepositoryLinks"),
                    new TableInfo("xd", "ExcludedPairs"),
                    new TableInfo("_", ProjectSettingsName)
                        .AddColumn(new ColumnInfo("category", 1, OleDbType.Char, 255, null, null))
                        .AddColumn(new ColumnInfo("setting", 2, OleDbType.Char, 255, null, null))
                        .AddColumn(new ColumnInfo("value", 3, OleDbType.Char, 255, null, null))
                        .AddIndex(new IndexInfo("category", "category", "category"))
                        .AddIndex(new IndexInfo("setting", "setting", "setting"))
                        .AddIndex(new IndexInfo("category_and_setting", "category+setting", new[] { "category", "setting"}, true))
                        .SetDataTableBuilder(_iniFileDataTableBuilder)
                };
            return tableInfo;
        }


        public IEnumerable<TableInfo> GetTables(OleDbConnection conn, IDataTableBuilder databaseDataTableBuilder)
        {
            var tablePrefix = GetTablePrefix(_projectFile);
            var tables = new List<TableInfo>();

            var defaultTableInfos = GetDefaultTableInfos().ToList();
            tables.AddRange(defaultTableInfos.Where(t => t.OutputTableName == ProjectSettingsName));

            // all columns not nullable
            // no primary keys found with OleDbSchemaGuid.Primary_Keys
            // column flags aren't super helpful (IsLong or IsFixedLength but can tell by char with int maxlen)
            // none of the columns have default values

            var allIndexes = GetAllIndexes(conn);

            foreach (var tableName in GetTableNames(conn).Where(t => t.StartsWith(tablePrefix)))
            {
                _logger.Info("Processing {0} table...", tableName);

                var tableInfo = GetTableInfo(defaultTableInfos, tableName, tablePrefix);
                tableInfo.SetDataTableBuilder(databaseDataTableBuilder);

                tables.Add(tableInfo);

                AddIndexes(tableInfo, allIndexes.DefaultView);

                foreach (var row in GetTableColumns(conn, tableName).AsEnumerable())
                {
                    AddColumn(tableInfo, row);
                }
            }

            return tables;
        }

        private void ValidateProjectFile(string projectFile)
        {
            var fileName = Path.GetFileNameWithoutExtension(projectFile);
            if (fileName != null && fileName.EndsWith("__") &&
                ".pjc".Equals(Path.GetExtension(projectFile), StringComparison.InvariantCultureIgnoreCase) &&
                File.Exists(projectFile))
                return;

            var message = string.Format("Not a valid project file: {0}", projectFile);
            _logger.Error(message);
            throw new ArgumentException(message, "projectFile");
        }

        private static string GetTablePrefix(string projectFile)
        {
            var fileName = Path.GetFileNameWithoutExtension(projectFile);
            var prefix = fileName != null ? fileName.Remove(fileName.Length - 1).ToLower() : null;
            return prefix;
        }

        private static TableInfo GetTableInfo(IEnumerable<TableInfo> defaultTableInfos, string tableName, string tablePrefix)
        {
            var key = tableName.Substring(tablePrefix.Length);
            var tableInfo = defaultTableInfos.First(t => t.Key == key);
            tableInfo.InputTableName = tableName;
            return tableInfo;
        }

        private static DataTable GetAllIndexes(OleDbConnection conn)
        {
            var indexes = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Indexes, new object[] { null, null, null, null });
            if (indexes == null)
                throw new InvalidOperationException("Failed to retrieve the indexes");

            return indexes;
        }

        private static DataTable GetTableColumns(OleDbConnection conn, string table)
        {
            var tableColumns = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] {null, null, table});
            if (tableColumns == null)
                throw new InvalidOperationException(string.Format("Couldn't get the columns for table {0}", table));
            return tableColumns;
        }

        private static void AddColumn(TableInfo tableInfo, DataRow row)
        {
            const string numericPrecisionColumn = "NUMERIC_PRECISION";
            const string numericScaleColumn = "NUMERIC_SCALE";
            const string characterMaximumLengthColumn = "CHARACTER_MAXIMUM_LENGTH";

            var columnName = row["COLUMN_NAME"].ToString();
            if (tableInfo.IsIgnoredColumn(columnName))
                return;

            var dbType = (OleDbType) row["DATA_TYPE"];
            OleDbType overiddenDbType;
            if (tableInfo.IsDbTypeOverridden(columnName, out overiddenDbType))
                dbType = overiddenDbType;

            var ordinalPosition = (Int64) row["ORDINAL_POSITION"];
            int? numericPrecision = null;
            if (row[numericPrecisionColumn] != DBNull.Value)
            {
                numericPrecision = int.Parse(row[numericPrecisionColumn].ToString());
            }
            int? numericScale = null;
            if (row[numericScaleColumn] != DBNull.Value)
            {
                numericScale = int.Parse(row[numericScaleColumn].ToString());
            }
            long? maxLength = null;
            if (row[characterMaximumLengthColumn] != DBNull.Value)
            {
                maxLength = long.Parse(row[characterMaximumLengthColumn].ToString());
            }

            long overriddenMaxLength;
            if (tableInfo.IsMaxLengthOverridden(columnName, out overriddenMaxLength))
            {
                maxLength = overriddenMaxLength;
            }

            switch (dbType)
            {
                case OleDbType.Char:
                case OleDbType.Integer:
                case OleDbType.Numeric:
                case OleDbType.Boolean:
                case OleDbType.DBDate:
                case OleDbType.Binary:
                    var columnInfo = new ColumnInfo(columnName, ordinalPosition, dbType, maxLength, numericPrecision,
                                                    numericScale,
                                                    tableInfo.PrimaryKey != null && columnName.Equals(tableInfo.PrimaryKey));
                    tableInfo.AddColumn(columnInfo);
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Unknown database type: {0}", dbType));
            }
        }

        private static void AddIndexes(TableInfo tableInfo, DataView allIndexes)
        {
            const string viewColumnName = "COLUMN_NAME";
            const string viewExpression = "EXPRESSION";

            var table = tableInfo.InputTableName;

            allIndexes.RowFilter = string.Format("TABLE_NAME = '{0}'",  table);

            foreach (var indexName in GetIndexNames(allIndexes))
            {
                allIndexes.RowFilter = string.Format("TABLE_NAME = '{0}' AND INDEX_NAME = '{1}'", table, indexName);
                if (allIndexes.Count == 1)
                {
                    var columnName = allIndexes[0][viewColumnName].ToString();
                    var expression = allIndexes[0][viewExpression].ToString();
                    if (!tableInfo.IsIgnoredColumn(columnName))
                        tableInfo.AddIndex(new IndexInfo(indexName, expression, columnName,
                                                         tableInfo.IsUniqueColumns(new[] {columnName})));
                }
                else if (allIndexes.Count > 1)
                {
                    var expression = allIndexes[0][viewExpression].ToString();
                    var columnNames = (from DataRowView row in allIndexes select row[viewColumnName].ToString()).ToList();
                    if (!columnNames.Any(tableInfo.IsIgnoredColumn))
                        tableInfo.AddIndex(new IndexInfo(indexName, expression, columnNames,
                                                         tableInfo.IsUniqueColumns(columnNames)));
                }
            }
        }

        private static IEnumerable<string> GetTableNames(OleDbConnection conn)
        {
            var dataTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            if (dataTable == null)
                return new string[] {};

            //var columnNames = GetColumnNames(dataTable).ToList();

            var tableNames = dataTable.AsEnumerable().Select(row => row["TABLE_NAME"].ToString());
            return tableNames.OrderBy(n => n);
        }

        private static IEnumerable<string> GetColumnNames(DataTable table)
        {
            return (from DataColumn c in table.Columns select c.ColumnName);
        }

        private static IEnumerable<string> GetIndexNames(DataView indexes)
        {
            var list = new List<string>();

            foreach (var indexName in indexes.Cast<DataRowView>().Select(row => row["INDEX_NAME"].ToString()).Where(indexName => !list.Contains(indexName)))
            {
                list.Add(indexName);
            }
            return list.OrderBy(n => n);
        }
    }
}
