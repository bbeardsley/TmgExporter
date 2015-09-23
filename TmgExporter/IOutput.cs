using System.Data;

namespace TmgExporter
{
    public interface IOutput
    {
        void Output(TableInfo table, DataTable data);
    }
}
