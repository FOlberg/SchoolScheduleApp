using System;
using System.Globalization;

namespace Helper.Header
{
    public enum Semester { winter_semester, summer_semester };
    public enum Hours {first, second, third, fourth, fifth, sixth, MP, seventh, eighth, ninth, tenth};
    public enum Days {Montag, Dienstag, Mittwoch, Donnerstag, Freitag };

    public class TimeHandler
    {
        DateTimeFormatInfo dfi;
        public DateTime today { get; set; }
        Calendar cal;
        public static readonly string[] HourName = { "1.", "2.", "3.", "4.", "5.", "6.", "MP", "7.", "8.", "9.", "10." };
        public static readonly string[] HourIndex = { "1", "2", "3", "4", "5", "6", "MP", "7", "8", "9", "10" };

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
        
        public static int GetCurrentWeek()
        {
            var calendar = DateTimeFormatInfo.CurrentInfo.Calendar;
            return calendar.GetWeekOfYear(System.DateTime.UtcNow, DateTimeFormatInfo.CurrentInfo.CalendarWeekRule, DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek);
        }

        public int GetWeekOfYear()
        {
            return cal.GetWeekOfYear(System.DateTime.UtcNow, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
        }

        public int GetNextWeek()
        {
            return cal.GetWeekOfYear(System.DateTime.UtcNow.AddDays(7), dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
        }

        public int GetNextWeek(bool early)
        {
            return cal.GetWeekOfYear(System.DateTime.UtcNow.AddDays(14), dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
        }

        public int GetWeekIndex(int week)
        {
            if (week == 0) return GetWeekOfYear();
            if (week == 1) return GetNextWeek();
            return GetNextWeek(true);
        }

        public static void ChangeToMonday(ref DateTime d)
        {
            while(d.Date.DayOfWeek != DayOfWeek.Monday)
            {
                d = d.AddDays(-1);
            }
        }

        public static DateTime GetMonday(int week)
        {
            DateTime t = DateTime.Now.Date;
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
