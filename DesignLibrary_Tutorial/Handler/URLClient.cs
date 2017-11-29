using System.Net;
using System.Web;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace Helper.Header
{
    public class URLClient
    {
        private WebClient webClient;
        [JsonProperty]
        public int early = 0;
        public static string[] classes;

        public URLClient()
        {
            webClient = new WebClient();
            webClient.Encoding = Encoding.GetEncoding(1252);
        }

        public string GetRawCode(string url)
        {
            webClient.Encoding = Encoding.GetEncoding(1252);
            if (url.Length == 0)
                return null;
            string source;
            try
            {
                source = webClient.DownloadString(url);
            }
            catch (System.Net.WebException e)
            {
                if (e.Message.Contains("Remote"))
                {
                    //pop-up
                }
                return null;
            }
            return source;//.HttpUtility.HtmlDecode(source);
        }

        public string GetRawCode(string[] urlParts, int week, int classIndex)
        {
            CheckEarly(urlParts);
            string url = LinkURL(urlParts, week + early, classIndex + 1);
            if (url.Length == 0)
                return null;
            string source = GetRawCode(url);
            if (source == null)
            {
                early = 1;
            }
            return source;
        }

        public string GetRawCode(string[] urlParts, int week)
        {
            CheckEarly(urlParts);
            string url = LinkURL(urlParts, week + early);
            if (url.Length == 0)
                return null;
            string source = GetRawCode(url);
            if (source == null)
            {
                early = 1;
            }
            return source;
        }

        private void CheckEarly(string[] urlParts)
        {
            if (early == 1)
            {
                if (GetRawCode(LinkURL(urlParts, TimeHandler.GetCurrentWeek(), 1)) != null)
                {
                    early = 0;
                }
            }
        }

        public static string GetRawCodeFile( string path )
        {
            return System.IO.File.ReadAllText(path);
        }

        public static string GetClass(string source)
        {
            string[] s = source.Split('\n');
            try
            {
                return s[15].Substring(3, s[15].Substring(3).IndexOf('<'));
            }catch
            {
                return "";
            }
        }


        public string[][] GetAllClasses(int week) //Unfinished
        {
            List<string> dNames = new List<string>();
            List<string> dSource = new List<string>();
            string p1 = "https://iserv.thg-goettingen.de/idesk/plan/public.php/Sch%C3%BCler-Vertretungsplan/e1fca97ce9638341/";
            int errc = 0;
            for(int i = 1; errc < 2; i++)
            {
                string source = GetRawCode(p1 + week + "/c/c" + i.ToString("D5") + ".htm"); //t.GetWeekOfYear()
                if (source != null)
                {
                    errc = 0;
                    dNames.Add(GetClass(source));
                    dSource.Add(source);
                }
                else errc++;
            }
            if(dNames.Count > 0)
            {
                return new string[][] { dNames.ToArray(), dSource.ToArray() };
            }
            return null;
        }

        public string LinkURL(string[] urlParts, int week)
        {
            if (urlParts.Length == 2)
            {
                return urlParts[0] + week + urlParts[1];
            }
            return "";
        }

        public string LinkURL(string[] urlParts, int week, int classIndex)
        {
            if(urlParts.Length > 2)
            {
                return urlParts[0] + week + urlParts[1] + classIndex.ToString("D5") + urlParts[2];
            }
            return "";
        }
    }
}
