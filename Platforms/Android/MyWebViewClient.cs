using Android.App;
using Android.Content;
using Android.Webkit;
using Android.Widget;
using System.Text.RegularExpressions;
using Microsoft.Maui.Networking;
using Java.IO;

namespace StudyingImprovement.Platforms.Android
{
    internal class MyWebViewClient : WebViewClient
    {
        public override WebResourceResponse? ShouldInterceptRequest(global::Android.Webkit.WebView? view, IWebResourceRequest? request)
        {
            var connectionProfile = Connectivity.Current.ConnectionProfiles;
            bool hasWifi =  connectionProfile.Contains(ConnectionProfile.WiFi);
            Regex regex = new Regex(".+\\.(mp3|mp4|ts)"); 
            if (hasWifi == false && regex.IsMatch(request.Url.Path))
            {
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
    }
}
