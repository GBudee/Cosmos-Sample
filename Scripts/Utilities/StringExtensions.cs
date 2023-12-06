using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
{
    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, System.StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static string RemoveSubstring(this string sourceString, string toCheck)
        {
            int index = sourceString.IndexOf(toCheck, System.StringComparison.Ordinal);
            return (index < 0) ? sourceString : sourceString.Remove(index, toCheck.Length);
        }

        public static string ToVerboseString<T>(this IEnumerable<T> list, bool addBrace = true)
        {
            StringBuilder s = new StringBuilder();
            if (addBrace) s.Append("{");
            int index = 0;
            foreach (var t in list)
            {
                if (index != 0) s.Append(", ");
                s.Append(t.ToString());
                index++;
            }
            
            if (addBrace) s.Append("}");
            return s.ToString();
        }

        public static string ToVerboseString<T>(this IEnumerable<T> list, Func<T, string> toString, bool addBrace = true)
        {
            StringBuilder s = new StringBuilder();
            if (addBrace) s.Append("{");
            int index = 0;
            foreach (var t in list)
            {
                if (index != 0) s.Append(", ");
                s.Append(toString(t));
                index++;
            }
            
            if (addBrace) s.Append("}");
            return s.ToString();
        }

        public static string AddSpacesToCamelCase(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                    newText.Append(' ');
                newText.Append(text[i]);
            }

            return newText.ToString();
        }
    }
}