using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace AppTestProzesse.Header
{
    public class cTimetable
    {
        public string mClassName { get; set; }
        public int[][] mTimetable { get; set; }

        public cTimetable() { }
        public cTimetable(string className)
        {
            mClassName = className;
            mTimetable = new int[5][];
        }

        public cTimetable(string className, int[][] timetable)
        {
            mClassName = className;
            mTimetable = timetable;
        }

        public cTimetable(string className, int day, int[] selectTable)
        {
            mClassName = className;
            mTimetable = new int[5][];
            mTimetable[day] = selectTable;
        }
    }

    public class Config
    {
        [JsonProperty]
        List<cTimetable> mCfgList;
        public int mConfigSel;
        public int updateSequence;
        public bool vibration;

        public string[] urlA = new string[] { "https://iserv.thg-goettingen.de/idesk/plan/public.php/Sch%C3%BCler-Vertretungsplan/e1fca97ce9638341/", "/c/c", ".htm" };
        public string[] urlB = new string[] { "https://iserv.thg-goettingen.de/idesk/plan/public.php/Sch%C3%BCler-Vertretungsplan/e1fca97ce9638341/", "/w/w00000.htm" };

        public event EventHandler OnConfigChanged;

        public Config()
        {
            mCfgList = new List<cTimetable>();
            updateSequence = 120;
            vibration = false;
        }

        /* Temp*/
        public List<cTimetable> GetMList()
        {
            return mCfgList;
        }

        public void SaveList(List<cTimetable> list)
        {
            mCfgList = list;
        }
        /* Test END*/

        public void AddTableConf(string className, int[][] table)
        {
            if (mCfgList.Count <= mConfigSel) //Add new 
            {
                mCfgList.Add(new cTimetable(className, table));
                mConfigSel = mCfgList.Count - 1;
            }
            else if (mCfgList.Count > 0 && mCfgList[mConfigSel].mClassName == className)
            {
                mCfgList[mConfigSel].mTimetable = table;
            }
            else
            {
                mCfgList.Add(new cTimetable(className, table));
                mConfigSel = mCfgList.Count - 1;
            }
            OnConfigChanged(this, new EventArgs());
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
    }
}
