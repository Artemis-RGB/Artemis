using System.Text;

namespace Artemis.Core;

/// <summary>
///     Provides some random string utilities.
/// </summary>
public static class StringUtilities
{
    /// <summary>
    ///     Produces optional, URL-friendly version of a title, "like-this-one".
    ///     hand-tuned for speed, reflects performance refactoring contributed
    ///     by John Gietzen (user otac0n)
    /// </summary>
    /// <remarks>Source: https://stackoverflow.com/a/25486</remarks>
    public static string UrlFriendly(string? title)
    {
        if (title == null) return "";

        const int maxlen = 80;
        int len = title.Length;
        bool prevdash = false;
        StringBuilder sb = new(len);
        char c;

        for (int i = 0; i < len; i++)
        {
            c = title[i];
            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
            {
                sb.Append(c);
                prevdash = false;
            }
            else if (c >= 'A' && c <= 'Z')
            {
                // tricky way to convert to lowercase
                sb.Append((char) (c | 32));
                prevdash = false;
            }
            else if (c == ' ' || c == ',' || c == '.' || c == '/' ||
                     c == '\\' || c == '-' || c == '_' || c == '=')
            {
                if (!prevdash && sb.Length > 0)
                {
                    sb.Append('-');
                    prevdash = true;
                }
            }
            else if (c >= 128)
            {
                int prevlen = sb.Length;
                sb.Append(RemapInternationalCharToAscii(c));
                if (prevlen != sb.Length) prevdash = false;
            }

            if (i == maxlen) break;
        }

        if (prevdash)
            return sb.ToString().Substring(0, sb.Length - 1);
        return sb.ToString();
    }

    /// <summary>
    ///     Remaps internation characters to their ASCII equivalent.
    ///     <remarks>Source: https://meta.stackexchange.com/a/7696</remarks>
    /// </summary>
    /// <param name="c">The character to remap</param>
    /// <returns>The ASCII equivalent.</returns>
    public static string RemapInternationalCharToAscii(char c)
    {
        string s = c.ToString().ToLowerInvariant();
        if ("àåáâäãåą".Contains(s))
        {
            return "a";
        }
        else if ("èéêëę".Contains(s))
        {
            return "e";
        }
        else if ("ìíîïı".Contains(s))
        {
            return "i";
        }
        else if ("òóôõöøőð".Contains(s))
        {
            return "o";
        }
        else if ("ùúûüŭů".Contains(s))
        {
            return "u";
        }
        else if ("çćčĉ".Contains(s))
        {
            return "c";
        }
        else if ("żźž".Contains(s))
        {
            return "z";
        }
        else if ("śşšŝ".Contains(s))
        {
            return "s";
        }
        else if ("ñń".Contains(s))
        {
            return "n";
        }
        else if ("ýÿ".Contains(s))
        {
            return "y";
        }
        else if ("ğĝ".Contains(s))
        {
            return "g";
        }
        else if (c == 'ř')
        {
            return "r";
        }
        else if (c == 'ł')
        {
            return "l";
        }
        else if (c == 'đ')
        {
            return "d";
        }
        else if (c == 'ß')
        {
            return "ss";
        }
        else if (c == 'Þ')
        {
            return "th";
        }
        else if (c == 'ĥ')
        {
            return "h";
        }
        else if (c == 'ĵ')
        {
            return "j";
        }
        else
        {
            return "";
        }
    }
}