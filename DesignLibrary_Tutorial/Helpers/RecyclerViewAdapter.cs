using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Helper.Header;
using System;
using Java.Lang;
using Android.Support.V4.View;
using Android.Views.Animations;
using Android.OS;

namespace ScheduleApp.Helpers
{
    public enum Type {USER, ALL};
    public class Card
    {
        public DateTime mTime;
        public List<CardList> mCardList;
        public Card(List<CardList> list, DateTime date)
        {
            mTime = date.Date;
            mCardList = list;
        }
        //public override int GetHashCode()
        //{
        //    return mTime.GetHashCode() + mCardList.GetHashCode();
        //}

        //public override bool Equals(object obj)
        //{
        //    Card card = obj as Card;
        //    return card == null ? false : GetHashCode() == card.GetHashCode();
        //}
    }

    public class CardList
    {
        public Subject mSubject;
        public Hours[] h;
        public CardList(Subject subject, Hours[] hours)
        {
            mSubject = subject;
            h = hours;
        }
        //public override int GetHashCode()
        //{
        //    return mSubject.GetHashCode() + h.GetHashCode();
        //}

        //public override bool Equals(object obj)
        //{
        //    CardList cardList = obj as CardList;
        //    return cardList == null ? false : GetHashCode() == cardList.GetHashCode();
        //}
    }

    public class RecyclerViewAdapter : RecyclerView.Adapter
    {
        public List<Card> mList;
        public Type mType;
        bool mTintActive;
        public RecyclerViewAdapter(List<Card> list, Type type, bool darkTheme)
        {
            mList = list;
            mType = type;
            mTintActive = darkTheme && Build.VERSION.SdkInt < BuildVersionCodes.Lollipop;
            SortOutData();
        }

        public void SortOutData()
        {
            if (mList != null)
            {
                var config = DataHandler.GetConfig();
                for (int i = mList.Count - 1; i >= 0; i--)
                {
                    if (mList[i].mTime.Date < DateTime.Now.Date)
                    {
                        mList.RemoveAt(i);
                    }
                    else if (mType == Type.USER)
                    {
                        for (int j = mList[i].mCardList.Count - 1; j >= 0; j--)
                        {
                            var table = config.GetTableConf();
                            if (table[(int)mList[i].mTime.Date.DayOfWeek - 1 % 7][(int)mList[i].mCardList[j].h[0]] == -1)
                            {
                                if (mList[i].mCardList[j].mSubject.ev != null && (int)mList[i].mCardList[j].h.Length == 2)
                                {
                                    var affected = false;
                                    for (int hour = (int)mList[i].mCardList[j].h[0] + 1; hour <= (int)mList[i].mCardList[j].h[1]; hour++)
                                    {
                                        if (table[(int)mList[i].mTime.Date.DayOfWeek - 1 % 7][hour] != -1)
                                        {
                                            affected = true;
                                            break;
                                        }                       
                                    }
                                    if (!affected)
                                    {
                                        mList[i].mCardList.RemoveAt(j);
                                    }
                                }
                                else mList[i].mCardList.RemoveAt(j);
                            }
                        }
                        if (mList[i].mCardList.Count < 1)
                        {
                            mList.RemoveAt(i);
                        }
                    }
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
            CardViewHolder viewHolder = holder as CardViewHolder;
            var culture = new System.Globalization.CultureInfo("de-DE");
            viewHolder.mTextView.Text = culture.DateTimeFormat.GetDayName(mList[position].mTime.DayOfWeek);
            viewHolder.mDateText.Text = GetDisplayedDay(mList[position].mTime);
            viewHolder.mCardRV.SetAdapter(new CardListAdapter(mList[position].mCardList, mTintActive));
            //animate(holder.ItemView, position);
        }
        private void AnimateView(View view, int pos)
        {
            ViewCompat.Animate(view)
                .Alpha(1)
                .TranslationY(0) //-250 / -160
                .SetDuration(1500)
                //.SetInterpolator(new DecelerateInterpolator(1.2f))
                .SetStartDelay(pos * 100)
                .Start();
            //view.Animate().Cancel();
            //view.SetTranslationY(100);
            //view.SetAlpha(0);
            //view.Animate().alpha(1.0f).translationY(0).setDuration(300).setStartDelay(pos * 100);
        }

        private string GetDisplayedDay(DateTime date)
        {
            DateTime now = DateTime.Now.Date;
            if (date.Date == now)
            {
                return "Heute";
            }
            if (date.Date == now.AddDays(1))
            {
                return "Morgen";
            }
            return "in " + date.Date.Subtract(now).Days + " Tagen";
        }

        public override int ItemCount => mList.Count;

        public override void OnAttachedToRecyclerView(RecyclerView recyclerView)
        {
            base.OnAttachedToRecyclerView(recyclerView);
        }

        public class CardViewHolder : RecyclerView.ViewHolder
        {
            public TextView mTextView;
            public TextView mDateText;
            public RecyclerView mCardRV;
            public CardViewHolder(View itemView) : base(itemView)
            {
                mTextView = itemView.FindViewById<TextView>(Resource.Id.infoText);
                mDateText = itemView.FindViewById<TextView>(Resource.Id.dateText);
                mCardRV = itemView.FindViewById<RecyclerView>(Resource.Id.cardRecView);
                mCardRV.SetLayoutManager(new LinearLayoutManager(itemView.Context));
                //cardRV.AddItemDecoration(new DividerItemDecoration(itemView.Context, DividerItemDecoration.Vertical));
            }
        }
    }

    public class CardListAdapter : RecyclerView.Adapter
    {
        List<CardList> mList;
        private bool mTintMode;
        private int mResource;

        public CardListAdapter(List<CardList> list, bool tintMode)
        {
            mList = list;
            mTintMode = tintMode;
            mResource = Build.VERSION.SdkInt < BuildVersionCodes.Lollipop ? Resource.Layout.BL_CardListItem : Resource.Layout.cardListItem;

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
            viewHolder.mDescText.Visibility = ViewStates.Visible;
            viewHolder.mNameText.Visibility = ViewStates.Visible;
            string subtext = "";
            viewHolder.mTypeText.TextSize = 24;

            if (mList[position].mSubject.omitted)
            {
                viewHolder.mNameText.Text = mList[position].mSubject.name;
                viewHolder.mTypeText.Text = "Entfall";
                if (mList[position].mSubject.change != null && mList[position].mSubject.change.remarks != null && mList[position].mSubject.change.remarks != "")
                {
                    subtext = mList[position].mSubject.change.remarks;
                }
                viewHolder.mImageView.SetImageResource(Resource.Drawable.ic_cal_remove);
            }
            else if (mList[position].mSubject.change != null)
            {
                viewHolder.mNameText.Text = mList[position].mSubject.name;
                viewHolder.mTypeText.Text = mList[position].mSubject.change.type;

                if (mList[position].mSubject.change.type == "Entfall")
                {
                    viewHolder.mImageView.SetImageResource(Resource.Drawable.ic_cal_remove);
                }
                else
                {
                    viewHolder.mImageView.SetImageResource(Resource.Drawable.ic_cal_alert);
                }

                if (mList[position].mSubject.change.newSubject != "" && mList[position].mSubject.change.newSubject != mList[position].mSubject.name)
                {
                    subtext += "Neues Fach: " + mList[position].mSubject.change.newSubject + "  ";
                }
                if (mList[position].mSubject.change.newRoom != "" && mList[position].mSubject.change.newRoom != mList[position].mSubject.room) //mList[position].mSubject.change.newRoom != mList[position].mSubject.room
                {
                    subtext += "Neuer Raum: " + mList[position].mSubject.change.newRoom + "  ";
                }
                if (mList[position].mSubject.change.remarks != null && mList[position].mSubject.change.remarks != "")
                {
                    subtext += mList[position].mSubject.change.remarks + "  ";
                }
                if (mList[position].mSubject.change.transfer != null)
                {
                    var trans = mList[position].mSubject.change.transfer;
                    if (trans.Item1 != null && trans.Item1.Length > 2)
                    {
                        subtext += trans.Item1;
                    }
                    if (trans.Item2 != null && trans.Item2.Length > 2)
                    {
                        subtext += trans.Item2;
                    }
                }
            }
            else if (mList[position].mSubject.ev != null)
            {
                viewHolder.mNameText.Visibility = ViewStates.Gone;
                if (mList[position].mSubject.ev.Describtion.Length > 24)
                {
                    viewHolder.mTypeText.TextSize = 20 - mList[position].mSubject.ev.Describtion.Length / 10f;
                }
                viewHolder.mTypeText.Text = mList[position].mSubject.ev.Describtion;
                viewHolder.mImageView.SetImageResource(Resource.Drawable.ic_cal_question);
            }
            if (subtext != "" && subtext != " ")
            {
                viewHolder.mTypeText.TextSize = 20;
                viewHolder.mDescText.Text = subtext;
                viewHolder.mDescText.Visibility = ViewStates.Visible;
            }
            else
            {
                viewHolder.mDescText.Visibility = ViewStates.Gone;
            }

            //set Hours
            if (mList[position].h.Length == 1)
            {
                viewHolder.mHourText.Text = TimeHandler.HourIndex[(int)mList[position].h[0]] + ".";
            }
            else if (mList[position].h.Length == 2)
            {
                if (mList[position].h[0] + 1 == mList[position].h[1])
                {
                    viewHolder.mHourText.Text = TimeHandler.HourIndex[(int)mList[position].h[0]] + "/" + TimeHandler.HourIndex[(int)mList[position].h[1]];
                }
                else
                {
                    viewHolder.mHourText.Text = TimeHandler.HourIndex[(int)mList[position].h[0]] + "-" + TimeHandler.HourIndex[(int)mList[position].h[1]];
                }
            }
        }

        public override void OnAttachedToRecyclerView(RecyclerView recyclerView)
        {
            base.OnAttachedToRecyclerView(recyclerView);
        }

        public class ListViewHolder : RecyclerView.ViewHolder
        {
            public ImageView mImageView;
            public TextView mNameText;
            public TextView mTypeText;
            public TextView mDescText;
            public TextView mHourText;
            public ListViewHolder(View itemView) : base(itemView)
            {
                mImageView = itemView.FindViewById<ImageView>(Resource.Id.cardListImageView);
                mNameText = itemView.FindViewById<TextView>(Resource.Id.nameText);
                mDescText = itemView.FindViewById<TextView>(Resource.Id.descText);
                mTypeText = itemView.FindViewById<TextView>(Resource.Id.typeText);
                mHourText = itemView.FindViewById<TextView>(Resource.Id.hourText);
            }
        }
    }
}