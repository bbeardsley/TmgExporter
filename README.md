# TmgExporter
A command line tool to export The Master Genealogist (TMG) data to a variety of databases and file formats like SQLite and JSON.

# Prerequisites
The Master Genealogist (TMG) data is stored in FoxPro tables and this software requires the
[Microsoft OLE DB Provider for Visual FoxPro 9.0](https://www.microsoft.com/en-us/download/details.aspx?id=14839)
to be installed on your system prior to running this software.

# Command line parameters

Required parameter:
-------------------
t:tmg          TMG project file to read data from (*.pjc)

And one of the following parameters:
------------------------------------
c:csv          Dump tables to csv  

j:json         Dump tables to json  

l:sqlite       SQLite database file to be created (*.sqlite3)  

m:mysql        [MySQL/MariaDB database connection string](http://www.connectionstrings.com/mysql/)  

p:postgres     [PostgreSQL database connection string](http://www.connectionstrings.com/postgresql/)  

s:sqlserver    [SQL Server database connection string](http://www.connectionstrings.com/sql-server/)  

x:xml          Dump tables to xml

# Examples
	TmgExporter -t "C:\Program Files (x86)\The Master Genealogist v8\sample\sample__.pjc" -j -l sample.sqlite3

This will dump the sample TMG database to JSON and to a SQLite database called sample.sqlite3
