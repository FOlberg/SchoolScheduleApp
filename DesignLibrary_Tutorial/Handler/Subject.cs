using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Helper.Header
{
    public class Event
    {
        public Days Day { get; set; }
        public Hours Hour { get; set; }
        public Hours Number { get; set; }
        public string Describtion { get; set; }

        public Event() { }

        public Event(Days d, Hours h, Hours n, string de = "")
        {
            Day = d;
            Hour = h;
            Number = n;
            Describtion = de;
        }
    }

    public class SubChange
    {
        public string type { get; set; }
        public string newSubject { get; set; }
        public string newRoom { get; set; }
        public string remarks { get; set; }
        public Tuple<string, string> transfer { get; set; }

        public SubChange() { }

        public SubChange(string type, string newSubject, string newRoom, string remarks = "")
        {
            this.type = type;
            this.newSubject = newSubject;
            this.newRoom = newRoom;
            this.remarks = remarks;
        }
        public SubChange(string type, string newSubject, string newRoom, Tuple<string, string> transfer, string remarks = "")
        {
            this.type = type;
            this.newSubject = newSubject;
            this.newRoom = newRoom;
            this.transfer = transfer;
            this.remarks = remarks;
        }
    }

    public class Subject
    {
        public string name { get; set; }
        public string room { get; set; }
        public bool omitted { get; set; }
        public Event ev { get; set; }
        public SubChange change { get; set; }
        public Subject() { }

        public Subject(string name, string room, bool omitted = false)
        {
            this.name = name;
            this.room = room;
            this.omitted = omitted;
            change = null;
        }
        public Subject(string name, string room, SubChange change, bool omitted = false)
        {
            this.name = name;
            this.room = room;
            this.omitted = omitted;
            this.change = change;
        }
        public Subject(Event ev, bool omitted = false)
        {
            this.ev = ev;
            this.omitted = omitted;
        }
    }

    public class Day
    {
        Days mDay;
        public Subject[][] list = new Subject[11][];

        public Day() { }

        public Day(Days d)
        {
            mDay = d;
        }

        public void AddLesson(Hours h, Subject[] arr)
        {
            list[(int)h] = arr;
        }

        public Subject[] GetSubjectList(Hours h)
        {
            return list[(int)h];
        }
    }

    public class Week
    {
        public DateTime tMon; //Time of Monday in that Week
        public string mClass { get; set; }
        public Day[] week = new Day[5];
        public List<Event> mEvents { get; }

        public Week() { }

        public Week(DateTime monday, string className)
        {
            tMon = monday;
            mClass = className;
            mEvents = new List<Event>();
            for (int i = 0; i < 5; i++)
            {
                week[i] = new Day((Days)i);
            }
        }

        public void AddEvent(Event ev)
        {
            mEvents.Add(ev);
        }

        public void AddLesson(Days d, Hours h, Subject[] arr)
        {
            bool added = false;
            if (mEvents.Count > 0) //Exception for events with a description greater than 1 lines 
            {
                foreach (var ev in mEvents)
                {
                    if (ev.Day == d && ev.Hour <= h && ev.Number >= h)
                    {
                        foreach (var subj in arr)
                        {
                            subj.ev = ev;
                            ev.Describtion += subj.name + " ";
                        }
                        week[(int)d].AddLesson(h, arr);
                        added = true;
                        break;
                    }
                }
            }
            if (!added) week[(int)d].AddLesson(h, arr);
        }

        public void LinkEventsToSubjects() //Not necessary anymore!
        {
            if (mEvents.Count > 0) //Exception for events with a description greater than 1 lines 
            {
                foreach (var ev in mEvents)
                {
                    for (Hours h = ev.Hour; h < ev.Number; h++)
                    {
                        if (week[(int)ev.Day].GetSubjectList(h) != null)
                        {
                            foreach (var subj in week[(int)ev.Day].GetSubjectList(h))
                            {
                                subj.ev = ev;
                                ev.Describtion += subj.name + " ";
                            }
                        }
                    }
                }
            }
        }
    }

    public class Timetable
    {
        public Tuple<string, string>[,][] list { get; set; } //simple Subject :: Item1 = room; Item2 = name
        public Semester sem;
        public string mClassIndex = "";

        [JsonProperty]
        List<int[]> leaks = new List<int[]>();

        public Timetable() { }

        public Timetable(Week w1, Week w2, DateTime date) //NullException handling is missing
        {
            if (w1 == null && w2 == null)
                throw new Exception("Timetable: Both weeks are null");
            else if (w2 == null || w1 == null)
            {
                if (w2 == null) InitTimetable(w1, null, date, false);
                else InitTimetable(w2, null, date, false);
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

            //Time
            if (date.Month >= 8) sem = Semester.winter_semester;
            else sem = Semester.summer_semester;

            //Adding Subjects
            list = new Tuple<string, string>[5, 11][];

            for (int day = 0; day < 5; day++)
            {
                for (int hour = 0; hour < 11; hour++)
                {
                    List<Tuple<string, string>> tSubjects = new List<Tuple<string, string>>();

                    if (w1.week[day].list[hour] != null && w1.week[day].list[hour][0].ev == null)
                    {
                        for (int sPosition = 0; sPosition < w1.week[day].list[hour].Length; sPosition++) //condition? maybe (<=) than only < //NEEDS CHECK 
                        {
                            if (w1.week[day].list[hour][sPosition].ev == null)
                            {
                                tSubjects.Add(new Tuple<string, string>(w1.week[day].list[hour][sPosition].room, w1.week[day].list[hour][sPosition].name));
                            }
                        }
                    }
                    else if (both && w2.week[day].list[hour] != null && w2.week[day].list[hour][0].ev == null) //same  here
                    {
                        for (int sPos = 0; sPos < w2.week[day].list[hour].Length; sPos++)
                        {
                            if (w2.week[day].list[hour][sPos].ev == null)
                            {
                                tSubjects.Add(new Tuple<string, string>(w2.week[day].list[hour][sPos].room, w2.week[day].list[hour][sPos].name));
                            }
                            else //leaking information beacause there are still events going on
                            {
                                leaks.Add(new int[] { day, hour, -1 });
                            }
                        }
                    }
                    else
                    {
                        foreach (var ev in w1.mEvents)
                        {
                            if (ev.Day == (Days)day && ev.Hour <= (Hours)hour && ev.Number >= (Hours)hour)
                            {
                                leaks.Add(new int[] { day, hour, -1 });
                                break;
                            }
                        }
                    }
                    //Add array of subjects to 
                    list[day, hour] = tSubjects.ToArray();
                }
            }
        }

        public void Update(Week w)
        {
            int day = 0, hour = 0;
            if (IsLeaking() && w != null)
            {
                for (int i = leaks.Count - 1; i >= 0; i--)
                {
                    day = leaks[i][0];
                    hour = leaks[i][1];
                    if (w.week[day].list[hour] != null)
                    {
                        if (leaks[i][2] == -1)
                        {
                            List<Tuple<string, string>> tList = new List<Tuple<string, string>>();
                            for (int pos = 0; pos < w.week[day].list[hour].Length; pos++)
                            {
                                tList.Add(new Tuple<string, string>(w.week[day].list[hour][pos].room, w.week[day].list[hour][pos].name));
                                //list[day, hour][pos] = new Tuple<string, string>(w.week[day].list[hour][pos].room, w.week[day].list[hour][pos].name);
                            }
                            if (tList.Count > 0)
                            {
                                list[day, hour] = tList.ToArray();
                                //remove
                                leaks.RemoveAt(i);
                            }
                        }
                    }
                    else if (hour == (int)Hours.MP)
                    {
                        bool isEvent = false;
                        foreach (var ev in w.mEvents)
                        {
                            if (ev.Day == (Days)day && ev.Hour <= (Hours)hour && ev.Number >= (Hours)hour)
                            {
                                isEvent = true;
                            }
                        }
                        if (!isEvent)
                        {
                            leaks.RemoveAt(i);
                        }
                    }
                }
            }
        }

        public bool IsLeaking()
        {
            return leaks.Count > 0;
        }
    }
}
