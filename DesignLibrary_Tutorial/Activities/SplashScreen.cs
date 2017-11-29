using Android.App;
using Android.OS;
using Helper.Header;
using System.Threading.Tasks;
using Android.Support.V7.App;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Android.Views.Animations;
using Android.Content;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using System;
using System.Threading;

namespace ScheduleApp.Activities
{
    [Activity(Label = "@string/app_name", Icon = "@drawable/icon", MainLauncher = true, NoHistory = true, Theme = "@style/Theme.DesignDemo.CenterAnimation")]
    public class SplashScreen : AppCompatActivity
    {
        public static int STARTUP_DELAY = 300;
        public static int ANIM_ITEM_DURATION = 1000;
        public static int ITEM_DELAY = 300;
        bool firstStartUp = false;

        private bool animationStarted = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(Resource.Style.Theme_DesignDemo);
            Window.DecorView.SystemUiVisibility = StatusBarVisibility.Hidden;
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.splash);

            Button buttonU = FindViewById<Button>(Resource.Id.btn_choice1);
            buttonU.Click += ButtonU_Click;
        }
        protected override void OnStart()
        {
            base.OnStart();
            firstStartUp = DataHandler.GetConfig().IsEmpty();
        }

        private void ButtonU_Click(object sender, System.EventArgs e)
        {
            if (firstStartUp)
            {
                new Thread(new ThreadStart(delegate
                {
                    firstStartUp = !GetDataAsync();
                })).Start();
            }
            if (!firstStartUp)
            {
                Intent i = new Intent(this, typeof(Activities.TimetableSetupActivity));
                i.PutExtra("StartMain", true);
                StartActivity(i);
            }
        }



        public override void OnWindowFocusChanged(bool hasFocus)
        {
            if (!hasFocus || animationStarted)
            {
                return;
            }
            StartUpAnimation();
            base.OnWindowFocusChanged(hasFocus);
        }

        private void StartUpAnimation()
        {
            ImageView logoImageView = FindViewById<ImageView>(Resource.Id.img_logo);
            ViewGroup container;
            animationStarted = true;

            if (firstStartUp)
            {
                //container_norm
                container = FindViewById<ViewGroup>(Resource.Id.container_first);
                firstStartUp = !GetDataAsync(true);
            }
            else
            {
                container = FindViewById<ViewGroup>(Resource.Id.container_norm);
            }

            ViewCompat.Animate(logoImageView)
                .TranslationY(-180) //-250 / -160
                .SetStartDelay(STARTUP_DELAY)
                .SetDuration(ANIM_ITEM_DURATION)
                .SetInterpolator(new DecelerateInterpolator(1.2f))
                .Start();

            for (int i = 0; i < container.ChildCount; i++)
            {
                View v = container.GetChildAt(i);
                ViewPropertyAnimatorCompat viewAnimator;

                if (v is Button)
                {
                    viewAnimator = ViewCompat.Animate(v)
                        .ScaleY(1)
                        .ScaleX(1)
                        .SetStartDelay((ITEM_DELAY * i) + 500)
                        .SetDuration(500);
                }
                else if (v is ProgressBar)
                {
                    v.Visibility = ViewStates.Visible;
                    viewAnimator = ViewCompat.Animate(v)
                            .ScaleY(1)
                            .ScaleX(1)
                            .SetStartDelay((ITEM_DELAY * i) + 500)
                            .SetDuration(500);
                    viewAnimator.WithEndAction(new RunInnerClassHelper(this));
                }
                else
                {
                    viewAnimator = ViewCompat.Animate(v)
                            .TranslationY(50)
                            .Alpha(1)
                            .SetStartDelay((ITEM_DELAY * i) + 500)
                            .SetDuration(1000);
                }

                viewAnimator.SetInterpolator(new DecelerateInterpolator()).Start();
            }
        }

        private bool GetDataAsync(bool start = false)
        {
            try
            {
                GetDataHandler().GetClasses();
            }
            catch (Exception)
            {
                if (!start)
                    RunOnUiThread(() => BuildAlertDialog(this).Show());
                    //Toast.MakeText(this, "Netzwerkfehler!", ToastLength.Short).Show();
                return false;
            }
            return true;
        }
        private AlertDialog BuildAlertDialog(Context context)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetTitle("Keine Internetverbindung")
                .SetMessage("Um fortzufahren, muss eine Verbindung mittels WLAN oder Mobile Daten sichergestellt werden")
                .SetPositiveButton("Ok", (o, e) => {});
            return builder.Create();
        }

        private DataHandler GetDataHandler()
        {
            //return await Task.Factory.StartNew(() => DataHandler.GetDataHandler());
            return Task.Factory.StartNew(() => DataHandler.GetDataHandler()).Result;
        }

        public class RunInnerClassHelper : Java.Lang.Object, Java.Lang.IRunnable
        {
            private Context mContext;
            public RunInnerClassHelper(Context context)
            {
                mContext = context;
            }
            public void Run()
            {
                mContext.StartActivity(new Intent(mContext, typeof(Activities.MainActivity)));
            }
        }
    }
}
