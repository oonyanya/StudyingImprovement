using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Text;
using CommunityToolkit.Maui.Alerts;
using StudyingImprovement.Model;
using Microsoft.Maui.Controls;
using System.Text.RegularExpressions;
using Microsoft.Maui.Media;
using System.Threading;
using CommunityToolkit.Mvvm.Messaging;

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
            this.WebView.JSInvokeTarget = new TextSpechListener(this.WebView);
            BindingContext = Setting.Current;
        }

        //TODO:結合度が高すぎるのでよくない
        public class TextSpechListener
        {
            HybridWebView.HybridWebView webView;
            CancellationTokenSource cancellationTokenSource;
            public TextSpechListener(HybridWebView.HybridWebView web)
            {
                this.webView = web;
                WeakReferenceMessenger.Default.Register<TextSpechListener, Message>(this, (s, e) =>
                {
                    if (e.Method == Message.SpeechToText.Method)
                        this.onStartTextToSpeech();
                });
            }
            public void onStartTextToSpeech()
            {
                Setting.Current.IsShowTextSpeech = true;
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await this.webView.InvokeJsMethodAsync("onStartTextToSpeech");
                });
            }

            public async void speakText(string text)
            {
                IEnumerable<Locale> locales = await TextToSpeech.Default.GetLocalesAsync();

                cancellationTokenSource = new CancellationTokenSource();

                System.Diagnostics.Debug.WriteLine("Speaing now:" + text);

                if(!string.IsNullOrEmpty(text))
                    await TextToSpeech.Default.SpeakAsync(text, null, cancellationTokenSource.Token);

                MainThread.BeginInvokeOnMainThread(async () => {
                    await this.webView.InvokeJsMethodAsync("onSpeakReadFinish");
                });
            }
            public void pauseSpeech()
            {
                if (cancellationTokenSource?.IsCancellationRequested ?? true)
                    return;

                cancellationTokenSource.Cancel();

                MainThread.BeginInvokeOnMainThread(async () => {
                    await this.webView.InvokeJsMethodAsync("onSpeakReadPause");
                });
            }

            public void cancelSpeech()
            {
                Setting.Current.IsShowTextSpeech = false;

                if (cancellationTokenSource?.IsCancellationRequested ?? true)
                    return;

                cancellationTokenSource.Cancel();

                MainThread.BeginInvokeOnMainThread(async () => {
                    await this.webView.InvokeJsMethodAsync("onSpeakReadCancel");
                });
            }
            public void playSpeech()
            {
                MainThread.BeginInvokeOnMainThread(async () => {
                    await this.webView.InvokeJsMethodAsync("onSpeakPlayStart");
                });
            }
            public void prevSpeech()
            {
                cancellationTokenSource.Cancel();

                System.Diagnostics.Debug.WriteLine("前へ移動する");

                MainThread.BeginInvokeOnMainThread(async () => {
                    await this.webView.InvokeJsMethodAsync("onSpeakPrev");
                });
            }
            public void nextSpeech()
            {
                cancellationTokenSource.Cancel();

                System.Diagnostics.Debug.WriteLine("次へ進む");

                MainThread.BeginInvokeOnMainThread(async () => {
                    await this.webView.InvokeJsMethodAsync("onSpeakNext");
                });
            }
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
            this.ProgressBar.IsVisible = true;
            this.ProgressBar.Progress = 0.1;
            this.ProgressBar.ProgressTo(0.8, 500, Easing.Linear);
        }

        private async void WebView_Navigated(object? sender, WebNavigatedEventArgs e)
        {
            if(e.Result == WebNavigationResult.Success)
            {
                string css = await LoadAsset("Injection.css");
                string js = await LoadAsset("Injection.js");
                string inection_code = string.Format("function inject_file(filename){{var el=document.createElement('script');el.setAttribute('src',filename);document.head.append(el);}}function inject_css(){{var el=document.createElement('style');el.textContent = '{0}';document.head.append(el);}}inject_file('https://0.0.0.0/_hwv/HybridWebView.js');if (document.readyState === 'complete'){{inject_css();}}else{{window.addEventListener('load', function(){{inject_css();}});}}{1}", css, js); ;
                await WebView.EvaluateJavaScriptAsync(inection_code);
                this.ProgressBar.Progress = 1;
                this.ProgressBar.IsVisible = false;
            }
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
            Setting.Current.IsShowTextSpeech = false;
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

        private void Button_Pause_Clicked(object sender, EventArgs e)
        {
            var textSpeech = (TextSpechListener)this.WebView.JSInvokeTarget;
            textSpeech.pauseSpeech();
        }
        private void Button_Stop_Clicked(object sender, EventArgs e)
        {
            var textSpeech = (TextSpechListener)this.WebView.JSInvokeTarget;
            textSpeech.cancelSpeech();
            Setting.Current.IsShowTextSpeech = false;
        }

        private void Button_Play_Clicked(object sender, EventArgs e)
        {
            var textSpeech = (TextSpechListener)this.WebView.JSInvokeTarget;
            textSpeech.playSpeech();
        }
        private void Button_Prev_Clicked(object sender, EventArgs e)
        {
            var textSpeech = (TextSpechListener)this.WebView.JSInvokeTarget;
            textSpeech.prevSpeech();
        }
        private void Button_Next_Clicked(object sender, EventArgs e)
        {
            var textSpeech = (TextSpechListener)this.WebView.JSInvokeTarget;
            textSpeech.nextSpeech();
        }
    }

}
