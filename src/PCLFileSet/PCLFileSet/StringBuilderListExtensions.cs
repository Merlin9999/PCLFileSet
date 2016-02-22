using System.Collections.Generic;
using System.Text;

namespace PCLFileSet
{
    internal static class StringBuilderListExtensions
    {
        public static List<StringBuilder> Append(this List<StringBuilder> regexes, char toAppend)
        {
            foreach (StringBuilder regexStr in regexes)
                regexStr.Append(toAppend);

            return regexes;
        }

        public static List<StringBuilder> Append(this List<StringBuilder> regexes, string toAppend)
        {
            foreach (StringBuilder regexStr in regexes)
                regexStr.Append(toAppend);

            return regexes;
        }
    }
}