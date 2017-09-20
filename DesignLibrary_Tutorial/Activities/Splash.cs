using Newtonsoft.Json;
using Android.App;
using Android.Content;
using Android.OS;
using AppTestProzesse.Header;
using DesignLibrary_Tutorial.Handler;
using System.Threading.Tasks;

namespace DesignLibrary_Tutorial.Activities
{
    [Activity(Label = "@string/app_name", Icon = "@drawable/icon", MainLauncher = true, NoHistory = true, Theme = "@style/Theme.Splash")]
    public class Splash : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Init Shared Preferences
            //Datahandler
            ISharedPreferences preferences = Application.GetSharedPreferences("DataHandler", FileCreationMode.Private);
            DataHandler dataHandler;
            string dataSource = preferences.GetString("mData", string.Empty);
            if (dataSource == string.Empty)
            {
                dataHandler = new DataHandler();
                preferences.Edit().PutString("mData", JsonConvert.SerializeObject(dataHandler)).Apply();
            }
            else
            {
                dataHandler = JsonConvert.DeserializeObject<DataHandler>(dataSource);
            }

            //Start first Activity
            if (dataHandler.mConfig.IsEmpty())
            {
                //Activities.TimetableWeekActivity
                StartActivity(new Intent(this, typeof(Activities.TimetableSetupActivity)));
                //StartActivity(new Intent(this, typeof(Activities.MainActivity)));
            }
            else
            {
                //Activities.TimetableWeekActivity
                StartActivity(new Intent(this, typeof(Activities.MainActivity)));
            }
        }
    }
}