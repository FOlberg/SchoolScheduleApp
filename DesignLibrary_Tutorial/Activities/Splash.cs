using Newtonsoft.Json;
using Android.App;
using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using AppTestProzesse.Header;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;
using DesignLibrary_Tutorial.Handler;
using System.Threading.Tasks;

namespace DesignLibrary_Tutorial.Activities
{
    [Activity(Label = "@string/app_name", Icon = "@drawable/icon", MainLauncher = true, NoHistory = true, Theme = "@style/Theme.Splash")]
    public class Splash : Activity
    {
        DataHandler mDataHandler;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //Datahandler
            mDataHandler = GetDataHandler();

            //Start first Activity
            if (mDataHandler.mConfig.IsEmpty())
            {
                //Activities.TimetableWeekActivity
                Intent i = new Intent(this, typeof(Activities.TimetableSetupActivity));
                i.PutExtra("StartMain", true);
                StartActivity(i);
                //StartActivity(new Intent(this, typeof(Activities.MainActivity)));
            }
            else
            {
                //Activities.TimetableWeekActivity
                StartActivity(new Intent(this, typeof(Activities.MainActivity)));
            }
        }

        private DataHandler GetDataHandler()
        {
            return Task.Factory.StartNew(() => GetData()).Result;
        }

        private DataHandler GetData()
        {
            ISharedPreferences preferences = Application.GetSharedPreferences("DataHandler", FileCreationMode.Private);
            string dataSource = preferences.GetString("mData", string.Empty);
            if (dataSource == string.Empty)
            {
                DataHandler dataHandler = new DataHandler();
                preferences.Edit().PutString("mData", JsonConvert.SerializeObject(dataHandler)).Apply();
                return dataHandler;
            }
            return JsonConvert.DeserializeObject<DataHandler>(dataSource);
        }
    }
}