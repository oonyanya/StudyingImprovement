using Android.App;
using Android.Content.PM;
using Android.Views;
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
