using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Text;
using CommunityToolkit.Maui.Alerts;
using StudyingImprovement.Model;

namespace StudyingImprovement
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object? sender, EventArgs e)
        {
            var connectionProfile = Connectivity.Current.ConnectionProfiles;
            bool hasWifi = connectionProfile.Contains(ConnectionProfile.WiFi);
            if(hasWifi == false && Setting.Current.ForceDownloadMovie == false)
            {
                var toast = Toast.Make("WIFI is disabled. To play movie and sound, please enable WIFI.");
                toast.Show();
            }
        }

        protected override bool OnBackButtonPressed()
        {
#if ANDROID
            var androidWebView = WebView.Handler.PlatformView as Android.Webkit.WebView;

            if (androidWebView.CanGoBack())
            {
                androidWebView.GoBack();
                return true;
            }
            else
            {
                base.OnBackButtonPressed();
                return false;
            }
#else
            if (WebView.CanGoBack)
            {
                WebView.GoBack();
                return true;
            }
            else
            {
                base.OnBackButtonPressed();
                return false;
            }
#endif
        }

        protected override void OnHandlerChanged()
        {
#if ANDROID
        var androidWebView = WebView.Handler.PlatformView as Android.Webkit.WebView;

        // If this is not disabled then download links that open in a new tab won't work
        androidWebView.Settings.SetSupportMultipleWindows(false);

        androidWebView.SetWebViewClient(new Platforms.Android.MyWebViewClient());

        // Custom download listener for Android
        androidWebView.SetDownloadListener(new Platforms.Android.MyDownloadListener());
#endif
        }

    }

}
