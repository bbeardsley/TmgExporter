using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fclp;
using Fclp.Internals;

namespace TmgExporter
{
    public class HelpFormatter : ICommandLineOptionFormatter
    {
        private readonly string _version;

        public HelpFormatter(string version)
        {
            _version = version;
        }

        public string Format(IEnumerable<ICommandLineOption> incomingOptions)
        {
            var sb = new StringBuilder();

            sb.AppendLine();
            sb.AppendFormat("TmgExporter {0}", _version);
            sb.AppendLine();
            sb.AppendLine();

            var options = incomingOptions.ToList();
            var padding = options.Max(o => o.LongName.Length) + 4;


            var required = options.Where(o => o.IsRequired).OrderBy(o => o.ShortName).ToList();
            var optional = options.Where(o => !o.IsRequired).OrderBy(o => o.ShortName).ToList();
            var hasRequired = required.Any();
            var hasOptional = optional.Any();

            if (hasRequired)
            {
                AddHeader(sb, "Required parameters:");
                sb.Append(GetTextForOptions(required, padding));

                if (hasOptional)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                }
            }

            if (hasOptional)
            {
                AddHeader(sb, "Optional parameters:");
                sb.Append(GetTextForOptions(optional, padding));
            }

            return sb.ToString();
        }

        private static string GetTextForOptions(IEnumerable<ICommandLineOption> options, int padding)
        {
            return string.Join("\r\n", options.Select(o => GetLineForOption(o, padding)));
        }

        private static string GetLineForOption(ICommandLineOption option, int padding)
        {
            return string.Format("\t{0}:{1}{2}", option.ShortName, option.LongName.PadRight(padding), option.Description);
        }

        private static void AddHeader(StringBuilder sb, string headerText)
        {
            var underline = new string('-', headerText.Length);
            sb.Append(headerText);
            sb.AppendLine();
            sb.Append(underline);
            sb.AppendLine();
        }
    }
}
