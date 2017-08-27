using Newtonsoft.Json;
using System.IO;

namespace AppTestProzesse.Header
{
    public class JsonHandler
    {
        public static string codepath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        JsonSerializer serializer;
        static int count;

        public JsonHandler()
        {
            codepath = System.Reflection.Assembly.GetExecutingAssembly().Location;//System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            codepath = System.IO.Path.GetDirectoryName(codepath);
            serializer = new JsonSerializer();
        }

        public static int countFiles(string folder, string filename, string format)
        {
            if(folder.Length > 0) folder = "/" + folder;
            if(Directory.Exists(codepath + folder))
            {
                count = Directory.GetFiles(codepath + folder, filename + "*." + format, SearchOption.TopDirectoryOnly).Length;
                return count;
            }
            return -1;
        }


        public void saveObject<T>(T obj, string folder, string filename)
        {
            CheckFolder(folder);
            if (folder.Length > 0) folder = "/" + folder;
            File.WriteAllText(codepath + folder + "/" + filename, JsonConvert.SerializeObject(obj));
        }

        public void saveObjects<T>(string[] info, T obj, string folder, string filename)
        {
            CheckFolder(folder);
            if (folder.Length > 0) folder = "/" + folder;
            string source = JsonConvert.SerializeObject(info) + System.Environment.NewLine;
            source += JsonConvert.SerializeObject(obj);
            File.WriteAllText(codepath + folder + "/" + filename, source);
        }

        public void saveObjects<T, U>(U objTop, T obj, string folder, string filename)
        {
            CheckFolder(folder);
            if (folder.Length > 0) folder = "/" + folder;
            string source = JsonConvert.SerializeObject(objTop) + System.Environment.NewLine;
            source += JsonConvert.SerializeObject(obj);
            File.WriteAllText(codepath + folder + "/" + filename, source);
        }

        public T GetObject<T>(string folder, string filename, string format)
        {
            if (folder.Length > 0) folder = "/" + folder;
            if (File.Exists(codepath + folder + "/" + filename + "." + format))
            {
                return JsonConvert.DeserializeObject<T>( File.ReadAllText(codepath + folder + "/" + filename + "." + format) );
            }
            return default(T);
        }

        public T GetObjects<T>(string folder, string filename, string format, out string[] fileInfo)
        {
            if (folder.Length > 0) folder = "/" + folder;
            if (File.Exists(codepath + folder + "/" + filename + "." + format))
            {
                string[] source = File.ReadAllText(codepath + folder + "/" + filename + "." + format).Split('\n');
                fileInfo = JsonConvert.DeserializeObject<string[]>(source[0]);
                return JsonConvert.DeserializeObject<T>(source[1]);
            }
            fileInfo = null;
            return default(T);
        }

        public T GetObjects<T, U>(string folder, string filename, string format, out U objTop)
        {
            if (folder.Length > 0) folder = "/" + folder;
            if (File.Exists(codepath + folder + "/" + filename + "." + format))
            {
                string[] source = File.ReadAllText(codepath + folder + "/" + filename + "." + format).Split('\n');
                objTop = JsonConvert.DeserializeObject<U>(source[0]);
                return JsonConvert.DeserializeObject<T>(source[1]);
            }
            objTop = default(U);
            return default(T);
        }

        public Timetable GetTimetable(string folder, string filename, string format, Semester sem)
        {
            if (folder.Length > 0) folder = "/" + folder;
            if (File.Exists(codepath + folder + "/" + filename + "." + format))
            {
                string[] source = File.ReadAllText(codepath + folder + "/" + filename + "." + format).Split('\n');
                if(JsonConvert.DeserializeObject<Semester>(source[0]) == sem)
                {
                    return JsonConvert.DeserializeObject<Timetable>(source[1]);
                }  
            }
            return null;
        }

        private void CheckFolder(string name)
        {
            if(!Directory.Exists(codepath + "/" + name))
            {
                Directory.CreateDirectory(codepath + "/" + name);
            }
        }

        public bool FileExists(string folder, string filename, string format)
        {
            try
            {
                return File.Exists(codepath + "/" + folder + "/" + filename + "." + format);
            }
            catch (FileNotFoundException) { } // Add to Log
            return false;
        }
    }
}
