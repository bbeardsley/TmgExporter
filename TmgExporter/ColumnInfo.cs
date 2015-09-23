using System.Data.OleDb;
using System.Text;

namespace TmgExporter
{
    public class ColumnInfo
    {
        public string Name { get; private set; }
        public long OrdinalPosition { get; private set; }
        public OleDbType DbType { get; private set; }
        public long? CharacterMaxLength { get; private set; }
        public int? NumericPrecision { get; private set; }
        public int? NumericScale { get; private set; }
        public bool IsPrimaryKey { get; private set; }

        public ColumnInfo(string name, long ordinalPosition, OleDbType dbType, long? characterMaxLength, int? numericPrecision, int? numericScale, bool isPrimaryKey = false)
        {
            Name = name;
            OrdinalPosition = ordinalPosition;
            DbType = dbType;
            CharacterMaxLength = characterMaxLength;
            NumericPrecision = numericPrecision;
            NumericScale = numericScale;
            IsPrimaryKey = isPrimaryKey;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0}:{1}, {2}", OrdinalPosition, Name, DbType);

            switch (DbType)
            {
                case OleDbType.Char:
                    if (CharacterMaxLength != null)
                    {
                        sb.AppendFormat("({0})", CharacterMaxLength);
                    }
                    break;
                case OleDbType.Integer:
                case OleDbType.Numeric:
                    if (NumericPrecision != null)
                    {
                        if (NumericScale != null)
                        {
                            sb.AppendFormat("({0}:{1})", NumericPrecision, NumericScale);
                        }
                        else
                        {
                            sb.AppendFormat("({0})", NumericPrecision);
                        }
                    }
                    break;
            }
            return sb.ToString();
        }
    }
}
