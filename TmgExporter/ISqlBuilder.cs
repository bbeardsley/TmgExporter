using System.Data;

namespace TmgExporter
{
    public interface ISqlBuilder
    {
        string BuildTableCreateSql(TableInfo table);

        string BuildIndexCreateSql(TableInfo table, IndexInfo index);

        void ConfigureTableInsert(IDbCommand command, TableInfo table);
    }
}
