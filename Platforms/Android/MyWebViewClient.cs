using Android.App;
using Android.Content;
using Android.Webkit;
using Android.Widget;
using System.Text.RegularExpressions;
using Microsoft.Maui.Networking;
using Java.IO;
using System.Text;
using Android.Graphics;
using StudyingImprovement.Model;

namespace StudyingImprovement.Platforms.Android
{
    internal class MyWebViewClient : WebViewClient
    {
        public override WebResourceResponse? ShouldInterceptRequest(global::Android.Webkit.WebView? view, IWebResourceRequest? request)
        {
            var connectionProfile = Connectivity.Current.ConnectionProfiles;
            bool hasWifi =  connectionProfile.Contains(ConnectionProfile.WiFi);
            Regex regex = new Regex(".+\\.(mp3|mp4|ts)"); 
            if (hasWifi == false && Setting.Current.ForceDownloadMovie == false && regex.IsMatch(request.Url.Path))
            {
                System.Diagnostics.Debug.WriteLine("blocked:" + request.Url.Path);
                //ストリーミング動画をブロックする
                var bytes = System.Text.Encoding.ASCII.GetBytes("<?xml\r\nversion=\"1.0\" encoding=\"UTF-8\"?><Error><Code>AccessDenied</Code><Message>AccessDenied.Please enable WIFI.</Message></Error>");
                var stream = new MemoryStream(bytes);
                var response = new WebResourceResponse("application/xml", "UTF-8", stream);
                response.SetStatusCodeAndReasonPhrase(404, "NOT FOUND");
                response.ResponseHeaders = new Dictionary<string, string>();
                response.ResponseHeaders.Add("Access-Control-Allow-Origin", "*");
                return response; 
            }
            else
            {
                return base.ShouldInterceptRequest(view, request);
            }
        }

        public override async void OnPageStarted(global::Android.Webkit.WebView? view, string? url, Bitmap? favicon)
        {
            string css = await LoadAsset("Injection.css");
            string js = await LoadAsset("Injection.js");
            string inection_code = string.Format("function inject_css(){{var el=document.createElement('style');el.textContent = '{0}';document.head.append(el);}}if (document.readyState === 'complete'){{inject_css();}}else{{window.addEventListener('load', function(){{inject_css();}});}}{1}", css, js);
            view.EvaluateJavascript(inection_code, null);
            base.OnPageStarted(view, url, favicon);
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
