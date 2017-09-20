using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using AppTestProzesse.Header;
using System;

namespace DesignLibrary_Tutorial.Helpers
{
    public class Card
    {
        public DateTime mTime;
        public List<CardList> mCardList;
        public Card(List<CardList> list, DateTime date)
        {
            mTime = date.Date;
            mCardList = list;
        }
        public override int GetHashCode()
        {
            return mTime.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Card card = obj as Card;
            return GetHashCode() == card.GetHashCode();
        }
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
    }

    public class RecyclerViewAdapter : RecyclerView.Adapter
    {
        public List<Card> mList;

        public RecyclerViewAdapter(List<Card> list)
        {
            mList = list;
            for (int i = mList.Count - 1; i >= 0; i--)
            {
                if (mList[i].mTime.Date < DateTime.Now.Date)
                {
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
            CardViewHolder viewHolder = holder as CardViewHolder;
            viewHolder.mTextView.Text = mList[position].mTime.DayOfWeek.ToString();
            viewHolder.mDateText.Text = GetDisplayedDay(mList[position].mTime);
            viewHolder.mCardRV.SetAdapter(new CardListAdapter(mList[position].mCardList));
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

        public CardListAdapter(List<CardList> list)
        {
            mList = list;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View v = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.cardListItem, parent, false);
            return new ListViewHolder(v);
        }

        public override int ItemCount => mList.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ListViewHolder viewHolder = holder as ListViewHolder;
            viewHolder.mDescText.Visibility = ViewStates.Visible;
            viewHolder.mNameText.Visibility = ViewStates.Visible;
            viewHolder.mTypeText.TextSize = 20;

            if (mList[position].mSubject.omitted)
            {
                viewHolder.mNameText.Text = mList[position].mSubject.name;
                viewHolder.mTypeText.Text = "Entfall";
                if (mList[position].mSubject.change != null && mList[position].mSubject.change.remarks != null && mList[position].mSubject.change.remarks != "")
                {
                    viewHolder.mDescText.Text = mList[position].mSubject.change.remarks;
                }
                else
                {
                    viewHolder.mTypeText.TextSize = 24;
                    viewHolder.mDescText.Visibility = ViewStates.Gone;
                }
                viewHolder.mImageView.SetImageResource(Resource.Drawable.ic_cal_remove);
            }
            else if (mList[position].mSubject.change != null)
            {
                viewHolder.mNameText.Text = mList[position].mSubject.name;
                viewHolder.mTypeText.Text = mList[position].mSubject.change.type;
                viewHolder.mDescText.Text = "";
                if (mList[position].mSubject.change.type == "Entfall")
                {
                    viewHolder.mImageView.SetImageResource(Resource.Drawable.ic_cal_remove);
                }
                else
                {
                    viewHolder.mImageView.SetImageResource(Resource.Drawable.ic_cal_alert);
                }

                string stream = "";
                if (mList[position].mSubject.change.newSubject != "" && mList[position].mSubject.change.newSubject != mList[position].mSubject.name)
                {
                    stream += "Neues Fach: " + mList[position].mSubject.change.newSubject + "  ";
                }
                if (mList[position].mSubject.change.newRoom != "" && mList[position].mSubject.change.newRoom != mList[position].mSubject.room)
                {
                    stream += "Neuer Raum: " + mList[position].mSubject.change.newRoom + "  ";
                }
                if (mList[position].mSubject.change.remarks != "")
                {
                    stream += mList[position].mSubject.change.remarks + "  ";
                }
                //if(mList[position].mSubject.change.transfer != null)
                //{
                //Transfer 
                //}
                //Tabs/ Space

                if (stream != "")
                {
                    viewHolder.mDescText.Text = mList[position].mSubject.change.remarks;
                }
                else
                {
                    viewHolder.mTypeText.TextSize = 24;
                    viewHolder.mDescText.Visibility = ViewStates.Gone;
                }
            }
            else if (mList[position].mSubject.ev != null)
            {
                viewHolder.mNameText.Visibility = ViewStates.Gone;
                viewHolder.mTypeText.Text = mList[position].mSubject.ev.Describtion;
                viewHolder.mImageView.SetImageResource(Resource.Drawable.ic_cal_question);

                viewHolder.mTypeText.TextSize = 24;
                viewHolder.mDescText.Visibility = ViewStates.Gone;
            }

            //set Hours
            if (mList[position].h.Length == 1)
            {
                viewHolder.mHourText.Text = TimeHandler.HourIndex[(int)mList[position].h[0]];
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