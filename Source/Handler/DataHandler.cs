using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Android.App;
using Android.Content;
using System.Threading.Tasks;
using ScheduleApp.Objects;

namespace ScheduleApp.Handler
{
    public class DataHandler
    {
        string[,]           mSource;

        [JsonProperty]
        string[]            mClassNames;
        [JsonProperty]
        URLClient           mClientURL;
        TimeHandler         mTimeHandler; // -""-
        InformationHandler  mInfoHandler; // -""-
        [JsonProperty]
        Week[,]             mWeekStack;
        [JsonProperty]
        List<Timetable>     mTimetables;

        public DataHandler()
        {
            mClientURL      = new URLClient();
            mTimeHandler    = new TimeHandler();
            mInfoHandler    = new InformationHandler();
            mTimetables     = new List<Timetable>();
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
            DateTime mondayDate = TimeHandler.GetMonday(week);
            var config  = GetConfig();
            if (classNameIndex == -1) //log
                return null;

            if (mWeekStack == null)
                mWeekStack = new Week[mClassNames.Length, 2];

            if (mWeekStack[classNameIndex, week] != null && mWeekStack[classNameIndex, week].mMonDate.Date == mondayDate.Date && !newDload)
                return mWeekStack[classNameIndex, week];

            if (mSource == null)
                mSource = new string[mClassNames.Length, 2];

            if (mSource[classNameIndex, week] == null || newDload)
            {
                mSource[classNameIndex, week] = mClientURL.GetRawCode(config.urlSourceClass, mTimeHandler.GetWeekIndex(week), classNameIndex);
                if (mSource[classNameIndex, week] == null)
                    mSource[classNameIndex, week] = mClientURL.GetRawCode(config.urlSourceClass, mTimeHandler.GetWeekIndex(week), classNameIndex);
            }
            if (mClientURL.mEarlyWeek == 1)
                mondayDate = mondayDate.AddDays(7);

            Week w = mInfoHandler.ClassSourceToWeek(mSource[classNameIndex, week], mClassNames[classNameIndex], mondayDate);
            mWeekStack[classNameIndex, week] = w;
            return w;
        }

        public Task DeleteOutdatedDataAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                if (mWeekStack == null)
                    return;

                DateTime[] date = new DateTime[] { TimeHandler.GetMonday(0), TimeHandler.GetMonday(1) };
                for (int i = 0; i < mWeekStack.Length; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        if (mWeekStack[i, j] == null)
                            continue;

                        if (mWeekStack[i, j].mMonDate.Date != date[j].Date)
                            mWeekStack[i, j] = null;
                    }
                }
                SaveState();
            }); 
        }

        public Week GetDetailedWeek(int week, bool newDload = false)
        {
            var config      = GetConfig();
            Week w          = GetWeek(GetClassIndex(config.GetClassName()), week, newDload);
            string source   = mClientURL.GetRawCode(config.urlSourceAll, mTimeHandler.GetWeekIndex(week));
            mInfoHandler.ApplyChanges(w, mInfoHandler.GetDetailedInfo(source));
            return w;
        }

        public Week GetDetailedWeek(int className, int week)
        {
            Week w  = GetWeek(className, week, true);
            string detailedSource = mClientURL.GetRawCode(GetConfig().urlSourceAll, mTimeHandler.GetWeekIndex(week));
            mInfoHandler.ApplyChanges(w, mInfoHandler.GetDetailedInfo(detailedSource));
            return w;
        }

        private int GetClassIndex(string className)
        {
            for (int i = 0; i < mClassNames.Length; i++)
            {
                if (mClassNames[i] == className)
                    return i;
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
                    if (tTable.mClassIndex == mClassNames[classIndex] && tTable.mSem == mTimeHandler.GetSemester())
                    {
                        return CheckTimetable(tTable, classIndex);
                    }
                }
            }
            if (JsonHandler.FileExists("Data", "Timetable" + mClassNames[classIndex], "json")) //Saved Files
            {
                mTimetables.Add(CheckTimetable(JsonHandler.GetTimetable("Data", "Timetable" + mClassNames[classIndex], "json", mTimeHandler.GetSemester()), classIndex));
                return mTimetables[mTimetables.Count - 1];
            }

            //or create new Timetable
            Timetable table = new Timetable(GetWeek(classIndex, 0), GetWeek(classIndex, 1), System.DateTime.UtcNow);
            mTimetables.Add(table);
            JsonHandler.saveObjects<Timetable, Semester>(mTimeHandler.GetSemester(), table, "Data", "Timetable" + mClassNames[classIndex] + ".json");
            return table;
        }

        private Timetable CheckTimetable(Timetable table, int classIndex)
        {
            if (table == null)
                return null;

            if (!table.IsLeaking())
                return table;

            table.Update(GetWeek(classIndex, 1)); //Which week?
            JsonHandler.saveObjects<Timetable, Semester>(mTimeHandler.GetSemester(), table, "Data", "Timetable" + mClassNames[classIndex] + ".json");
            return table;
        }

        public string[] GetClasses()
        {
            if (mClassNames == null || mClassNames.Length == 0)
                GetClassData();
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
                string[] temp = JsonHandler.GetObjects<string[], Semester>("Data", "classes", "json", out Semester sem);
                if (sem == mTimeHandler.GetSemester())
                    mClassNames = temp;
                else DownloadClassInfromation();
            }
            else DownloadClassInfromation();
        }

        private void DownloadClassInfromation()
        {
            var url = GetConfig().urlSourceClass;
            var source = mClientURL.GetAllClasses(mTimeHandler.GetWeekOfYear(), url);
            if (source == null)
            {
                mClientURL.mEarlyWeek = 1;
                source = mClientURL.GetAllClasses(mTimeHandler.GetNextWeek(), url);  
            }
            if (source == null)
                source = mClientURL.GetAllClasses(mTimeHandler.GetNextWeek() + 1, url);

            if (source != null)
            {
                mSource = new string[source[1].Length, 2];
                for (int i = 0; i < source[1].Length; i++)
                {
                    mSource[i, 0] = source[1][i];
                }
                mClassNames = source[0];
                JsonHandler.saveObjects<string[], Semester>(mTimeHandler.GetSemester(), mClassNames, "Data", "classes.json");

            }
            else
                //log
                throw new Exception("TimeException: At the moment is no information online!");
        }

        public static Config GetConfig()
        {
            if (JsonHandler.FileExists("Data", "Config", "json"))
                return JsonHandler.GetObject<Config>("Data", "Config", "json");

            var config = new Config();
            SaveConfig(config);
            return config;
        }

        public static void SaveConfig(Config config)
        {
            JsonHandler.saveObject<Config>(config, "Data", "Config.json");
        }

        public static DataHandler GetDataHandler()
        {
            string HandlerSource = Application.Context.GetSharedPreferences("DataHandler", FileCreationMode.Private).GetString("mData", null);
            if (HandlerSource != null)
                return JsonConvert.DeserializeObject<DataHandler>(HandlerSource);

            var dataHandler = new DataHandler();
            var editor      = Application.Context.GetSharedPreferences("DataHandler", FileCreationMode.Private).Edit();
            editor.PutString("mData", JsonConvert.SerializeObject(dataHandler));
            editor.Apply();
            return dataHandler;
        }

        public static bool GetDarkThemePref(Activity activity)
        {
            return activity.GetSharedPreferences("Config", FileCreationMode.Private).GetBoolean("DarkTheme", false);
        }

        public void SaveState()
        {
            var editor = Application.Context.GetSharedPreferences("DataHandler", FileCreationMode.Private).Edit();
            editor.PutString("mData", JsonConvert.SerializeObject(this));
            editor.Apply();
        }

        public void DeleteCache()
        {
            mSource = null;
            mClassNames = null;
            mWeekStack = null;
            mTimetables.Clear();

            var config = GetConfig();
            config.RemoveAllConfigs();
            JsonHandler.DeleteFiles("Data");
            SaveConfig(config);
            SaveState();
        }
    }
}
