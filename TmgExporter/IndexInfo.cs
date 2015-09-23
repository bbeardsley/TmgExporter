using System.Collections.Generic;
using System.Text;

namespace TmgExporter
{
    public class IndexInfo
    {
        private readonly List<string> _columnNames;

        public string Name { get; private set; }
        public string Expression { get; private set; }
        public bool IsUnique { get; private set; }

        public IndexInfo(string name, string expression, string columnName, bool isUnique = false)
            : this(name, expression, new[] { columnName}, isUnique)
        {
        }

        public IndexInfo(string name, string expression, IEnumerable<string> columnNames, bool isUnique = false)
        {
            IsUnique = isUnique;
            Name = name;
            Expression = expression;
            _columnNames = new List<string>();
            if (columnNames != null)
                _columnNames.AddRange(columnNames);
        }

        public IEnumerable<string> ColumnNames
        {
            get { return _columnNames; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder(Name);
            if (_columnNames.Count == 1)
            {
                var columnName = _columnNames[0];
                if (columnName == Expression)
                {
                    sb.AppendFormat(", Column: {0}", columnName);
                }
                else
                {
                    sb.AppendFormat(", Column: {0}, Expression: {1}", columnName, Expression);
                }
            }
            else
            {
                sb.AppendFormat(", Columns: {0}, Expression: {1}", string.Join(",", _columnNames), Expression);
            }

            return sb.ToString();
        }
    }
}
