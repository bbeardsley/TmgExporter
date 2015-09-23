using System.Collections.Generic;

namespace TmgExporter
{
    public interface IDatabaseOutput : IOutput
    {
        void Open();

        void Close();

        void CreateSchema(IEnumerable<TableInfo> tables);

        string GetCreateSchemaSql(IEnumerable<TableInfo> tables);
    }
}
