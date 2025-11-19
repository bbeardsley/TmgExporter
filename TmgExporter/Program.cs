using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Data.OleDb;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;
using MySqlConnector;
using NLog;
using Newtonsoft.Json;
using Npgsql;

namespace TmgExporter
{
    class Program
    {
        private const string FoxProConnectionStringFormat =  @"Provider=VFPOLEDB.1;Data Source={0}" ;

        static int Main(string[] args)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
            
            var tmgOption = new Option<string>(
                aliases: new[] { "-t", "--tmg" },
                description: "TMG project file to read data from (*.pjc)")
            {
                IsRequired = true
            };
            
            var csvOption = new Option<bool>(
                aliases: new[] { "-c", "--csv" },
                description: "Dump tables to csv");
            
            var jsonOption = new Option<bool>(
                aliases: new[] { "-j", "--json" },
                description: "Dump tables to json");
            
            var xmlOption = new Option<bool>(
                aliases: new[] { "-x", "--xml" },
                description: "Dump tables to xml");
            
            var sqliteOption = new Option<string?>(
                aliases: new[] { "-l", "--sqlite" },
                description: "SQLite database file to be created (*.sqlite3)");
            
            var mysqlOption = new Option<string?>(
                aliases: new[] { "-m", "--mysql" },
                description: "MySQL/MariaDB database connection string");
            
            var postgresOption = new Option<string?>(
                aliases: new[] { "-p", "--postgres" },
                description: "PostgreSQL database connection string");
            
            var sqlserverOption = new Option<string?>(
                aliases: new[] { "-s", "--sqlserver" },
                description: "Sql Server database connection string");

            var rootCommand = new RootCommand($"TmgExporter {version}")
            {
                tmgOption,
                csvOption,
                jsonOption,
                xmlOption,
                sqliteOption,
                mysqlOption,
                postgresOption,
                sqlserverOption
            };

            rootCommand.SetHandler((projectFile, dumpCsv, dumpJson, dumpXml, sqliteDb, mysqlConnectionString, postgresConnectionString, sqlserverConnectionString) =>
            {
                var sqliteSpecified = !string.IsNullOrEmpty(sqliteDb);
                var mysqlSpecified = !string.IsNullOrEmpty(mysqlConnectionString);
                var postgresSpecified = !string.IsNullOrEmpty(postgresConnectionString);
                var sqlserverSpecified = !string.IsNullOrEmpty(sqlserverConnectionString);
                
                ExecuteExport(projectFile, dumpCsv, dumpJson, dumpXml, sqliteDb, sqliteSpecified, 
                    mysqlConnectionString, mysqlSpecified, postgresConnectionString, postgresSpecified,
                    sqlserverConnectionString, sqlserverSpecified);
            }, 
            tmgOption, csvOption, jsonOption, xmlOption, sqliteOption, mysqlOption, postgresOption, sqlserverOption);

            return rootCommand.Invoke(args);
        }

        static void ExecuteExport(string projectFile, bool dumpCsv, bool dumpJson, bool dumpXml,
            string? sqliteDb, bool sqliteSpecified, string? mysqlConnectionString, bool mysqlSpecified,
            string? postgresConnectionString, bool postgresSpecified, string? sqlserverConnectionString, 
            bool sqlserverSpecified)
        {

            var logger = LogManager.GetLogger("Program");

            if (!File.Exists(projectFile))
            {
                var message = string.Format("Project file not found: {0}", projectFile);
                logger.Error(message);
                Environment.Exit(-2);
                return;
            } 
            if (!".pjc".Equals(Path.GetExtension(projectFile), StringComparison.InvariantCultureIgnoreCase))
            {
                var message = string.Format("Please specify a valid project file: {0}", projectFile);
                logger.Error(message);
                Environment.Exit(-3);
                return;
            }

            if (sqliteSpecified && File.Exists(sqliteDb))
            {
                var message = string.Format("Database already exists: {0}", sqliteDb);
                logger.Error(message);
                Environment.Exit(-4);
                return;
            }

            if (Registry.ClassesRoot.OpenSubKey("TypeLib\\{50BAEECA-ED25-11D2-B97B-000000000000}") == null)
            {
                const string message = "Visual Fox Pro OLE DB driver not installed.  Please go to https://www.microsoft.com/en-us/download/details.aspx?id=14839 and download and install the driver.";
                logger.Error(message);
                Environment.Exit(-5);
                return;
            }

            var dbOutputs = new List<IDatabaseOutput>();
            if (sqliteSpecified)
            {
                // Create SQLite file if it doesn't exist
                if (!File.Exists(sqliteDb))
                {
                    File.Create(sqliteDb).Dispose();
                }
                var toConnection = new SqliteConnection(string.Format("Data Source={0}", sqliteDb));
                var sqlBuilder = new SqlBuilder(v => string.Format("[{0}]", v), "DATETIME", "BLOB", "INTEGER", "BOOLEAN");
                dbOutputs.Add(new DatabaseOutput(toConnection, sqlBuilder));
            }
            if (mysqlSpecified)
            {
                //Server=ipaddr;uid=tmg;pwd=tmg;database=tmg
                var toConnection = new MySqlConnection(mysqlConnectionString);
                var sqlBuilder = new SqlBuilder(v => string.Format("`{0}`", v), "DATETIME", "BLOB", "INTEGER", "BOOLEAN");
                dbOutputs.Add(new DatabaseOutput(toConnection, sqlBuilder));
            }
            if (postgresSpecified)
            {
                //Server=ipaddr;uid=tmg;pwd=tmg;database=tmg
                var toConnection = new NpgsqlConnection(postgresConnectionString);
                var sqlBuilder = new SqlBuilder(v => string.Format("\"{0}\"", v), "timestamp", "bytea", "integer", "boolean");
                dbOutputs.Add(new DatabaseOutput(toConnection, sqlBuilder));
            }
            if (sqlserverSpecified)
            {
                var toConnection = new SqlConnection(sqlserverConnectionString);
                var sqlBuilder = new SqlBuilder(v => string.Format("[{0}]", v), "Datetime", "Varbinary(max)", "Int", "bit");
                dbOutputs.Add(new DatabaseOutput(toConnection, sqlBuilder));
            }

            if (dbOutputs.Count == 0 && !dumpJson && !dumpXml && !dumpCsv)
            {
                const string message = "Please specify a valid output format using -c, -j, -l, -m, -p, -s, or -x";
                logger.Error(message);
                Environment.Exit(-6);
                return;
            }

            var allOutputs = new List<IOutput>(dbOutputs);
            if (dumpJson)
            {
                allOutputs.Add(new JsonOutput(dt => JsonConvert.SerializeObject(dt, Formatting.Indented)));
            }
            if (dumpXml)
            {
                allOutputs.Add(new XmlOutput());
            }
            if (dumpCsv)
            {
                allOutputs.Add(new CsvOutput());
            }
            try
            {
                logger.Info("Started export");

                var project = new TmgProject(projectFile);

                using (var fromConnection = new OleDbConnection(string.Format(FoxProConnectionStringFormat, Path.GetDirectoryName(projectFile))))
                {
                    fromConnection.Open();

                    var tables = project.GetTables(fromConnection, new DatabaseDataTableBuilder(fromConnection)).ToList();

                    foreach (var dbOutput in dbOutputs)
                    {
                        dbOutput.Open();
                        dbOutput.CreateSchema(tables);
                    }

                    foreach (var table in tables)
                    {
                        var data = table.GetData();

                        foreach (var output in allOutputs)
                        {
                            output.Output(table, data);
                        }
                    }
                }

                logger.Info("Finished export");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            finally
            {
                foreach (var dbOutput in dbOutputs)
                {
                    dbOutput.Close();
                }
            }
        }
    }
}
