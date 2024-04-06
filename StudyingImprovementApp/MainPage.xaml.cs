using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Text;
using CommunityToolkit.Maui.Alerts;
using StudyingImprovement.Model;
using Microsoft.Maui.Controls;
using System.Text.RegularExpressions;

namespace StudyingImprovement
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.Loaded += MainPage_Loaded;
            this.WebView.Navigating += WebView_Navigating;
            this.WebView.Navigated += WebView_Navigated;
            this.WebView.RequestReceived += WebView_RequestReceived;
        }

        private Task WebView_RequestReceived(HybridWebView.HybridWebViewProxyEventArgs arg)
        {
            var connectionProfile = Connectivity.Current.ConnectionProfiles;
            bool hasWifi = connectionProfile.Contains(ConnectionProfile.WiFi);
            Regex regex = new Regex(".+\\.(mp3|mp4|ts)");
            if (hasWifi == false && Setting.Current.ForceDownloadMovie == false && regex.IsMatch(arg.Url))
            {
                System.Diagnostics.Debug.WriteLine("blocked:" + arg.Url);
                //ストリーミング動画をブロックする
                var bytes = System.Text.Encoding.ASCII.GetBytes("<?xml\r\nversion=\"1.0\" encoding=\"UTF-8\"?><Error><Code>AccessDenied</Code><Message>AccessDenied.Please enable WIFI.</Message></Error>");
                var stream = new MemoryStream(bytes);
                arg.ResponseContentType = "application/xml";
                arg.ResponseStream = stream;
            }
            return Task.CompletedTask;
        }
        private void WebView_Navigating(object? sender, WebNavigatingEventArgs e)
        {
            this.ActivityIndicator.IsRunning = true;
        }

        private async void WebView_Navigated(object? sender, WebNavigatedEventArgs e)
        {
            string css = await LoadAsset("Injection.css");
            string js = await LoadAsset("Injection.js");
            string inection_code = string.Format("function inject_css(){{var el=document.createElement('style');el.textContent = '{0}';document.head.append(el);}}if (document.readyState === 'complete'){{inject_css();}}else{{window.addEventListener('load', function(){{inject_css();}});}}{1}", css, js);
            await WebView.EvaluateJavaScriptAsync(inection_code);

            this.ActivityIndicator.IsRunning = false;
        }

        private async Task<string> LoadAsset(string name)
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(name);
            using var reader = new StreamReader(stream);
            StringBuilder sb = new StringBuilder();
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                sb.Append(line.Trim());
            }
            var contents = sb.ToString();
            return contents;
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
        }

        protected override void OnHandlerChanged()
        {
        }

    }

}
