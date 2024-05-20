using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWebScraping
{
  public static class StringExtensions
  {
    public static string DecodeAndSanitize(this string s)
    {
      s = WebUtility.HtmlDecode(s);
      foreach (char c in Path.GetInvalidFileNameChars())
      {
        s = s.Replace(c, '_');
      }

      return s;
    }
  }
}
