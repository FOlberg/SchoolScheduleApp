using System.Net;
using System.Collections.Generic;

namespace AppTestProzesse.Header
{
    public class URLClient
    {
        private WebClient webClient;

        public static string[] classes;

        public URLClient()
        {
            webClient = new WebClient();
        }
        ~URLClient() { }

        public string GetRawCode(string url)
        {
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
            return source;
        }

        public string GetRawCode(string[] urlParts, int week, int classIndex)
        {
            string url = LinkURL(urlParts, week, classIndex + 1);
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
            return source;
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
            //Header.Time t = new Header.Time();
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
            string[][] arr = { dNames.ToArray(), dSource.ToArray() };
            return arr;
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
