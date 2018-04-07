using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ScheduleApp.Objects
{
    public class Timetable
    {
        public Tuple<string, string>[,][]   mSubNames { get; set; } //simple Subject :: Item1 = room; Item2 = name
        public Semester                     mSem;
        public string                       mClassIndex = string.Empty;

        [JsonProperty]
        List<int[]> mLeaks = new List<int[]>();

        public Timetable() { }

        public Timetable(Week w1, Week w2, DateTime date) //NullException handling is missing
        {
            if (w1 == null && w2 == null)
                throw new Exception("Timetable: Both weeks are null");
            else if (w2 == null || w1 == null)
            {
                if (w2 == null)
                    InitTimetable(w1, null, date, false);
                else
                    InitTimetable(w2, null, date, false);
                //Log Message
            }
            else if (w1.mClass != w2.mClass)
                throw new Exception("Timetable: Classes don't match!");
            else
                InitTimetable(w1, w2, date, true);
        }

        private void InitTimetable(Week w1, Week w2, DateTime date, bool both)
        {
            //Variables
            mClassIndex = w1.mClass;
            //Adding Subjects
            mSubNames = new Tuple<string, string>[5, 11][];

            //Time
            if (date.Month >= 8)
                mSem = Semester.winter_semester;
            else
                mSem = Semester.summer_semester;

            for (int day = 0; day < 5; day++)
            {
                for (int hour = 0; hour < 11; hour++)
                {
                    List<Tuple<string, string>> tSubjects = new List<Tuple<string, string>>();

                    if (w1.mWeek[day].mSubs[hour] != null && w1.mWeek[day].mSubs[hour][0].mEvent == null)
                    {
                        for (int sPosition = 0; sPosition < w1.mWeek[day].mSubs[hour].Length; sPosition++) //condition? maybe (<=) than only < //NEEDS CHECK 
                        {
                            if (w1.mWeek[day].mSubs[hour][sPosition].mEvent == null)
                                tSubjects.Add(new Tuple<string, string>(w1.mWeek[day].mSubs[hour][sPosition].mRoom, w1.mWeek[day].mSubs[hour][sPosition].mName));
                        }
                    }
                    else if (both && w2.mWeek[day].mSubs[hour] != null && w2.mWeek[day].mSubs[hour][0].mEvent == null) //same  here
                    {
                        for (int sPos = 0; sPos < w2.mWeek[day].mSubs[hour].Length; sPos++)
                        {
                            if (w2.mWeek[day].mSubs[hour][sPos].mEvent == null)
                                tSubjects.Add(new Tuple<string, string>(w2.mWeek[day].mSubs[hour][sPos].mRoom, w2.mWeek[day].mSubs[hour][sPos].mName));
                            else //leaking information beacause there are still events going on
                                mLeaks.Add(new int[] { day, hour, -1 });
                        }
                    }
                    else
                    {
                        foreach (var ev in w1.mEvents)
                        {
                            if (ev.mDay == (Days)day && ev.mHour <= (Hours)hour && ev.mNumber >= (Hours)hour)
                            {
                                mLeaks.Add(new int[] { day, hour, -1 });
                                break;
                            }
                        }
                    }
                    //Add array of subjects to 
                    mSubNames[day, hour] = tSubjects.ToArray();
                }
            }
        }

        public void Update(Week w)
        {
            int day = 0, hour = 0;
            if (!IsLeaking() || w == null)
                return;

            for (int i = mLeaks.Count - 1; i >= 0; i--)
            {
                day     = mLeaks[i][0];
                hour    = mLeaks[i][1];
                if (mLeaks[i][2] == -1 && w.mWeek[day].mSubs[hour] != null && w.mWeek[day].mSubs[hour][0].mEvent == null)
                {
                    List<Tuple<string, string>> tList = new List<Tuple<string, string>>();
                    for (int pos = 0; pos < w.mWeek[day].mSubs[hour].Length; pos++)
                    {
                        tList.Add(new Tuple<string, string>(w.mWeek[day].mSubs[hour][pos].mRoom, w.mWeek[day].mSubs[hour][pos].mName));
                        //list[day, hour][pos] = new Tuple<string, string>(w.week[day].list[hour][pos].room, w.week[day].list[hour][pos].name);
                    }
                    if (tList.Count > 0)
                    {
                        mSubNames[day, hour] = tList.ToArray();
                        mLeaks.RemoveAt(i);
                    }
                }
                else if (hour == (int)Hours.MP)
                {
                    bool isEvent = false;
                    foreach (var ev in w.mEvents)
                    {
                        if (ev.mDay == (Days)day && ev.mHour <= Hours.MP && ev.mNumber >= Hours.MP)
                        {
                            isEvent = true;
                            break;
                        }
                    }
                    if (!isEvent)
                        mLeaks.RemoveAt(i);
                }
            }
        }

        public bool IsLeaking()
        {
            return mLeaks.Count > 0;
        }
    }
}