using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using ScheduleApp.Handler;
using ScheduleApp.Objects;
using System;
using Android.Support.V4.View;
using Android.OS;

namespace ScheduleApp.Helpers
{
    public enum Type {USER, ALL};

    public class Card
    {
        public DateTime         mTime;
        public List<CardList>   mCardList;

        public Card(List<CardList> list, DateTime date)
        {
            mTime       = date.Date;
            mCardList   = list;
        }
    }

    public class CardList
    {
        public Subject mSubject;
        public Hours[] mHour;

        public CardList(Subject subject, Hours[] hours)
        {
            mSubject    = subject;
            mHour       = hours;
        }
    }

    public class RecyclerViewAdapter : RecyclerView.Adapter
    {
        public List<Card>   mList;
        private Type        mType;
        private bool        mTintActive, mPotraitMode;

        public RecyclerViewAdapter(List<Card> list, Type type, bool darkTheme, bool orientation)
        {
            mList           = list;
            mType           = type;
            mPotraitMode    = orientation;
            mTintActive     = darkTheme && Build.VERSION.SdkInt < BuildVersionCodes.Lollipop;
            SortOutData();
        }

        public void SortOutData()
        {
            if (mList == null)
                return;

            var config = DataHandler.GetConfig();
            for (int i = mList.Count - 1; i >= 0; i--)
            {
                if (mList[i].mTime.Date < DateTime.Now.Date)
                    mList.RemoveAt(i);

                else if (mType == Type.USER)
                {
                    var table = config.GetTableConf();

                    if (table == null)
                        return;

                    for (int j = mList[i].mCardList.Count - 1; j >= 0; j--)
                    {
                        if (table[(int)mList[i].mTime.Date.DayOfWeek - 1 % 7][(int)mList[i].mCardList[j].mHour[0]] != -1)
                            continue;

                        if (mList[i].mCardList[j].mSubject.mEvent != null && (int)mList[i].mCardList[j].mHour.Length == 2)
                        {
                            var affected = false;
                            for (int hour = (int)mList[i].mCardList[j].mHour[0] + 1; hour <= (int)mList[i].mCardList[j].mHour[1]; hour++)
                            {
                                if (table[(int)mList[i].mTime.Date.DayOfWeek - 1 % 7][hour] != -1)
                                {
                                    affected = true;
                                    break;
                                }
                            }
                            if (!affected)
                                mList[i].mCardList.RemoveAt(j);
                        }
                        else mList[i].mCardList.RemoveAt(j);
                    }
                    if (mList[i].mCardList.Count < 1)
                        mList.RemoveAt(i);
                }
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View v = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.cardView, parent, false);
            return new CardViewHolder(v);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var culture                 = new System.Globalization.CultureInfo("de-DE");
            CardViewHolder viewHolder   = holder as CardViewHolder;
            viewHolder.mTextView.Text   = culture.DateTimeFormat.GetDayName(mList[position].mTime.DayOfWeek);
            viewHolder.mDateText.Text   = GetDisplayedDay(mList[position].mTime);
            viewHolder.mCardRV.SetAdapter(new CardListAdapter(mList[position].mCardList, mTintActive, mPotraitMode, mType));
            //animate(holder.ItemView, position);
        }
        //private void AnimateView(View view, int pos)
        //{
        //    ViewCompat.Animate(view)
        //        .Alpha(1)
        //        .TranslationY(0) //-250 / -160
        //        .SetDuration(1500)
        //        //.SetInterpolator(new DecelerateInterpolator(1.2f))
        //        .SetStartDelay(pos * 100)
        //        .Start();
        //    //view.Animate().Cancel();
        //    //view.SetTranslationY(100);
        //    //view.SetAlpha(0);
        //    //view.Animate().alpha(1.0f).translationY(0).setDuration(300).setStartDelay(pos * 100);
        //}

        private string GetDisplayedDay(DateTime date)
        {
            var currentDate = DateTime.Now.Date;
            if (date.Date == currentDate)
                return "Heute";

            if (date.Date == currentDate.AddDays(1))
                return "Morgen";

            return "in " + date.Date.Subtract(currentDate).Days + " Tagen";
        }

        public override int ItemCount => mList != null ? mList.Count : 0;

        public override void OnAttachedToRecyclerView(RecyclerView recyclerView)
        {
            base.OnAttachedToRecyclerView(recyclerView);
        }

        public class CardViewHolder : RecyclerView.ViewHolder
        {
            public TextView     mTextView;
            public TextView     mDateText;
            public RecyclerView mCardRV;
            public CardViewHolder(View itemView) : base(itemView)
            {
                mTextView   = itemView.FindViewById<TextView>(Resource.Id.infoText);
                mDateText   = itemView.FindViewById<TextView>(Resource.Id.dateText);
                mCardRV     = itemView.FindViewById<RecyclerView>(Resource.Id.cardRecView);
                mCardRV.SetLayoutManager(new LinearLayoutManager(itemView.Context));
                //cardRV.AddItemDecoration(new DividerItemDecoration(itemView.Context, DividerItemDecoration.Vertical));
            }
        }
    }

    public class CardListAdapter : RecyclerView.Adapter
    {
        private List<CardList>  mList;
        private int             mResource;
        private bool            mTintMode, mPotraitMode, mAllPlansView;

        public CardListAdapter(List<CardList> list, bool tintMode, bool orientation, Type type)
        {
            mList           = list;
            mTintMode       = tintMode;
            mPotraitMode    = orientation;
            mAllPlansView   = type == Type.ALL;
            mResource       = Build.VERSION.SdkInt < BuildVersionCodes.Lollipop ? Resource.Layout.BL_CardListItem : Resource.Layout.cardListItem_plan;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View v = LayoutInflater.From(parent.Context).Inflate(mResource, parent, false);
            return new ListViewHolder(v);
        }

        public override int ItemCount => mList.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ListViewHolder viewHolder = holder as ListViewHolder;
            var subtext = string.Empty;

            viewHolder.mDescText.Visibility = ViewStates.Visible;
            viewHolder.mNameText.Visibility = ViewStates.Visible;
            viewHolder.mRoomText.Visibility = ViewStates.Invisible; 
            viewHolder.mTypeText.TextSize   = 24;

            if (mList[position].mSubject.mOmitted)
            {
                viewHolder.mNameText.Text = mList[position].mSubject.mName;
                viewHolder.mTypeText.Text = "Entfall";
                if (mList[position].mSubject.mChange != null && mList[position].mSubject.mChange.mRemarks != null && mList[position].mSubject.mChange.mRemarks != "")
                    subtext = mList[position].mSubject.mChange.mRemarks;
                viewHolder.mImageView.SetImageResource(Resource.Drawable.ic_cal_remove);
            }
            else if (mList[position].mSubject.mChange != null)
            {
                viewHolder.mNameText.Text = mList[position].mSubject.mName;
                viewHolder.mTypeText.Text = mList[position].mSubject.mChange.mType;

                if (mList[position].mSubject.mChange.mType == "Entfall")
                    viewHolder.mImageView.SetImageResource(Resource.Drawable.ic_cal_remove);
                else
                    viewHolder.mImageView.SetImageResource(Resource.Drawable.ic_cal_alert);

                if (mList[position].mSubject.mChange.mNewSubject != "" && mList[position].mSubject.mChange.mNewSubject != mList[position].mSubject.mName)
                    subtext += "Neues Fach: " + mList[position].mSubject.mChange.mNewSubject + "  ";

                if (mList[position].mSubject.mChange.mNewRoom != "" && mList[position].mSubject.mChange.mNewRoom != mList[position].mSubject.mRoom) //mList[position].mSubject.change.newRoom != mList[position].mSubject.room
                    subtext += "Neuer Raum: " + mList[position].mSubject.mChange.mNewRoom + "  ";

                if (mList[position].mSubject.mChange.mRemarks != null && mList[position].mSubject.mChange.mRemarks != "")
                    subtext += mList[position].mSubject.mChange.mRemarks + "  ";

                if (mList[position].mSubject.mChange.mTransfer != null)
                {
                    var trans = mList[position].mSubject.mChange.mTransfer;
                    if (trans.Item1 != null && trans.Item1.Length > 2)
                        subtext += trans.Item1;

                    if (trans.Item2 != null && trans.Item2.Length > 2)
                        subtext += trans.Item2;
                }
            }
            else if (mList[position].mSubject.mEvent != null)
            {
                viewHolder.mNameText.Visibility = ViewStates.Gone;
                if (mPotraitMode && mList[position].mSubject.mEvent.mDescribtion.Length > 24)
                    viewHolder.mTypeText.TextSize = 20 - mList[position].mSubject.mEvent.mDescribtion.Length / 10f;
                viewHolder.mTypeText.Text = mList[position].mSubject.mEvent.mDescribtion;
                viewHolder.mImageView.SetImageResource(Resource.Drawable.ic_cal_question);
            }
            if (subtext != "" && subtext != " ") 
            {
                viewHolder.mTypeText.TextSize = 20;
                if (mPotraitMode && subtext.Length > 45) //lower textsize if text is too long
                    viewHolder.mDescText.TextSize = 9;
                else
                    viewHolder.mDescText.TextSize = 10;
                viewHolder.mDescText.Text = subtext;
                viewHolder.mDescText.Visibility = ViewStates.Visible;
            }
            else
                viewHolder.mDescText.Visibility = ViewStates.Gone;

            //set Hours
            if (mList[position].mHour.Length == 1)
                viewHolder.mHourText.Text = TimeHandler.HourIndex[(int)mList[position].mHour[0]] + ".";

            else if (mList[position].mHour.Length == 2)
            {
                if (mList[position].mHour[0] + 1 == mList[position].mHour[1])
                    viewHolder.mHourText.Text = TimeHandler.HourIndex[(int)mList[position].mHour[0]] + "/" + TimeHandler.HourIndex[(int)mList[position].mHour[1]];
                else
                    viewHolder.mHourText.Text = TimeHandler.HourIndex[(int)mList[position].mHour[0]] + "-" + TimeHandler.HourIndex[(int)mList[position].mHour[1]];
            }

            //Room
            if (mAllPlansView && mList[position].mSubject.mEvent == null)
            {
                viewHolder.mRoomText.Text       = mList[position].mSubject.mRoom.Replace(".","").Split(',')[0];
                viewHolder.mRoomText.Visibility = ViewStates.Visible;
            }
        }

        public override void OnAttachedToRecyclerView(RecyclerView recyclerView)
        {
            base.OnAttachedToRecyclerView(recyclerView);
        }

        public class ListViewHolder : RecyclerView.ViewHolder
        {
            public ImageView    mImageView;
            public TextView     mNameText;
            public TextView     mTypeText;
            public TextView     mDescText;
            public TextView     mHourText;
            public TextView     mRoomText;
            public ListViewHolder(View itemView) : base(itemView)
            {
                mImageView  = itemView.FindViewById<ImageView>(Resource.Id.cardListImageView);
                mNameText   = itemView.FindViewById<TextView>(Resource.Id.nameText);
                mDescText   = itemView.FindViewById<TextView>(Resource.Id.descText);
                mTypeText   = itemView.FindViewById<TextView>(Resource.Id.typeText);
                mHourText   = itemView.FindViewById<TextView>(Resource.Id.hourText);
                mRoomText   = itemView.FindViewById<TextView>(Resource.Id.roomText);
            }
        }
    }
}