using Android.Content;
using Android.Views;
using Android.Webkit;
using Java.Interop;
using Microsoft.Maui.Controls;
using AWebView = Android.Webkit.WebView;

namespace HybridWebView
{
    partial class HybridWebView
    {
        private static readonly string AppHostAddress = "0.0.0.0";

        /// <summary>
        /// Gets the application's base URI. Defaults to <c>https://0.0.0.0/</c>
        /// </summary>
        private static readonly string AppOrigin = $"https://{AppHostAddress}/";

        internal static readonly Uri AppOriginUri = new(AppOrigin);

        private HybridWebViewJavaScriptInterface? _javaScriptInterface;

        private AWebView PlatformWebView => (AWebView)Handler!.PlatformView!;

        private partial Task InitializeHybridWebView()
        {
            // Note that this is a per-app setting and not per-control, so if you enable
            // this, it is enabled for all Android WebViews in the app.
            AWebView.SetWebContentsDebuggingEnabled(enabled: EnableWebDevTools);

            PlatformWebView.Settings.JavaScriptEnabled = true;

            _javaScriptInterface = new HybridWebViewJavaScriptInterface(this);
            PlatformWebView.AddJavascriptInterface(_javaScriptInterface, "hybridWebViewHost");

            // If this is not disabled then download links that open in a new tab won't work
            PlatformWebView.Settings.SetSupportMultipleWindows(false);

            // Custom download listener for Android
            PlatformWebView.SetDownloadListener(new MyDownloadListener());

            PlatformWebView.Settings.CacheMode = CacheModes.Normal;

            PlatformWebView.Settings.DatabaseEnabled = true;

            PlatformWebView.Settings.MixedContentMode = MixedContentHandling.CompatibilityMode;

            return Task.CompletedTask;
        }

        private partial void NavigateCore(string url)
        {
            if(url == "/")
                PlatformWebView.LoadUrl(new Uri(AppOriginUri, url).ToString());
            else
                PlatformWebView.LoadUrl(url);
        }

        private sealed class HybridWebViewJavaScriptInterface : Java.Lang.Object
        {
            private readonly HybridWebView _hybridWebView;

            public HybridWebViewJavaScriptInterface(HybridWebView hybridWebView)
            {
                _hybridWebView = hybridWebView;
            }

            [JavascriptInterface]
            [Export("sendMessage")]
            public void SendMessage(string message)
            {
                _hybridWebView.OnMessageReceived(message);
            }
        }
    }
}
