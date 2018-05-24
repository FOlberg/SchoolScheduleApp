using System;
using System.Collections.Generic;

namespace ScheduleApp.Objects
{
    public class Week
    {
        public DateTime     mMonDate; //Time of Monday in that Week
        public string       mClass { get; set; }
        public Day[]        mWeek = new Day[5];
        public List<Event>  mEvents { get; }

        public Week() { }

        public Week(DateTime monday, string className)
        {
            mMonDate    = monday;
            mClass      = className;
            mEvents     = new List<Event>();
            for (int i = 0; i < 5; i++)
            {
                mWeek[i] = new Day((Days)i);
            }
        }

        public void AddEvent(Event ev)
        {
            mEvents.Add(ev);
        }

        public void AddLesson(Days d, Hours h, Subject[] arr)
        {
            if (mEvents.Count > 0) //Exception for events with a description greater than 1 lines 
            {
                foreach (var ev in mEvents)
                {
                    if (ev.mDay == d && ev.mHour <= h && ev.mNumber >= h)
                    {
                        foreach (var subj in arr)
                        {
                            subj.mEvent = ev;
                            ev.mDescribtion += subj.mName + " ";
                        }
                        mWeek[(int)d].AddLesson(h, arr);
                        return;
                    }
                }
            }
            mWeek[(int)d].AddLesson(h, arr);
        }

        public void LinkEventsToSubjects() //Not necessary anymore!
        {
            if (mEvents.Count == 0)
                return;
            //Exception for events with a description greater than 1 lines 

            foreach (var ev in mEvents)
            {
                for (Hours h = ev.mHour; h < ev.mNumber; h++)
                {
                    if (mWeek[(int)ev.mDay].GetSubjectList(h) == null)
                        continue;

                    foreach (var subj in mWeek[(int)ev.mDay].GetSubjectList(h))
                    {
                        subj.mEvent = ev;
                        ev.mDescribtion += subj.mName + " ";
                    }
                }
            }
        }
    }
}