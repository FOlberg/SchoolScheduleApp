using System;
using System.Globalization;

namespace AppTestProzesse.Header
{
    public enum Semester { winter_semester, summer_semester };
    public enum Hours {first, second, third, fourth, fifth, sixth, MP, seventh, eighth, ninth, tenth};
    public enum Days {Montag, Dienstag, Mittwoch, Donnerstag, Freitag };

    public class TimeHandler
    {
        DateTimeFormatInfo dfi;
        public DateTime today { get; set; }
        Calendar cal;

        public TimeHandler()
        {
            dfi = DateTimeFormatInfo.CurrentInfo;
            today = System.DateTime.UtcNow;
            cal = dfi.Calendar;
        }
        public TimeHandler(DateTime date)
        {
            dfi = DateTimeFormatInfo.CurrentInfo;
            today = date;
            cal = dfi.Calendar;
        }
        ~TimeHandler() { }

        public void update()
        {
            today = System.DateTime.UtcNow;
        }

        public void SetTime(DateTime date)
        {
            today = date;
        }
        
        public int GetWeekOfYear()
        {
            return cal.GetWeekOfYear(today, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
        }

        public int GetNextWeek()
        {
            return cal.GetWeekOfYear(today.AddDays(7), dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
        }
        public int GetWeekIndex(int week)
        {
            if (week == 0) return GetWeekOfYear();
            return GetNextWeek();
        }

        public void ChangeToMonday(ref DateTime d)
        {
            while(d.DayOfWeek != DayOfWeek.Monday)
            {
                d.Subtract(TimeSpan.FromDays(1));
            }
        }

        public DateTime GetMonday(int week)
        {
            DateTime t = new DateTime();
            if (week == 1)
            {
                t = t.AddDays(7);        
            }
            else if(week != 0)
            {
                throw new Exception("Time: Week out of Range!");
            }
            ChangeToMonday(ref t);
            return t;
        }

        public Semester GetSemester()
        {
            if (System.DateTime.UtcNow.Month >= 8)
                return Semester.winter_semester;
            return Semester.summer_semester;
        }

        public Semester GetSemester(DateTime date)
        {
            if (date.Month >= 8)
                return Semester.winter_semester;
            return Semester.summer_semester;
        }
    }
}
