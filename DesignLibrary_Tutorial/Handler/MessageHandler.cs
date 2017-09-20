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
        public DataHandler mDataHandler;
        public List<Card> mList;
        public event EventHandler OnDataChanged;

        [JsonConstructor]
        public MessageHandler(DataHandler dataHandler, List<Card> list)
        {
            mDataHandler = dataHandler;
            mList = list;
            mWeek = new Week[2];
        }

        public MessageHandler()
        {
            mDataHandler = DataHandler.GetDataHandler();
            mWeek = new Week[2];
            mWeek[0] = mDataHandler.GetDetailedWeek(0);
            mWeek[1] = mDataHandler.GetDetailedWeek(1);
            mList = Process();
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
            var tList = Process();
            bool t = false;
            if (mList.Count == tList.Count)
            {
                for (int i = 0; i < mList.Count; i++)
                {
                    if (!mList[i].Equals(tList[i]))
                    {
                        t = true;
                    }
                }
            }
            else t = true;
            Console.WriteLine(t);
            if ( mList != tList )
            {
                mList = tList;
                OnDataChanged(this, new EventArgs());
            }
        }

        public List<Card> Process()
        {
            List<Card> tList = new List<Card>();
            int[][] config = mDataHandler.mConfig.GetTableConf();
            for (int wp = 0; wp < mWeek.Length; wp++)
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
                    }
                }
            }
            return tList;
        }

        public string GetCurrentClass()
        {
            return mDataHandler.mConfig.GetClassName();
        }
    }
}