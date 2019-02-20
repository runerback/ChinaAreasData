using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChinaAreas
{
    sealed class LatestChinaAreaProvider : IChinaAreaProvider
    {
        private Area chinaCountryAreaData;

        public Area GetChinaArea()
        {
            var data = chinaCountryAreaData;

            if (data == null)
            {
                var chinaCountryCache = new AreaCache(
                    name: "中国",
                    url: $@"http://www.stats.gov.cn/tjsj/tjbz/tjyqhdmhcxhfdm/{DateTime.Now.Year - 1}/")
                {
                    ID = "0",
                    Type = "country"
                };

                UpdateCountryCacheSource(chinaCountryCache);

                data = chinaCountryCache.ToData();
                chinaCountryAreaData = data;
            }

            return data;
        }

        private readonly Random rnd = new Random(DateTime.Now.Millisecond);

        private string GetSourceCode(string uri)
        {
            return new PatientWebClient().DownloadAsString(new Uri(uri));
        }

        private void UpdateCountryCacheSource(AreaCache country)
        {
            var baseUrl = country.URL;

            var source = GetSourceCode(baseUrl);

            var match = Regex.Match(source, Filters.Country.filter1, RegexOptions.Singleline);
            if (!match.Success)
                return;
            
            var type = "province";

            var matches = Regex.Matches(match.Groups["sub"]?.Value, Filters.Country.filter2);
            var subAreas = matches
                .OfType<Match>()
                .Select(item =>
                    new AreaCache(
                        item.Groups["name"]?.Value,
                        baseUrl + item.Groups["uri"].Value)
                    {
                        ID = item.Groups["id"]?.Value,
                        Type = type
                    })
                .ToArray();

            foreach (var item in subAreas)
                UpdateSubAreaCacheSource(item);

            country.AddRange(subAreas);
        }

        private void UpdateSubAreaCacheSource(AreaCache area)
        {
            if (area == null)
                return;
            
            var baseUrl = area.URL;

            var source = GetSourceCode(baseUrl);

            var match = Regex.Match(source, Filters.SubArea.filter1, RegexOptions.Singleline);
            if (!match.Success)
                return;

            var type = match.Groups["type"]?.Value;

            var matches = Regex.Matches(match.Groups["sub"]?.Value, Filters.SubArea.filter2);
            var subAreas = matches
                .OfType<Match>()
                .Select(item =>
                    new AreaCache(
                        name: item.Groups["name"]?.Value,
                        url: CombineUrl(baseUrl, item.Groups["uri"]?.Value))
                    {
                        ID = item.Groups["id"]?.Value,
                        Type = type
                    })
                .ToArray();
            
            foreach (var item in subAreas
                .Where(item => !string.IsNullOrWhiteSpace(item.URL)))
            {
                UpdateSubAreaCacheSource(item);
            }

            area.AddRange(subAreas);
        }

        private string CombineUrl(string pre, string cur)
        {
            if (string.IsNullOrWhiteSpace(cur))
                return null;
            return pre.Substring(0, pre.LastIndexOf('/') + 1) + cur;
        }
        
        sealed class Filters
        {
            public sealed class Country
            {
                public const string filter1 = @"<table\s+class\='provincetable'.*?>.*?(?<sub><tr\s+class\='provincetr'>.*?)(?:</tbody>)?</table>";
                public const string filter2 = @"<td><a\s+href='(?<uri>(?<id>\d+)\.h.*?)'>(?<name>.*?)<.*?></a></td>";
            }

            public sealed class SubArea
            {
                public const string filter1 = @"<table\s+class\='(?<type>.*?)table'.*?</tr>.*?(?<sub><tr\s+class\='.*?tr'>.*?)(?:</tbody>)?</table>";
                public const string filter2 = @"<tr\s+class\='.*?tr'><td>(?:<a.*?>)?(?<id>.*?)(?:</a>)?</td>.*?(?:<td>\d+</td>)?.*?<td>(?:<a\s+href\='(?<uri>.*?)'>)?(?<name>.*?)(?:</a>)?</td></tr>";
            }
        }

        sealed class PatientWebClient : WebClient
        {
            private const int retry_count = 3;
            private const int retry_interval = 3600;
            private const int timeout = 3000;

            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address);
                request.Timeout = timeout;
                return request;
            }

            public string DownloadAsString(Uri uri)
            {
                int count = retry_count;
                while (count > 0)
                {
                    try
                    {
                        return DownloadString(uri);
                    }
                    catch
                    {
                        if (--count == 0)
                            throw;
                        Task.Delay(retry_interval).Wait();
                    }
                }
                throw new NotImplementedException();
            }
        }

        string IChinaAreaProvider.Provide()
        {
            return GetChinaArea().ToJson();
        }
    }
}
