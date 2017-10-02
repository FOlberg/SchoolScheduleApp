using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AppTestProzesse.Header;
using DesignLibrary_Tutorial.Helpers;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DesignLibrary_Tutorial.Handler
{
    public class MessageHandler
    {
        Week[] mWeek;
        //[JsonProperty]
        List<LData> testList;
        //[JsonProperty]
        List<LData> testListo;
        public DataHandler mDataHandler;
        public List<Card> mList;
        public event EventHandler OnDataChanged;

        [JsonConstructor]
        public MessageHandler(DataHandler dataHandler, List<Card> list)
        {
            mDataHandler = dataHandler;
            mList = list;
            testList = new List<LData>();
            testListo = new List<LData>();
            mWeek = new Week[2];
            DeleteOutdatedData();
        }

        public MessageHandler()
        {
            mDataHandler = DataHandler.GetDataHandler();
            mWeek = new Week[2];
            testList = new List<LData>();
            testListo = new List<LData>();
            mWeek[0] = mDataHandler.GetDetailedWeek(0);
            mWeek[1] = mDataHandler.GetDetailedWeek(1);
            mList = Process();
            DeleteOutdatedData();
        }

        ~MessageHandler()
        {
            SaveMsgHandler(this);
        }

        public Task UpdateAsync()
        {
            return Task.Factory.StartNew(() => Update());
        }

        public void Update()
        {
            mDataHandler.LoadCfg();
            mWeek[0] = mDataHandler.GetDetailedWeek(0, true);
            mWeek[1] = mDataHandler.GetDetailedWeek(1, true);
            mList = Process();
            SaveMsgHandler(this);
            Check();
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
                }
            }
        }

        public List<Card> Process()
        {
            List<Card> tList = new List<Card>();
            int[][] config = mDataHandler.mConfig.GetTableConf();

            if (testList != null)
            {
                testListo = testList;
                testList.Clear();
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
                            if (config[day][hour] != -1)
                            {
                                if (mWeek[wp].week[day].list[hour] != null && mWeek[wp].week[day].list[hour].Length >= config[day][hour])
                                {
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
                                }
                            }
                            else if (mWeek[wp].mEvents.Count > 0 && mWeek[wp].week[day].list[hour] != null && mWeek[wp].week[day].list[hour][0].ev != null)
                            {
                                Subject temp = mWeek[wp].week[day].list[hour][0];
                                tCardList.Add(new CardList(temp, new Hours[] { temp.ev.Hour, temp.ev.Number }));
                            }
                        }
                        if (tCardList.Count > 0)
                        {
                            tList.Add(new Card(tCardList, mWeek[wp].tMon.AddDays(day)));
                            foreach (var item in tCardList)
                            {
                                testList.Add(new LData(mWeek[wp].tMon.AddDays(day).Date, item));
                            }
                        }
                    }
                }
            }
            return tList;
        }

        private void Check()
        {
            OnDataChanged(this, new EventArgs());
            if (testListo == testList)
            {
                var list = testList.Except(testListo).ToList();
                if (list.Count > 0)
                {
                    //list are new messages that will be displayed in refresh or as notification
                    bool b = true;
                    OnDataChanged(this, new EventArgs());
                }
                //if its the other way -> only views have to be deleted
            }
        }

        public string GetCurrentClass()
        {
            return mDataHandler.mConfig.GetClassName();
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
            try
            {
                if (messageHandler.GetType() == typeof(MessageHandler))
                {
                    Application.Context.GetSharedPreferences("Dashboard", FileCreationMode.Private).Edit().PutString("MsgHandler", JsonConvert.SerializeObject(messageHandler)).Apply();
                }

            }
            catch (Exception) { }
            //log
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
    }
}