using System.Text;

namespace QA.Search.Generic.Integration.QP.Extensions
{
    internal static class StringBuilderExtensions
    {
        /// <summary>
        /// Remove last occurence of <paramref name="separator"/> character
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="separator"></param>
        public static void RemoveLastSeparator(this StringBuilder sb, char separator)
        {
            for (int i = sb.Length - 1; i >= 0; i--)
            {
                if (sb[i] == separator)
                {
                    sb.Remove(i, 1);
                    break;
                }
            }
        }

        /// <summary>
        /// If <see cref="StringBuilder"/> ends with line break — insert <paramref name="text"/>
        /// before line break or append it to the end of <see cref="StringBuilder"/> otherwise.
        /// </summary>
        public static void InsertBeforeTrailingLineBreak(this StringBuilder sb, string text)
        {
            if (sb.Length > 0 && sb[sb.Length - 1] == '\n')
            {
                if (sb.Length > 1 && sb[sb.Length - 2] == '\r')
                {
                    sb.Insert(sb.Length - 2, text);
                }
                else
                {
                    sb.Insert(sb.Length - 1, text);
                }
            }
            else
            {
                sb.Append(text);
            }
        }
    }
}
