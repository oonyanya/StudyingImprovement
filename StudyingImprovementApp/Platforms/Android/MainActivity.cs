using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Activity;
using CommunityToolkit.Mvvm.Messaging;

namespace StudyingImprovement
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public override void OnActionModeStarted(ActionMode? mode)
        {
            var menu = mode.Menu;
            menu.Add("音声で読み上げる");
            menu.GetItem(0).SetOnMenuItemClickListener(new MyCreateContextMenuListener());
            mode.InvalidateContentRect();
            base.OnActionModeStarted(mode);
        }
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            OnBackPressedDispatcher.AddCallback(this, new BackPress(this));
        }
    }

    class BackPress : OnBackPressedCallback
    {
        private readonly Activity activity;
        private long backPressed;

        public BackPress(Activity activity) : base(true)
        {
            this.activity = activity;
        }

        public override void HandleOnBackPressed()
        {
            var mainpage = Microsoft.Maui.Controls.Application.Current?.MainPage;
            var navigation = mainpage?.Navigation;
            if(mainpage is not null)
            {
                var result = mainpage.SendBackButtonPressed();
                if (result == false && navigation is not null && navigation.NavigationStack.Count <= 1 && navigation.ModalStack.Count <= 0)
                {
                    const int delay = 2000;
                    if (backPressed + delay > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                    {
                        activity.FinishAndRemoveTask();
                        Process.KillProcess(Process.MyPid());
                    }
                    else
                    {
                        backPressed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    }
                }
            }
        }
    }

    internal class MyCreateContextMenuListener : Java.Lang.Object, Android.Views.IMenuItemOnMenuItemClickListener
    {
        public bool OnMenuItemClick(IMenuItem item)
        {
            WeakReferenceMessenger.Default.Send(Model.Message.SpeechToText);
            return true;
        }
    }

}
