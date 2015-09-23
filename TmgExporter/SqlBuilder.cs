using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;

namespace TmgExporter
{
    public class SqlBuilder : ISqlBuilder
    {
        private readonly string _dateTimeTypeName;
        private readonly string _blobType;
        private readonly string _integerType;
        private readonly string _booleanType;
        private readonly Func<string, string> _parameterNameBuilder;
        private readonly Func<string, string> _escaper;

        public SqlBuilder(Func<string, string> escaper, string dateTimeTypeName, string blobType, string integerType, string booleanType)
        {
            _booleanType = booleanType;
            _integerType = integerType;
            _dateTimeTypeName = dateTimeTypeName;
            _blobType = blobType;
            _parameterNameBuilder = columnName => string.Format("@{0}", columnName);
            _escaper = escaper;
        }

        public string BuildTableCreateSql(TableInfo table)
        {
            return string.Format("CREATE TABLE {0} ({1});", _escaper(table.OutputTableName),
                string.Join(",", table.Columns.OrderBy(c => c.OrdinalPosition).Select(BuildCreateTableColumnSql)));
        }

        private string BuildCreateTableColumnSql(ColumnInfo column)
        {
            var sb = new StringBuilder();
            sb.Append(_escaper(column.Name));
            var isPrimaryKey = column.IsPrimaryKey ? " PRIMARY KEY NOT NULL" : "";
            switch (column.DbType)
            {
                case OleDbType.Char:
                    if (column.CharacterMaxLength >= int.MaxValue)
                    {
                        sb.AppendFormat("TEXT");
                    }
                    else
                    {
                        sb.AppendFormat("VARCHAR({0})", column.CharacterMaxLength);
                    }
                    break;
                case OleDbType.Integer:
                    if (column.NumericPrecision != 4)
                        throw new NotSupportedException(string.Format("Only int(4) is supported at this time..."));

                    sb.AppendFormat("{0}{1}", _integerType, isPrimaryKey);
                    break;
                case OleDbType.Numeric:
                    const string numericVarType = "NUMERIC";
                    if (column.NumericPrecision != null)
                    {
                        if (column.NumericScale != null && column.NumericScale > 0)
                        {
                            sb.AppendFormat("{0}({1},{2}){3}", numericVarType, column.NumericPrecision, column.NumericScale,
                                            isPrimaryKey);
                        }
                        else
                        {
                            sb.AppendFormat("{0}({1}){2}", numericVarType, column.NumericPrecision, isPrimaryKey);
                        }
                    }
                    else
                    {
                        sb.AppendFormat("{0}{1}", numericVarType, isPrimaryKey);
                    }
                    break;
                case OleDbType.Boolean:
                    sb.AppendFormat("{0}{1}", _booleanType, isPrimaryKey);
                    break;
                case OleDbType.DBDate:
                    sb.AppendFormat("{0}{1}", _dateTimeTypeName, isPrimaryKey);
                    break;
                case OleDbType.Binary:
                    sb.AppendFormat(_blobType);
                    break;
            }
            return sb.ToString();
        }

        public string BuildIndexCreateSql(TableInfo table, IndexInfo index)
        {
            var indexName = string.Format("{0}_{1}", table.OutputTableName, index.Name);

            var unique = index.IsUnique ? "UNIQUE " : "";

            return string.Format("CREATE {0}INDEX {1} ON {2} ({3});", 
                                unique, _escaper(indexName), _escaper(table.OutputTableName),
                                string.Join(",", index.ColumnNames.Select(c => _escaper(c))));
        }

        private string BuildTableInsertSql(TableInfo table, IEnumerable<ColumnInfo> columns)
        {
            var columnNames = columns.Select(c => c.Name).ToList();

            var sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2});",
                                    _escaper(table.OutputTableName),
                                    string.Join(",", columnNames.Select(c => _escaper(c))),
                                    string.Join(",", columnNames.Select(c => _parameterNameBuilder(c))));
            return sql;
        }

        public void ConfigureTableInsert(IDbCommand command, TableInfo table)
        {
            var columns = table.Columns.OrderBy(c => c.OrdinalPosition).ToList();
            command.CommandText = BuildTableInsertSql(table, columns);
            foreach (var column in columns)
            {
                var parameter = command.CreateParameter();
                ConfigureParameter(parameter, column);
                command.Parameters.Add(parameter);
            }
        }

        private void ConfigureParameter(IDbDataParameter parameter, ColumnInfo column)
        {
            parameter.ParameterName = _parameterNameBuilder(column.Name);
            switch (column.DbType)
            {
                case OleDbType.Char:
                    if (column.CharacterMaxLength >= int.MaxValue)
                    {
                        parameter.DbType = DbType.String;
                    }
                    else
                    {
                        parameter.DbType = DbType.StringFixedLength;
                        parameter.Size = int.Parse(column.CharacterMaxLength.ToString());
                    }
                    break;
                case OleDbType.Integer:
                    parameter.DbType = DbType.Int32;
                    break;
                case OleDbType.Numeric:
                    parameter.DbType = DbType.Int32;
                    break;
                case OleDbType.Boolean:
                    parameter.DbType = DbType.Boolean;
                    break;
                case OleDbType.Binary:
                    parameter.DbType = DbType.Binary;
                    break;
                case OleDbType.DBDate:
                    parameter.DbType = DbType.DateTime;
                    break;
            }
        }
    }
}