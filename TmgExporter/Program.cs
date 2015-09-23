using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using Fclp;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
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
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string projectFile = null;
            string sqliteDb = null;
            var postgresSpecified = false;
            var sqliteSpecified = false;
            var mysqlSpecified = false;
            string mysqlConnectionString = null;
            string postgresConnectionString = null;
            var dumpJson = false;
            var dumpXml = false;
            var dumpCsv = false;
            var sqlserverSpecified = false;
            string sqlserverConnectionString = null;

            var parser = new FluentCommandLineParser();
            parser.Setup<bool>('c', "csv")
                  .Callback(val => dumpCsv = val)
                  .WithDescription("Dump tables to csv");
            parser.SetupHelp("h", "help", "?")
                  .Callback(text => Console.WriteLine(text))
                  .WithCustomFormatter(new HelpFormatter(version))
                  .WithHeader("TmgExporter")
                  .UseForEmptyArgs();
            parser.Setup<string>('t', "tmg")
                  .Callback(val => projectFile = val)
                  .WithDescription("TMG project file to read data from (*.pjc)")
                  .Required();
            parser.Setup<bool>('j', "json")
                  .Callback(val => dumpJson = val)
                  .WithDescription("Dump tables to json");
            parser.Setup<string>('l', "sqlite")
                  .Callback(val => {
                      sqliteDb = val;
                      sqliteSpecified = true;
                  })
                  .WithDescription("SQLite database file to be created (*.sqlite3)");
            parser.Setup<string>('m', "mysql")
                  .Callback(val =>
                  {
                      mysqlConnectionString = val;
                      mysqlSpecified = true;
                  })
                  .WithDescription("MySQL/MariaDB database connection string");
            parser.Setup<string>('p', "postgres")
                  .Callback(val =>
                  {
                      postgresConnectionString = val;
                      postgresSpecified = true;
                  })
                  .WithDescription("PostgreSQL database connection string");
            parser.Setup<string>('s', "sqlserver")
                  .Callback(val =>
                  {
                      sqlserverConnectionString = val;
                      sqlserverSpecified = true;
                  })
                  .WithDescription("Sql Server database connection string");
            parser.Setup<bool>('x', "xml")
                  .Callback(val => dumpXml = val)
                  .WithDescription("Dump tables to xml");

            var result = parser.Parse(args);
            if (result.HelpCalled)
            {
                return 0;
            }
            if (result.HasErrors)
            {
                Console.Error.WriteLine(result.ErrorText);
                return -1;
            }

            var logger = LogManager.GetLogger("Program");

            if (!File.Exists(projectFile))
            {
                var message = string.Format("Project file not found: {0}", projectFile);
                logger.Error(message);
                return -2;
            } 
            if (!".pjc".Equals(Path.GetExtension(projectFile), StringComparison.InvariantCultureIgnoreCase))
            {
                var message = string.Format("Please specify a valid project file: {0}", projectFile);
                logger.Error(message);
                return -3;
            }

            if (sqliteSpecified && File.Exists(sqliteDb))
            {
                var message = string.Format("Database already exists: {0}", sqliteDb);
                logger.Error(message);
                return -4;
            }

            if (Registry.ClassesRoot.OpenSubKey("TypeLib\\{50BAEECA-ED25-11D2-B97B-000000000000}") == null)
            {
                const string message = "Visual Fox Pro OLE DB driver not installed.  Please go to https://www.microsoft.com/en-us/download/details.aspx?id=14839 and download and install the driver.";
                logger.Error(message);
                return -5;
            }

            var dbOutputs = new List<IDatabaseOutput>();
            if (sqliteSpecified)
            {
                SQLiteConnection.CreateFile(sqliteDb);
                var toConnection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", sqliteDb));
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
                return -6;
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

            return 0;
        }
    }
}
