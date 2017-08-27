using System;

namespace DesignLibrary_Tutorial.Helpers
{
    public class Parent
    {
        public int mSelected { get; set; }
        public int mHour { get; set; }
        public Tuple<string, string>[] mChildren {get; set;}
        public bool mEnabled, mEditMode;

        public Parent(Tuple<string, string>[] childrenList)
        {
            mChildren = childrenList;
            mSelected = -1;
            mEditMode = false;
            if (mChildren.Length == 0)
            {
                mEnabled = false;
            }
            else mEnabled = true;
        }

        public Parent(Tuple<string, string>[] childrenList, int selectedItem)
        {
            mChildren = childrenList;
            mSelected = selectedItem;
            mEditMode = false;
            if (mChildren.Length == 0)
            {
                mEnabled = false;
            }
            else mEnabled = true;
        }
    }
}