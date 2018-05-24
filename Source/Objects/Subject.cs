using System;

namespace ScheduleApp.Objects
{
    public enum Semester    { winter_semester, summer_semester };
    public enum Hours       { first, second, third, fourth, fifth, sixth, MP, seventh, eighth, ninth, tenth };
    public enum Days        { Montag, Dienstag, Mittwoch, Donnerstag, Freitag };

    public class Event
    {
        public Days     mDay { get; set; }
        public Hours    mHour { get; set; }
        public Hours    mNumber { get; set; }
        public string   mDescribtion { get; set; }

        public Event() { }

        public Event(Days d, Hours h, Hours n, string de = "")
        {
            mDay            = d;
            mHour           = h;
            mNumber         = n;
            mDescribtion    = de;
        }
    }

    public class SubChange
    {
        public string mType                     { get; set; }
        public string mNewSubject               { get; set; }
        public string mNewRoom                  { get; set; }
        public string mRemarks                  { get; set; }
        public Tuple<string, string> mTransfer  { get; set; }

        public SubChange() { }

        public SubChange(string type, string newSubject, string newRoom, string remarks = "")
        {
            this.mType          = type;
            this.mNewSubject    = newSubject;
            this.mNewRoom       = newRoom;
            this.mRemarks       = remarks;
        }
        public SubChange(string type, string newSubject, string newRoom, Tuple<string, string> transfer, string remarks = "")
        {
            this.mType          = type;
            this.mNewSubject    = newSubject;
            this.mNewRoom       = newRoom;
            this.mTransfer      = transfer;
            this.mRemarks       = remarks;
        }
    }

    public class Subject
    {
        public string   mName       { get; set; }
        public string   mRoom       { get; set; }
        public bool     mOmitted    { get; set; }
        public Event    mEvent      { get; set; }
        public SubChange mChange    { get; set; }

        public Subject() { }

        public Subject(string name, string room, bool omitted = false)
        {
            this.mName      = name;
            this.mRoom      = room;
            this.mOmitted   = omitted;
            this.mChange    = null;
        }
        public Subject(string name, string room, SubChange change, bool omitted = false)
        {
            this.mName      = name;
            this.mRoom      = room;
            this.mOmitted   = omitted;
            this.mChange    = change;
        }
        public Subject(Event ev, bool omitted = false)
        {
            this.mEvent     = ev;
            this.mOmitted   = omitted;
        }
    }

    public class Day
    {
        Days                mDay;
        public Subject[][]  mSubs;

        public Day() { }

        public Day(Days d)
        {
            mSubs   = new Subject[11][];
            mDay    = d;
        }

        public void AddLesson(Hours h, Subject[] arr)
        {
            mSubs[(int)h] = arr;
        }

        public Subject[] GetSubjectList(Hours h)
        {
            return mSubs[(int)h];
        }
    }
}
