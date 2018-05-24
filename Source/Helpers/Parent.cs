using System;

namespace ScheduleApp.Helpers
{
    public class Parent
    {
        public bool                     mEnabled, mEditMode;
        public int                      mSelected   { get; set; }
        public int                      mHour       { get; set; } 
        public Tuple<string, string>[]  mChildren   { get; set; }

        public Parent(Tuple<string, string>[] childrenList)
        {
            mChildren = childrenList;
            mSelected = -1;
            mEditMode = false;
            mEnabled  = mChildren.Length != 0;
        }

        public Parent(Tuple<string, string>[] childrenList, int selectedItem)
        {
            mChildren = childrenList;
            mSelected = selectedItem;
            mEditMode = false;
            mEnabled  = mChildren.Length != 0;
        }
    }
}