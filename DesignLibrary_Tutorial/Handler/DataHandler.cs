using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using AppTestProzesse.Header;
using Android.App;
using Android.Content;
using System.Threading.Tasks;

namespace AppTestProzesse.Header
{
    public class DataHandler
    {
        public Config mConfig;
        string[,] mSource;
        string[] urlpartsA = new string[] { "https://iserv.thg-goettingen.de/idesk/plan/public.php/Sch%C3%BCler-Vertretungsplan/e1fca97ce9638341/", "/c/c", ".htm" };
        string[] urlpartsB = new string[] { "https://iserv.thg-goettingen.de/idesk/plan/public.php/Sch%C3%BCler-Vertretungsplan/e1fca97ce9638341/", "/w/w00000.htm" };

        [JsonProperty]
        string[] mClassNames;
        [JsonProperty]
        URLClient mClientURL; //Handler ? Save? -> What is faster initializing or deserializing
        TimeHandler mTimeHandler; // -""-
        JsonHandler mJsonHandler; // -""-
        InformationHandler mInfoHandler; // -""-
        [JsonProperty]
        Week[,] mWeekStack;
        [JsonProperty]
        List<Timetable> mTimetables;

        public DataHandler()
        {
            mClientURL = new URLClient();
            mTimeHandler = new TimeHandler();
            mJsonHandler = new JsonHandler();
            mInfoHandler = new InformationHandler();
            mTimetables = new List<Timetable>();
            LoadCfg();
            GetClassData();
        }

        /// <summary>
        /// Week: 0 = current Week; 1 = next Week
        /// </summary>
        /// <param name="classNameIndex"></param>
        /// <param name="week"></param>
        /// <returns></returns>
        public Week GetWeek(int classNameIndex, int week, bool newDload = false)
        {
            //Maybe Dictionary check depending on update peroid in settings
            DateTime mondayDate = mTimeHandler.GetMonday(week);
            if (mWeekStack == null)
            {
                mWeekStack = new Week[mClassNames.Length, 2];
            }
            if (mWeekStack[classNameIndex, week] != null && mWeekStack[classNameIndex, week].tMon.Date == mondayDate.Date && !newDload)
            {
                return mWeekStack[classNameIndex, week];
            }

            if (mSource == null)
            {
                mSource = new string[mClassNames.Length, 2];
            }
            if (mSource[classNameIndex, week] == null || newDload)
            {
                mSource[classNameIndex, week] = mClientURL.GetRawCode(urlpartsA, mTimeHandler.GetWeekIndex(week), classNameIndex);
                if (mSource[classNameIndex, week] == null)
                {
                    mSource[classNameIndex, week] = mClientURL.GetRawCode(urlpartsA, mTimeHandler.GetWeekIndex(week), classNameIndex);
                }
            }
            Week w = mInfoHandler.ClassSourceToWeek(mSource[classNameIndex, week], mClassNames[classNameIndex], mondayDate);
            mWeekStack[classNameIndex, week] = w;
            return w;
        }

        public Task DeleteOutdatedDataAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                if (mWeekStack != null)
                {
                    DateTime[] date = new DateTime[] { mTimeHandler.GetMonday(0), mTimeHandler.GetMonday(1) };
                    for (int i = 0; i < mWeekStack.Length; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            if (mWeekStack[i, j] != null)
                            {
                                if (mWeekStack[i, j].tMon.Date != date[j].Date)
                                {
                                    mWeekStack[i, j] = null;
                                }
                            }
                        }
                    }
                }
            });
        }

        public Week GetDetailedWeek(int week, bool newDload = false)
        {
            Week w = GetWeek(GetClassIndex(mConfig.GetClassName()), week, newDload);
            string source = mClientURL.GetRawCode(urlpartsB, mTimeHandler.GetWeekIndex(week));
            mInfoHandler.ApplyChanges(w, mInfoHandler.GetDetailedInfo(source));
            return w;
        }

        private int GetClassIndex(string className)
        {
            for (int i = 0; i < mClassNames.Length; i++)
            {
                if (mClassNames[i] == className)
                {
                    return i;
                }
            }
            //log
            return -1;
        }

        public Timetable GetTimetable(int classIndex)
        {
            if (mTimetables.Count > 0) //Cache
            {
                foreach (var tTable in mTimetables)
                {
                    if (tTable.mClassIndex == mClassNames[classIndex] && tTable.sem == mTimeHandler.GetSemester())
                    {
                        return CheckTimetable(tTable, classIndex);
                    }
                }
            }
            if (mJsonHandler.FileExists("Data", "Timetable" + mClassNames[classIndex], "json")) //Saved Files
            {
                mTimetables.Add(CheckTimetable(mJsonHandler.GetTimetable("Data", "Timetable" + mClassNames[classIndex], "json", mTimeHandler.GetSemester()), classIndex));
                return mTimetables[mTimetables.Count - 1];
            }

            //or create new Timetable
            Timetable table = new Timetable(GetWeek(classIndex, 0), GetWeek(classIndex, 1), System.DateTime.UtcNow);
            mTimetables.Add(table);
            mJsonHandler.saveObjects<Timetable, Semester>(mTimeHandler.GetSemester(), table, "Data", "Timetable" + mClassNames[classIndex] + ".json");
            return table;
        }

        private Timetable CheckTimetable(Timetable table, int classIndex)
        {
            if (table == null)
                return null;
            if (!table.IsLeaking())
                return table;
            table.Update(GetWeek(classIndex, 1)); //Which week?
            mJsonHandler.saveObjects<Timetable, Semester>(mTimeHandler.GetSemester(), table, "Data", "Timetable" + mClassNames[classIndex] + ".json");
            return table;
        }

        public string[] GetClasses()
        {
            if (mClassNames == null || mClassNames.Length == 0) GetClassData();
            return mClassNames;
        }

        public string GetClassName(int index)
        {
            if (mSource == null)
                GetClassData();
            if (index < 0 || index >= mClassNames.Length)
                return null;
            return mClassNames[index];
        }

        private void GetClassData()
        {
            //Loads or get Data by WebClient/Json
            if (JsonHandler.countFiles("Data", "classes", "json") > 0)
            {
                string[] temp = mJsonHandler.GetObjects<string[], Semester>("Data", "classes", "json", out Semester sem);
                if (sem == mTimeHandler.GetSemester())
                {
                    mClassNames = temp;
                }
                else DownloadClassInfromation();
            }
            else DownloadClassInfromation();
        }

        private void DownloadClassInfromation()
        {
            var source = mClientURL.GetAllClasses(mTimeHandler.GetWeekOfYear());
            if (source == null)
            {
                mClientURL.early = 1;
                source = mClientURL.GetAllClasses(mTimeHandler.GetNextWeek());  
            }
            if (source == null)
            {
                source = mClientURL.GetAllClasses(mTimeHandler.GetNextWeek() + 1);
            }

            if (source != null)
            {
                //Save the Source to classes.json
                mSource = new string[source[1].Length, 2];
                for (int i = 0; i < source[1].Length; i++)
                {
                    mSource[i, 0] = source[1][i];
                }
                mClassNames = source[0];
                mJsonHandler.saveObjects<string[], Semester>(mTimeHandler.GetSemester(), mClassNames, "Data", "classes.json");

            }
            else
                //log
                throw new Exception("TimeException: At the moment is no information online!");
        }

        public void LoadCfg()
        {
            if (mJsonHandler.FileExists("Data", "Config", "json"))
            {
                mConfig = mJsonHandler.GetObject<Config>("Data", "Config", "json");
            }
            else if (mConfig == null)
            {
                mConfig = new Config();
            }
            mConfig.OnConfigChanged += MConfig_OnConfigChanged;
        }

        private void MConfig_OnConfigChanged(object sender, EventArgs e)
        {
            mJsonHandler.saveObject<Config>(mConfig, "Data", "Config.json");
        }

        public static DataHandler GetDataHandler()
        {
            string HandlerSource = Application.Context.GetSharedPreferences("DataHandler", FileCreationMode.Private).GetString("mData", null);
            if (HandlerSource != null)
            {
                return JsonConvert.DeserializeObject<DataHandler>(HandlerSource);
            }
            else throw new System.Exception("DataHandler NullException");
        }
    }
}
