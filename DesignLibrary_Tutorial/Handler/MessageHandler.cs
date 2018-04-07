using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using ScheduleApp.Helpers;
using Newtonsoft.Json;
using System.Threading.Tasks;
using ScheduleApp.Objects;

namespace ScheduleApp.Handler
{
    public class MessageHandler
    {
        Week[]              mWeek;
        List<LData>         mMsgListOld;
        DataHandler         mDataHandler;
        public List<Card>   mList;
        [JsonProperty]
        List<LData>         mMsgList;

        public event EventHandler OnDataChanged;

        [JsonConstructor]
        public MessageHandler(List<Card> list, List<LData> msgList)
        {
            mDataHandler    = DataHandler.GetDataHandler();
            mList           = list;
            mMsgList        = msgList;
            mMsgListOld     = new List<LData>();
            mWeek           = new Week[2];
            DeleteOutdatedData();
        }

        public MessageHandler()
        {
            mDataHandler    = DataHandler.GetDataHandler();
            mWeek           = new Week[2];
            mMsgList        = new List<LData>();
            mMsgListOld     = new List<LData>();
            Update();
            DeleteOutdatedData();
        }

        ~MessageHandler()
        {
            SaveMsgHandler(this);
        }

        public Task<bool> UpdateAsync()
        {
            return Task.Factory.StartNew(() => Update());
        }

        public bool Update()
        {
            mWeek[0] = mDataHandler.GetDetailedWeek(0, true);
            mWeek[1] = mDataHandler.GetDetailedWeek(1, true);
            if (mWeek[0] != null || mWeek[1] != null) //to avoid resetting data if there is no internet connection
            {
                mList = DataToList();
                Check();
                SaveMsgHandler(this);
                return true;
            }
            return false;
        }

        private void DeleteOutdatedData()
        {
            if (mList == null)
                return;

            for (int i = mList.Count - 1; i >= 0; i--)
            {
                if (mList[i].mTime.Date < DateTime.Now.Date)
                    mList.RemoveAt(i);
            }
        }

        public List<Card> DataToList()
        {
            List<Card> tList    = new List<Card>();
            int[][] config      = DataHandler.GetConfig().GetTableConf();

            if (mMsgList != null) //to avoid resetting data if there is no internet connection
            {
                mMsgListOld = mMsgList;
                mMsgList    = new List<LData>();
            }

            for (int wp = 0; mWeek != null && wp < mWeek.Length; wp++)
            {
                if (mWeek[wp] == null)
                    continue;

                for (int day = 0; day < 5; day++)
                {
                    List<CardList> tCardList = new List<CardList>();
                    for (int hour = 0; hour < 11; hour++)
                    {
                        if (config[day][hour] != -1 && mWeek[wp].mWeek[day].mSubs[hour] != null && mWeek[wp].mWeek[day].mSubs[hour].Length > config[day][hour])
                        {
                            Subject temp = mWeek[wp].mWeek[day].mSubs[hour][config[day][hour]];
                            if (temp.mOmitted || temp.mChange != null || temp.mEvent != null)
                            {
                                if (temp.mEvent != null)
                                {
                                    CardList cList  = new CardList(temp, new Hours[] { temp.mEvent.mHour, temp.mEvent.mNumber });
                                    hour            = (int)temp.mEvent.mNumber;
                                    tCardList.Add(cList);
                                }
                                else if (tCardList.Count > 0 && tCardList[tCardList.Count - 1].mSubject.mRoom == temp.mRoom && tCardList[tCardList.Count - 1].mSubject.mName == temp.mName
                                        && tCardList[tCardList.Count - 1].mHour.Length == 1 && (int)tCardList[tCardList.Count - 1].mHour[0] + 1 == hour)
                                {
                                    tCardList[tCardList.Count - 1].mHour = new Hours[] { tCardList[tCardList.Count - 1].mHour[0], (Hours)hour };
                                }
                                else
                                {
                                    CardList cList = new CardList(temp, new Hours[] { (Hours)hour });
                                    tCardList.Add(cList);
                                }
                            }
                        }
                        else if (mWeek[wp].mEvents.Count > 0 && mWeek[wp].mWeek[day].mSubs[hour] != null && mWeek[wp].mWeek[day].mSubs[hour][0].mEvent != null)
                        {
                            Subject temp = mWeek[wp].mWeek[day].mSubs[hour][0];
                            for (int h = (int)temp.mEvent.mHour; h <= (int)temp.mEvent.mNumber; h++)
                            {
                                if (config[day][h] == -1)
                                    continue;

                                tCardList.Add(new CardList(temp, new Hours[] { temp.mEvent.mHour, temp.mEvent.mNumber }));
                                hour = (int)temp.mEvent.mNumber; //Check
                                break;
                            }
                        }
                        else if (config[day][hour] != -1 && mWeek[wp].mWeek[day].mSubs[hour] != null && mWeek[wp].mWeek[day].mSubs[hour].Length == 1) //Check
                        {
                            Subject temp    = mWeek[wp].mWeek[day].mSubs[hour][0];
                            temp.mEvent     = new Event((Days)day, (Hours)hour, (Hours)hour, temp.mName);
                            tCardList.Add(new CardList(temp, new Hours[] { temp.mEvent.mHour }));
                        }
                    }
                    if (tCardList.Count > 0)
                    {
                        tList.Add(new Card(tCardList, mWeek[wp].mMonDate.AddDays(day)));
                        foreach (var item in tCardList)
                        {
                            mMsgList.Add(new LData(mWeek[wp].mMonDate.AddDays(day).Date, item));
                        }
                    }
                }
            }
            return tList;
        }

        public static List<Card> GetCardList(Week[] week)
        {
            List<Card> tList = new List<Card>();

            for (int wp = 0; week != null && wp < week.Length; wp++)
            {
                if (week[wp] == null)
                    continue;

                for (int day = 0; day < 5; day++)
                {
                    if (week[wp].mMonDate.AddDays(day).Date >= System.DateTime.UtcNow.Date)
                    {
                        List<CardList> tCardList    = new List<CardList>();
                        int[] count                 = { 0, 0, 0, 0 };
                        for (int hour = 0; hour < 11; hour++)
                        {
                            if (week[wp].mWeek[day].mSubs[hour] == null)
                                continue;

                            count[0] = count[1] - count[2];
                            count[1] = 0;
                            count[2] = 0;
                            //count[3] = 0;

                            for (int index = 0; index < week[wp].mWeek[day].mSubs[hour].Length; index++)
                            {
                                Subject temp = null;
                                if (week[wp].mWeek[day].mSubs[hour][index] != null)
                                    temp = week[wp].mWeek[day].mSubs[hour][index];
                                if (temp != null && (temp.mOmitted || temp.mChange != null || temp.mEvent != null))
                                {
                                    if (temp.mEvent != null)
                                    {
                                        CardList cList  = new CardList(temp, new Hours[] { temp.mEvent.mHour, temp.mEvent.mNumber });
                                        hour            = (int)temp.mEvent.mNumber; //Check
                                        tCardList.Add(cList);
                                        break;
                                    }
                                    else
                                    {
                                        int counter = count[0] + count[1] - count[2];
                                        if (counter > 0 && tCardList.Count >= counter && tCardList[tCardList.Count - counter].mSubject.mRoom == temp.mRoom && tCardList[tCardList.Count - counter].mSubject.mName == temp.mName
                                            && tCardList[tCardList.Count - counter].mHour.Length == 1 && (int)tCardList[tCardList.Count - counter].mHour[0] + 1 == hour)
                                        {
                                            count[2]++;
                                            tCardList[tCardList.Count - counter].mHour = new Hours[] { tCardList[tCardList.Count - counter].mHour[0], (Hours)hour };
                                        }
                                        else if (count[3] > 0 && tCardList.Count >= count[3] && tCardList[tCardList.Count - count[3]].mSubject.mRoom == temp.mRoom && tCardList[tCardList.Count - count[3]].mSubject.mName == temp.mName
                                            && tCardList[tCardList.Count - count[3]].mHour.Length == 1 && (int)tCardList[tCardList.Count - count[3]].mHour[0] + 1 == hour)
                                        {
                                            count[2]++;
                                            tCardList[tCardList.Count - count[3]].mHour = new Hours[] { tCardList[tCardList.Count - count[3]].mHour[0], (Hours)hour };
                                        }
                                        else
                                        {
                                            CardList cList = new CardList(temp, new Hours[] { (Hours)hour });
                                            tCardList.Add(cList);
                                            count[1]++;
                                        }
                                        count[3] = counter;
                                    }
                                }
                            }
                        }
                        if (tCardList.Count > 0)
                            tList.Add(new Card(tCardList, week[wp].mMonDate.AddDays(day)));
                    }
                }
            }
            return tList;
        }

        public void DeleteOutdatedDataAsync()
        {
            mDataHandler.DeleteOutdatedDataAsync();
        }

        private void Check()
        {
            if (mMsgListOld != mMsgList)
            {
                //New Messages
                var list = ExceptLData(mMsgList, mMsgListOld);//mMsgList.Where(i => !mMsgListOld.Contains(i)).ToList();//mMsgList.Except(mMsgListOld).ToList();
                if (list.Count > 0)
                {
                    //list are new messages that will be displayed in refresh or as notification
                    if (OnDataChanged != null)
                        OnDataChanged(list, new MessageArgs(mMsgListOld.Count == 0));
                }

                //Delete outdated Messages
                var outdatedData = ExceptLData(mMsgListOld, mMsgList);
                foreach (var outdatedItem in outdatedData)
                {
                    mMsgListOld.Remove(outdatedItem);
                }
            }
        }

        public List<LData> ExceptLData(List<LData> a, List<LData> b)
        {
            List<LData> output = new List<LData>();
            foreach (var aItem in a)
            {
                bool exists = false;
                foreach (var bItem in b)
                {
                    if (aItem.Equals(bItem))
                    {
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                    output.Add(aItem);
            }
            return output;
        }

        public string GetCurrentClass()
        {
            return DataHandler.GetConfig().GetClassName();
        }

        public static MessageHandler GetMsgHandler()
        {
            try
            {
                string source = Application.Context.GetSharedPreferences("Dashboard", FileCreationMode.Private).GetString("MsgHandler", string.Empty);
                return JsonConvert.DeserializeObject<MessageHandler>(source);
            }
            catch (Exception) { }
            //log
            return new MessageHandler();
        }

        public static void SaveMsgHandler(MessageHandler messageHandler)
        {
            var e = Application.Context.GetSharedPreferences("Dashboard", FileCreationMode.Private).Edit();
            try
            {
                string s = JsonConvert.SerializeObject(messageHandler);
                e.PutString("MsgHandler", s).Apply();

            }
            catch (Exception ex)
            {
                //log
                var m = ex.Message;
            }
        }
    }

    public struct LData
    {
        public DateTime mDate;
        public CardList mItem;

        public LData(DateTime dateTime, CardList cardList)
        {
            mDate = dateTime;
            mItem = cardList;
        }
        public override bool Equals(object obj)
        {
            //Ensures exceptions
            LData ob;
            try
            {
                ob = (LData)obj;
            }
            catch (Exception)
            {
                ob = new LData();
                return false;
            }

            bool a = mDate.Date == ob.mDate.Date;
            bool b2 = mItem.mHour[0] == ob.mItem.mHour[0] && mItem.mHour.Length == ob.mItem.mHour.Length;
            if (b2 && mItem.mHour.Length == 2)
            {
                b2 = mItem.mHour[1] == ob.mItem.mHour[1];
            }
            bool b1 = mItem.mSubject.mName == ob.mItem.mSubject.mName;
            bool b3 = mItem.mSubject.mRoom == ob.mItem.mSubject.mRoom;
            bool b4 = mItem.mSubject.mOmitted == ob.mItem.mSubject.mOmitted;
            return a && b2 && b1 && b3 && b4;
        }
    }

    public class MessageArgs : EventArgs
    {
        private bool mOldListisEmpty;
        public MessageArgs(bool isEmpty)
        {
            mOldListisEmpty = isEmpty;
        }
        public bool EmptyList
        {
            get { return mOldListisEmpty; }
        }
    }
}