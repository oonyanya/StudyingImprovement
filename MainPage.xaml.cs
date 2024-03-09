using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Text;

namespace StudyingImprovement
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.WebView.Navigated += WebView_Navigated;
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
#if ANDROID
        var androidWebView = WebView.Handler.PlatformView as Android.Webkit.WebView;

        // If this is not disabled then download links that open in a new tab won't work
        androidWebView.Settings.SetSupportMultipleWindows(false);

        // Custom download listener for Android
        androidWebView.SetDownloadListener(new Platforms.Android.MyDownloadListener());
#endif
        }

        private async void WebView_Navigated(object? sender, WebNavigatedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("navigating {0} {1}", e.Url, e.NavigationEvent.ToString()));
            string css = await LoadAsset("Injection.css");
            string js = await LoadAsset("Injection.js");
            string inection_code = string.Format("function inject_css(){{var el=document.createElement('style');el.textContent = '{0}';document.head.append(el);}}if (document.readyState === 'complete'){{inject_css();}}else{{window.addEventListener('load', function(){{inject_css();}});}}{1}", css, js);
            await this.WebView.EvaluateJavaScriptAsync(inection_code);
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

    }

}
