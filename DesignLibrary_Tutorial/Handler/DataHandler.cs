using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using AppTestProzesse.Header;

namespace AppTestProzesse.Header
{
    public class DataHandler
    {
        public Config mConfig;

        [JsonProperty]
        string[,] mSource;
        string[] mClassNames;
        URLClient mClientURL;
        TimeHandler mTimeHandler;
        JsonHandler mJsonHandler;
        InformationHandler mInfoHandler;
        Dictionary<DateTime, Week> mWeekStack;
        List<Timetable> mTimetables;
        string[] urlpartsA = new string[] { "https://iserv.thg-goettingen.de/idesk/plan/public.php/Sch%C3%BCler-Vertretungsplan/e1fca97ce9638341/", "/c/c", ".htm" };

        public DataHandler()
        {
            mClientURL = new URLClient();
            mTimeHandler = new TimeHandler();
            mJsonHandler = new JsonHandler();
            mInfoHandler = new InformationHandler();
            mWeekStack = new Dictionary<DateTime, Week>();
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
        public Week GetWeek(int classNameIndex, int week)
        {
            //Maybe Dictionary check depending on update peroid in settings
            DateTime mondayDate = mTimeHandler.GetMonday(week);
            Week w;
            if (mSource == null) mSource = new string[mClassNames.Length, 2];
            if (mSource[classNameIndex, week] == null)
                mSource[classNameIndex, week] = mClientURL.GetRawCode(urlpartsA, mTimeHandler.GetWeekIndex(week), classNameIndex);
            w = mInfoHandler.ClassSourceToWeek(mSource[classNameIndex, week], mClassNames[classNameIndex], mondayDate);
            mWeekStack.Add(System.DateTime.UtcNow, w);
            return w;
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
            if (!table.isLeaking())
                return table;
            table.update(GetWeek(classIndex, 1)); //Which week?
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
                source = mClientURL.GetAllClasses(mTimeHandler.GetNextWeek());

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
                throw new Exception("TimeException: At the moment is no information online!");
        }

        private void LoadCfg()
        {
            if(mJsonHandler.FileExists("Data", "Config", "json"))
            {
                mConfig = mJsonHandler.GetObject<Config>("Data", "Config", "json");
            }
            else if(mConfig == null)
            {
                mConfig = new Config();
            }
            mConfig.OnConfigChanged += MConfig_OnConfigChanged;
        }

        private void MConfig_OnConfigChanged(object sender, EventArgs e)
        {
            mJsonHandler.saveObject<Config>(mConfig, "Data", "Config.json");
        }
    }
}
