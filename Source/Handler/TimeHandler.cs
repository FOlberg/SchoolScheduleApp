using ScheduleApp.Objects;
using System;
using System.Globalization;

namespace ScheduleApp.Handler
{

    public class TimeHandler
    {
        DateTimeFormatInfo  mDfi;
        Calendar            mCal;
        public DateTime     mToday { get; set; }
        
        public static readonly string[] HourName = { "1.", "2.", "3.", "4.", "5.", "6.", "MP", "7.", "8.", "9.", "10." };
        public static readonly string[] HourIndex = { "1", "2", "3", "4", "5", "6", "MP", "7", "8", "9", "10" };

        public TimeHandler()
        {
            mDfi    = DateTimeFormatInfo.CurrentInfo;
            mToday  = System.DateTime.UtcNow;
            mCal    = mDfi.Calendar;
        }
        public TimeHandler(DateTime date)
        {
            mDfi    = DateTimeFormatInfo.CurrentInfo;
            mToday  = date;
            mCal    = mDfi.Calendar;
        }

        public void update()
        {
            mToday = System.DateTime.UtcNow;
        }

        public void SetTime(DateTime date)
        {
            mToday = date;
        }
        
        public static int GetCurrentWeek()
        {
            var calendar = DateTimeFormatInfo.CurrentInfo.Calendar;
            return calendar.GetWeekOfYear(System.DateTime.UtcNow, DateTimeFormatInfo.CurrentInfo.CalendarWeekRule, DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek);
        }

        public int GetWeekOfYear()
        {
            return mCal.GetWeekOfYear(System.DateTime.UtcNow, mDfi.CalendarWeekRule, mDfi.FirstDayOfWeek);
        }

        public int GetNextWeek()
        {
            return mCal.GetWeekOfYear(System.DateTime.UtcNow.AddDays(7), mDfi.CalendarWeekRule, mDfi.FirstDayOfWeek);
        }

        public int GetNextWeek(bool early)
        {
            return mCal.GetWeekOfYear(System.DateTime.UtcNow.AddDays(14), mDfi.CalendarWeekRule, mDfi.FirstDayOfWeek);
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
                t = t.AddDays(7);        
            else if(week != 0)
                throw new Exception("Time: Week out of Range!");
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
