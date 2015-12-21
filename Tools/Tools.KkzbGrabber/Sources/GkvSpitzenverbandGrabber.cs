using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Tools.KkzbGrabber.Sources
{
    [Description("www.gkv-spitzenverband.de")]
    class GkvSpitzenverbandGrabber : IGrabber
    {
        public IEnumerable<KeyValuePair<string, Rate>> GetBeitraege()
        {
            var pages = new List<string>();

            int lastPageId;
            pages.Add(GetFirstPage(out lastPageId));

            for (var page = 2; page <= lastPageId; ++page)
            {
                pages.Add(GetPage(page));
            }

            foreach (var content in pages)
            {
                var tableMatch = Regex.Match(content, @"<\s*div\s+class=""clearfix module textImage careInsuranceTable""\s*>.*?<\s*div(?:\s+class=""[^""]*"")\s*>.*?<\s*table(?:\s+class=""[^""]*"")\s*>.*?<thead>.*?<\/thead>.*?<tbody>(.+?)<\/tbody>.*?</table>.*?<\/div>.*?<\/div>", RegexOptions.Singleline);
                var table = tableMatch.Groups[1].Value;

                var rowMatches = Regex.Matches(table, @"<tr>.*?<th[^>]*>.*?<a[^>]*>(.+?)<\/a>.*?<\/th>.*?<td[^>]*>.*?<\/td>.*?<td[^>]*>.*?(\d+),(\d{2})\s*%.*?<\/td>.*?<\/tr>", RegexOptions.Singleline);
                foreach (var row in rowMatches.Cast<Match>())
                {
                    var name = row.Groups[1].Value.Trim();
                    var value = row.Groups[2].Value;
                    var decimals = row.Groups[3].Value;
                    var rate = int.Parse(value) * 100 + int.Parse(decimals);

                    yield return new KeyValuePair<string, Rate>(name, new Rate(rate, -4));
                }
            }
        }

        private static string GetPage(int pageNo)
        {
            return GetUriContent($"https://www.gkv-spitzenverband.de/service/versicherten_service/krankenkassenliste/krankenkassen.jsp?pageNo={pageNo}");
        }

        private static string GetFirstPage(out int lastPageId)
        {
            var content = GetUriContent("https://www.gkv-spitzenverband.de/service/versicherten_service/krankenkassenliste/krankenkassen.jsp");

            var pagerMatches = Regex.Matches(content, @"<\s*div\s+class=""clearfix pagingModule careInsurancePagingModule""\s*>.+?<\s*div\s+class=""clearfix pager""\s*>(.+)<\/div>", RegexOptions.Singleline);
            var pagers = string.Join(Environment.NewLine, pagerMatches.Cast<Match>().Select(m => m.Groups[1].Value));

            var pageNoMatches = Regex.Matches(pagers, @"href=""\/[^""]+pageNo=(\d+)(?>\D)");
            lastPageId = pageNoMatches.Cast<Match>().Select(m => int.Parse(m.Groups[1].Value)).Max();

            return content;
        }

        private static string GetUriContent(string uri)
        {
            var request = WebRequest.CreateHttp(uri);
            request.KeepAlive = false;
            request.Method = WebRequestMethods.Http.Get;

            using (var buffer = new MemoryStream())
            {
                using (var response = request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        stream.CopyTo(buffer);
                    }
                }

                return Encoding.UTF8.GetString(buffer.ToArray());
            }
        }
    }
}
