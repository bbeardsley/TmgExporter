using System.Data;

namespace TmgExporter
{
    public interface IDataTableBuilder
    {
        DataTable BuildDataTable(TableInfo table);
    }
}
