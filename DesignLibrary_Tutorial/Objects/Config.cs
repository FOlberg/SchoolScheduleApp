using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace ScheduleApp.Objects
{
    public class ConfigTimetable
    {
        public string   mClassName { get; set; }
        public int[][]  mTimetable { get; set; }

        public ConfigTimetable() { }
        public ConfigTimetable(string className)
        {
            mClassName = className;
            mTimetable = new int[5][];
        }

        public ConfigTimetable(string className, int[][] timetable)
        {
            mClassName = className;
            mTimetable = timetable;
        }

        public ConfigTimetable(string className, int day, int[] selectTable)
        {
            mClassName      = className;
            mTimetable      = new int[5][];
            mTimetable[day] = selectTable;
        }
    }

    public class Settings
    {
        public bool mVibration;
        public bool mPriority;
        public int  mUpdateSequence;

        public Settings()
        {
            mVibration      = false;
            mPriority       = false;
            mUpdateSequence = 120; 
        }
    }

    public class Config
    {
        [JsonProperty]
        List<ConfigTimetable>    mCfgList;
        public int          mConfigSel;
        public Settings     mSettings;
        public string[]     urlSourceClass, urlSourceAll;

        public Config()
        {
            mCfgList    = new List<ConfigTimetable>();
            mSettings   = new Settings();
            ResetSourceAll();
            ResetSourceClass();
        }

        public void AddTableConf(string className, int[][] table)
        {
            if (mCfgList.Count > 45)
            {
                //log
                throw new Exception("Configurtaion File Overflow");
            }
            if (mCfgList.Count <= mConfigSel) //Add new 
            {
                mCfgList.Add(new ConfigTimetable(className, table));
                mConfigSel = mCfgList.Count - 1;
            }
            else if (mCfgList.Count > 0 && mCfgList[mConfigSel].mClassName == className)
            {
                mCfgList[mConfigSel].mTimetable = table;
            }
            else
            {
                for (int i = 0; i < mCfgList.Count; i++)
                {
                    if (mCfgList[i].mClassName == className)
                    {
                        mCfgList[i].mTimetable = table;
                        mConfigSel = i;
                        return;
                    }
                }
                mCfgList.Add(new ConfigTimetable(className, table));
                mConfigSel = mCfgList.Count - 1;
            }
        }

        public int[][] GetTableConf()
        {
            if (mCfgList.Count <= mConfigSel) return null;
            return mCfgList[mConfigSel].mTimetable;
        }

        public int[][] GetTableConf(int configIndex)
        {
            if (mCfgList.Count <= configIndex) return null;
            return mCfgList[configIndex].mTimetable;
        }

        public string GetClassName()
        {
            if (mCfgList.Count <= mConfigSel) return null;
            return mCfgList[mConfigSel].mClassName;
        }

        public string GetClassName(int configIndex)
        {
            if (mCfgList.Count <= mConfigSel) return null;
            return mCfgList[configIndex].mClassName;
        }

        public bool IsEmpty()
        {
            return mCfgList.Count == 0;
        }

        public int GetConfigCount()
        {
            return mCfgList.Count;
        }

        public void ResetSourceClass()
        {
            urlSourceClass = new string[] { "https://iserv.thg-goettingen.de/idesk/plan/public.php/Sch%C3%BCler-Vertretungsplan/e1fca97ce9638341/", "/c/c", ".htm" };
        }

        public void ResetSourceAll()
        {
            urlSourceAll = new string[] { "https://iserv.thg-goettingen.de/idesk/plan/public.php/Sch%C3%BCler-Vertretungsplan/e1fca97ce9638341/", "/w/w00000.htm" };
        }
    }
}
