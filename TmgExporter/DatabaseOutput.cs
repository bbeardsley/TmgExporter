using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using NLog;

namespace TmgExporter
{
    public class DatabaseOutput : IDatabaseOutput
    {
        private readonly IDbConnection _conn;
        private readonly ISqlBuilder _sqlBuilder;
        private readonly ILogger _logger;

        public DatabaseOutput(IDbConnection conn, ISqlBuilder sqlBuilder)
            : this(conn, sqlBuilder, LogManager.GetLogger("DatabaseOutput"))
        {
        }

        public DatabaseOutput(IDbConnection conn, ISqlBuilder sqlBuilder, ILogger logger)
        {
            _logger = logger;
            _conn = conn;
            _sqlBuilder = sqlBuilder;
        }

        public void Open()
        {
            _conn.Open();
        }

        public void Close()
        {
            _conn.Close();
        }

        public void CreateSchema(IEnumerable<TableInfo> tables)
        {
            _logger.Debug("Entering CreateSchema...");
            using (var transaction = _conn.BeginTransaction())
            {
                foreach (var table in tables)
                {
                    _logger.Info("Creating {0} table...", table.OutputTableName);

                    var sql = _sqlBuilder.BuildTableCreateSql(table);
                    ExecuteNonQuery(sql, transaction);

                    foreach (var index in table.Indexes)
                    {
                        _logger.Debug("Creating {0} index for table {1}...", index.Name, table.OutputTableName);
                        sql = _sqlBuilder.BuildIndexCreateSql(table, index);
                        ExecuteNonQuery(sql, transaction);
                    }
                }
                transaction.Commit();
            }
            _logger.Debug("Leaving CreateSchema...");
        }

        public string GetCreateSchemaSql(IEnumerable<TableInfo> tables)
        {
            var sb = new StringBuilder();
            foreach (var table in tables)
            {
                var sql = _sqlBuilder.BuildTableCreateSql(table);
                sb.AppendLine(sql);

                foreach (var index in table.Indexes)
                {
                    sql = _sqlBuilder.BuildIndexCreateSql(table, index);
                    sb.AppendLine(sql);
                }
            }
            return sb.ToString();
        }

        private void ExecuteNonQuery(string sql, IDbTransaction transaction)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText = sql;
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw;
                }
            }
        }


        public void Output(TableInfo table, DataTable data)
        {
            if (_conn == null)
                return;

            var tableName = data.TableName;
            _logger.Info("Inserting the {0} data...", tableName);

            using (var insertCommand = _conn.CreateCommand())
            {
                _sqlBuilder.ConfigureTableInsert(insertCommand, table);
                using (var transaction = _conn.BeginTransaction())
                {
                    insertCommand.Transaction = transaction;
                    InsertData(data, insertCommand);
                    transaction.Commit();
                }
            }

            _logger.Info("Finished inserting the {0} data.", tableName);
        }

        private void InsertData(DataTable dataTable, IDbCommand insertCommand)
        {
            var tableName = dataTable.TableName;
            var columnCount = dataTable.Columns.Count;
            var count = dataTable.Rows.Count;
            var rowNum = 0;
            foreach (var row in dataTable.AsEnumerable())
            {
                rowNum++;

                if (_logger.IsInfoEnabled && (rowNum % 500 == 0))
                    _logger.Info("Processing {0}: {1} of {2}...", tableName, rowNum, count);
                else if (_logger.IsDebugEnabled)
                    _logger.Debug("Processing {0}: {1} of {2}...", tableName, rowNum, count);

                for (var i = 0; i < columnCount; i++)
                {
                    var parameter = (IDbDataParameter)insertCommand.Parameters[i];
                    parameter.Value = row[i];
                }
                try
                {
                    var result = insertCommand.ExecuteNonQuery();
                    if (result != 1)
                        _logger.Error("Failed to insert {0} record {1}", tableName, rowNum);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    throw;
                }
            }
        }

    }
}