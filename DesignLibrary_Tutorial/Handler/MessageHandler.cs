using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Helper.Header;
using ScheduleApp.Helpers;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ScheduleApp.Handler
{
    public class MessageHandler
    {
        Week[] mWeek;
        List<LData> mMsgListOld;
        DataHandler mDataHandler;
        public List<Card> mList;
        [JsonProperty]
        List<LData> mMsgList; 
        public event EventHandler OnDataChanged;

        [JsonConstructor]
        public MessageHandler(List<Card> list, List<LData> msgList)
        {
            mDataHandler = DataHandler.GetDataHandler();
            mList = list;
            mMsgList = msgList;
            mMsgListOld = new List<LData>();
            mWeek = new Week[2];
            DeleteOutdatedData();
        }

        public MessageHandler()
        {
            mDataHandler = DataHandler.GetDataHandler();
            mWeek = new Week[2];
            mMsgList = new List<LData>();
            mMsgListOld = new List<LData>();
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
            //mDataHandler.LoadCfg();
            mWeek[0] = mDataHandler.GetDetailedWeek(0, true);
            mWeek[1] = mDataHandler.GetDetailedWeek(1, true);
            if ((mWeek[0] != null || mWeek[1] != null)) //to avoid resetting data if there is no internet connection
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
            if(mList != null)
            {
                for (int i = mList.Count - 1; i >= 0; i--)
                {
                    if (mList[i].mTime.Date < DateTime.Now.Date)
                    {
                        mList.RemoveAt(i);
                    }
                    //else
                    //{
                    //    for (int j = mList[i].mCardList.Count - 1; j >= 0; j--)
                    //    {
                    //        if (DataHandler.GetConfig().GetTableConf()[(int)mList[i].mTime.Date.DayOfWeek - 1 % 7][(int)mList[i].mCardList[j].h[0]] == -1)
                    //        {
                    //            mList[i].mCardList.RemoveAt(j);
                    //        }
                    //    }
                    //    if (mList[i].mCardList.Count < 1)
                    //    {
                    //        mList.RemoveAt(i);
                    //    }
                    //}
                }
            }
        }

        public List<Card> DataToList()
        {
            List<Card> tList = new List<Card>();
            int[][] config = DataHandler.GetConfig().GetTableConf();

            if ( mMsgList != null) //to avoid resetting data if there is no internet connection
            {
                mMsgListOld = mMsgList;
                mMsgList = new List<LData>();
            }

            for (int wp = 0; mWeek != null && wp < mWeek.Length; wp++)
            {
                if (mWeek[wp] != null)
                {
                    for (int day = 0; day < 5; day++)
                    {
                        List<CardList> tCardList = new List<CardList>();
                        for (int hour = 0; hour < 11; hour++)
                        {
                            if (config[day][hour] != -1 && mWeek[wp].week[day].list[hour] != null && mWeek[wp].week[day].list[hour].Length > config[day][hour])
                            {
                                //if (mWeek[wp].week[day].list[hour] != null && (mWeek[wp].week[day].list[hour].Length >= config[day][hour])
                                //{
                                    Subject temp = mWeek[wp].week[day].list[hour][config[day][hour]];
                                    if (temp.omitted || temp.change != null || temp.ev != null)
                                    {

                                        if (temp.ev != null)
                                        {
                                            CardList cList = new CardList(temp, new Hours[] { temp.ev.Hour, temp.ev.Number });
                                            tCardList.Add(cList);
                                            hour = (int)temp.ev.Number; //Check
                                        }
                                        else
                                        {
                                            if (tCardList.Count > 0 && tCardList[tCardList.Count - 1].mSubject.room == temp.room && tCardList[tCardList.Count - 1].mSubject.name == temp.name
                                                && tCardList[tCardList.Count - 1].h.Length == 1 && (int)tCardList[tCardList.Count - 1].h[0] + 1 == hour)
                                            {
                                                tCardList[tCardList.Count - 1].h = new Hours[] { tCardList[tCardList.Count - 1].h[0], (Hours)hour };
                                            }
                                            else
                                            {
                                                CardList cList = new CardList(temp, new Hours[] { (Hours)hour });
                                                tCardList.Add(cList);
                                            }
                                        }
                                    }
                                //}
                            }
                            else if (mWeek[wp].mEvents.Count > 0 && mWeek[wp].week[day].list[hour] != null && mWeek[wp].week[day].list[hour][0].ev != null)
                            {
                                Subject temp = mWeek[wp].week[day].list[hour][0];
                                for (int h = (int)temp.ev.Hour; h <= (int)temp.ev.Number; h++)
                                {
                                    if (config[day][h] != -1)
                                    {
                                        tCardList.Add(new CardList(temp, new Hours[] { temp.ev.Hour, temp.ev.Number }));
                                        hour = (int) temp.ev.Number; //Check
                                        break;
                                    }
                                }
                            }
                            else if (config[day][hour] != -1 && mWeek[wp].week[day].list[hour] != null && mWeek[wp].week[day].list[hour].Length == 1) //Check
                            {
                                Subject temp = mWeek[wp].week[day].list[hour][0];
                                temp.ev = new Event((Days) day, (Hours) hour, (Hours) hour, temp.name);
                                tCardList.Add(new CardList(temp, new Hours[] { temp.ev.Hour }));
                            }
                        }
                        if (tCardList.Count > 0)
                        {
                            tList.Add(new Card(tCardList, mWeek[wp].tMon.AddDays(day)));
                            foreach (var item in tCardList)
                            {
                                mMsgList.Add(new LData(mWeek[wp].tMon.AddDays(day).Date, item));
                            }
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
                if (week[wp] != null)
                {
                    for (int day = 0; day < 5; day++)
                    {
                        if (week[wp].tMon.AddDays(day).Date >= System.DateTime.UtcNow.Date)
                        {
                            List<CardList> tCardList = new List<CardList>();
                            int[] count = { 0, 0, 0 };
                            for (int hour = 0; hour < 11; hour++)
                            {
                                if (week[wp].week[day].list[hour] != null)
                                {
                                    count[0] = count[1];
                                    count[1] = 0;
                                    count[2] = 0;
                                    for (int index = 0; index < week[wp].week[day].list[hour].Length; index++)
                                    {
                                        Subject temp = null;
                                        if (week[wp].week[day].list[hour][index] != null)
                                            temp = week[wp].week[day].list[hour][index];
                                        if (temp != null && (temp.omitted || temp.change != null || temp.ev != null))
                                        {
                                            if (temp.ev != null)
                                            {
                                                CardList cList = new CardList(temp, new Hours[] { temp.ev.Hour, temp.ev.Number });
                                                tCardList.Add(cList);
                                                hour = (int)temp.ev.Number; //Check
                                                break;
                                            }
                                            else
                                            {
                                                int counter = count[0] + count[1] - count[2];
                                                if (tCardList.Count > 0 && tCardList[tCardList.Count - 1].mSubject.room == temp.room && tCardList[tCardList.Count - 1].mSubject.name == temp.name
                                                    && tCardList[tCardList.Count - 1].h.Length == 1 && (int)tCardList[tCardList.Count - 1].h[0] + 1 == hour)
                                                {
                                                    count[2]++;
                                                    tCardList[tCardList.Count - 1].h = new Hours[] { tCardList[tCardList.Count - 1].h[0], (Hours)hour };
                                                }
                                                else if (counter > 0 && tCardList.Count >= counter && tCardList[tCardList.Count - counter].mSubject.room == temp.room && tCardList[tCardList.Count - counter].mSubject.name == temp.name
                                                    && tCardList[tCardList.Count - counter].h.Length == 1 && (int)tCardList[tCardList.Count - counter].h[0] + 1 == hour)
                                                {
                                                    count[2]++;
                                                    tCardList[tCardList.Count - counter].h = new Hours[] { tCardList[tCardList.Count - counter].h[0], (Hours)hour };
                                                }
                                                else
                                                {
                                                    CardList cList = new CardList(temp, new Hours[] { (Hours)hour });
                                                    tCardList.Add(cList);
                                                    count[1]++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (tCardList.Count > 0)
                            {
                                tList.Add(new Card(tCardList, week[wp].tMon.AddDays(day)));
                            }
                        }
                    }
                }
            }
            return tList;
        }

        public void DeleteOutdatedDataAsync()
        {
            mDataHandler.DeleteOutdatedDataAsync();
        }

        //test
        public class MessageArgs : EventArgs
        {
            private bool oldListisEmpty;
            public MessageArgs(bool isEmpty)
            {
                oldListisEmpty = isEmpty;
            }
            public bool EmptyList
            {
                get { return oldListisEmpty; }
            } 
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
                    {
                        OnDataChanged(list, new MessageArgs(mMsgListOld.Count == 0));
                    }  
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
                {
                    output.Add(aItem);
                }
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
        public DateTime date;
        public CardList item;

        public LData(DateTime dateTime, CardList cardList)
        {
            date = dateTime;
            item = cardList;
        }
        public override bool Equals(object obj)
        {
            //Ensures exceptions
            LData ob;
            try
            {
                ob = (LData)obj;
            } catch(Exception)
            {
                ob = new LData();
                return false;
            }
            
            bool a = date.Date == ob.date.Date;
            bool b2 = item.h[0] == ob.item.h[0] && item.h.Length == ob.item.h.Length;
            if (b2 && item.h.Length == 2)
            {
                b2 = item.h[1] == ob.item.h[1];
            }
            bool b1 = item.mSubject.name == ob.item.mSubject.name;
            bool b3 = item.mSubject.room == ob.item.mSubject.room;
            bool b4 = item.mSubject.omitted == ob.item.mSubject.omitted;
            return a && b2 && b1 && b3 && b4;

            //Extension in Future if problems are appeareing
            //bool b5 = item.mSubject.ev == ob.item.mSubject.ev;
            //if (!b5 && item.mSubject.ev != null)
            //{
            //    bool b7 = item.mSubject.ev.Describtion == ob.item.mSubject.ev.Describtion;
            //    bool b8 = item.mSubject.ev.Hour == ob.item.mSubject.ev.Hour;
            //    b5 = b7 && b8;
            //}
            //bool b9 = item.mSubject.change == ob.item.mSubject.change;
            //if (!b9 && item.mSubject.change != null)
            //{
            //    bool b10 = item.mSubject.change.newRoom == ob.item.mSubject.change.newRoom;
            //    bool b11 = item.mSubject.change.newSubject == ob.item.mSubject.change.newSubject;
            //    b9 = b10 && b11;
            //}
            
            //return a && b2 && b1 && b3 && b4 && b5 && b9;
        }
    }
}