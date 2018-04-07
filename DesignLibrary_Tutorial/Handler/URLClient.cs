using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

namespace ScheduleApp.Handler
{
    public class URLClient
    {
        private WebClient       mWebClient;
        [JsonProperty]
        public int              mEarlyWeek = 0; //If information is already published on the weekend before the week starts
        public static string[]  mClasses;

        public URLClient()
        {
            mWebClient = new WebClient
            {
                Encoding = Encoding.GetEncoding(1252)
            };
        }

        public string GetRawCode(string url)
        {
            mWebClient.Encoding = Encoding.GetEncoding(1252);
            if (url.Length == 0)
                return null;

            //Downloading htm source from website 
            string source;
            try
            {
                source = mWebClient.DownloadString(url);
            }
            catch (System.Net.WebException e)
            {
                return null;
            }
            return source;
        }

        public string GetRawCode(string[] urlParts, int week, int classIndex)
        {
            CheckEarly(urlParts);
            var url = LinkURL(urlParts, week + mEarlyWeek, classIndex + 1);
            if (url.Length == 0)
                return null;

            var source = GetRawCode(url);
            if (source == null)
                mEarlyWeek = 1;

            return source;
        }

        public string GetRawCode(string[] urlParts, int week)
        {
            CheckEarly(urlParts);
            var url = LinkURL(urlParts, week + mEarlyWeek);
            if (url.Length == 0)
                return null;

            var source = GetRawCode(url);
            if (source == null)
                mEarlyWeek = 1;

            return source;
        }

        private void CheckEarly(string[] urlParts)
        {
            if (mEarlyWeek == 1 && GetRawCode(LinkURL(urlParts, TimeHandler.GetCurrentWeek(), 1)) != null)
                mEarlyWeek = 0;
        }

        public static string GetRawCodeFile( string path )
        {
            return System.IO.File.ReadAllText(path);
        }

        public static string GetClass(string source)
        {
            var classSource = source.Split('\n');
            try
            {
                return classSource[15].Substring(3, classSource[15].Substring(3).IndexOf('<'));
            }catch
            {
                return string.Empty;
            }
        }


        public string[][] GetAllClasses(int week, string[] urlPart)
        {
            List<string> dNames     = new List<string>();
            List<string> dSource    = new List<string>();
            var errc    = 0;
            for (int i = 1; errc < 2; i++)
            {
                var source = GetRawCode(urlPart[0] + week.ToString("D2") + urlPart[1] + i.ToString("D5") + urlPart[2]); //t.GetWeekOfYear()
                if (source != null)
                {
                    errc = 0;
                    dNames.Add(GetClass(source));
                    dSource.Add(source);
                }
                else errc++;
            }
            if (dNames.Count > 0)
                return new string[][] { dNames.ToArray(), dSource.ToArray() };
            return null;
        }

        public string LinkURL(string[] urlParts, int week)
        {
            if (urlParts.Length == 2)
                return urlParts[0] + week.ToString("D2") + urlParts[1];
            return string.Empty;
        }

        public string LinkURL(string[] urlParts, int week, int classIndex)
        {
            if(urlParts.Length > 2)
                return urlParts[0] + week.ToString("D2") + urlParts[1] + classIndex.ToString("D5") + urlParts[2];
            return string.Empty;
        }
    }
}
