using Android.App;
using Android.OS;
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
using ScheduleApp.Handler;

namespace ScheduleApp.Activities
{
    [Activity(Label = "@string/app_name", Icon = "@drawable/icon", MainLauncher = true, Theme = "@style/Theme.Light.CenterAnimation")]
    public class SplashScreen : AppCompatActivity
    {
        public const int STARTUP_DELAY         = 300;
        public const int ITEM_DELAY            = 300;
        public const int ANIM_ITEM_DURATION    = 1000;
        private bool mFirstStart        = false;
        private bool mAnimationStarted  = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(Resource.Style.Theme_Light);
            Window.DecorView.SystemUiVisibility = StatusBarVisibility.Hidden;
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.splash);
            Button buttonU = FindViewById<Button>(Resource.Id.btn_choice1);
            buttonU.Click += ButtonU_Click;
        }
        protected override void OnStart()
        {
            base.OnStart();
            mFirstStart = DataHandler.GetConfig().IsEmpty();
        }

        private void ButtonU_Click(object sender, System.EventArgs e)
        {
            if (mFirstStart)
            {
                new Thread(new ThreadStart(delegate
                {
                    mFirstStart = !GetDataAsync();
                })).Start();
            }
            if (!mFirstStart)
            {
                Intent i = new Intent(this, typeof(Activities.TimetableSetupActivity));
                i.PutExtra("StartMain", true);
                StartActivity(i);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (mFirstStart)
            {
                new Thread(new ThreadStart(delegate
                {
                    mFirstStart = !GetDataAsync();
                })).Start();
            }
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            if (!hasFocus || mAnimationStarted)
                return;

            StartUpAnimation();
            base.OnWindowFocusChanged(hasFocus);
        }

        private void StartUpAnimation()
        {
            ViewGroup container;
            ImageView logoImageView = FindViewById<ImageView>(Resource.Id.img_logo); 
            mAnimationStarted       = true;

            if (mFirstStart)
            {
                container   = FindViewById<ViewGroup>(Resource.Id.container_first);
                mFirstStart = !GetDataAsync(true);
            }
            else
                container   = FindViewById<ViewGroup>(Resource.Id.container_norm);

            ViewCompat.Animate(logoImageView)
                .TranslationY(-220) //-250 / -160
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
                    RunOnUiThread(() => BuildAlertDialog(this));
                return false;
            }
            return true;
        }
        private void BuildAlertDialog(Context context)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetTitle("Keine Internetverbindung")
                .SetMessage("Um fortzufahren, muss eine Verbindung über WLAN oder Mobile Daten sichergestellt werden")
                .SetPositiveButton("Ok", (o, e) => { })
                .SetNeutralButton("Einstellungen", (o, e) =>
                {
                    Intent settings = new Intent(this, typeof(AdvancedPreferenceActivity));
                    StartActivity(settings);
                })
                .Create()
                .Show();
        }

        private DataHandler GetDataHandler()
        {
            return Task.Factory.StartNew(() => DataHandler.GetDataHandler()).Result;
        }

        public class RunInnerClassHelper : Java.Lang.Object, Java.Lang.IRunnable
        {
            private Activity mActivity;
            public RunInnerClassHelper(Activity activity)
            {
                mActivity = activity;
            }
            public void Run()
            {
                mActivity.StartActivity(new Intent(mActivity, typeof(Activities.MainActivity)));
                mActivity.Finish();
            }
        }
    }
}
