using System;
using System.Text.RegularExpressions;
using CefSharp;

namespace CefAutoPostToWp.Utility.CefSharp
{
    public class RequestHandler : IRequestHandler
    {
        public Func<string, string, bool> IsResponceSuccess { get; set; }

        public Action<string> ResponceSuccess { get; set; }

        bool IRequestHandler.OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame,
            IRequest request,
            bool isRedirect)
        {
            return false;
        }

        bool IRequestHandler.OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser, IFrame frame,
            string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
        {
            return false;
        }

        bool IRequestHandler.OnCertificateError(IWebBrowser browserControl, IBrowser browser, CefErrorCode errorCode,
            string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        {
            //NOTE: If you do not wish to implement this method returning false is the default behaviour
            // We also suggest you explicitly Dispose of the callback as it wraps an unmanaged resource.
            //callback.Dispose();
            //return false;

            //NOTE: When executing the callback in an async fashion need to check to see if it's disposed
            if (!callback.IsDisposed)
                using (callback)
                {
                    //To allow certificate
                    //callback.Continue(true);
                    //return true;
                }

            return false;
        }

        void IRequestHandler.OnPluginCrashed(IWebBrowser browserControl, IBrowser browser, string pluginPath)
        {
            // TODO: Add your own code here for handling scenarios where a plugin crashed, for one reason or another.
        }

        CefReturnValue IRequestHandler.OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame,
            IRequest request, IRequestCallback callback)
        {
            //NOTE: If you do not wish to implement this method returning false is the default behaviour
            // We also suggest you explicitly Dispose of the callback as it wraps an unmanaged resource.
            //callback.Dispose();
            //return false;
            //NOTE: When executing the callback in an async fashion need to check to see if it's disposed
            if (!callback.IsDisposed)
                using (callback)
                {
                    //if (request.Method == "POST")
                    //{
                    //    using (var postData = request.PostData)
                    //    {
                    //        var elements = postData.Elements;

                    //        var charSet = request.GetCharSet();

                    //        foreach (var element in elements)
                    //        {
                    //            if (element.Type == PostDataElementType.Bytes)
                    //            {
                    //                var body = element.GetBody(charSet);
                    //            }
                    //        }
                    //    }
                    //}
                    //var continueUrl = SiteFunctionManager.Instance.GetSiteCheckCodeUrl(request.Url.ToDomain());
                    if (Regex.IsMatch(request.Url, "\\S+(\\.jpg|\\.png|\\.gif)\\??\\S*")) return CefReturnValue.Cancel;
                    //Note to Redirect simply set the request Url
                    //if (request.Url.StartsWith("https://www.google.com", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    request.Url = "https://github.com/";
                    //}

                    //Callback in async fashion
                    //callback.Continue(true);
                    //return CefReturnValue.ContinueAsync;
                }

            return CefReturnValue.Continue;
        }

        bool IRequestHandler.GetAuthCredentials(IWebBrowser browserControl, IBrowser browser, IFrame frame,
            bool isProxy,
            string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            //NOTE: If you do not wish to implement this method returning false is the default behaviour
            // We also suggest you explicitly Dispose of the callback as it wraps an unmanaged resource.

            callback.Dispose();
            return false;
        }

        void IRequestHandler.OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser,
            CefTerminationStatus status)
        {
            // TODO: Add your own code here for handling scenarios where the Render Process terminated for one reason or another.
        }

        bool IRequestHandler.OnQuotaRequest(IWebBrowser browserControl, IBrowser browser, string originUrl,
            long newSize,
            IRequestCallback callback)
        {
            //NOTE: If you do not wish to implement this method returning false is the default behaviour
            // We also suggest you explicitly Dispose of the callback as it wraps an unmanaged resource.
            //callback.Dispose();
            //return false;

            //NOTE: When executing the callback in an async fashion need to check to see if it's disposed
            if (!callback.IsDisposed)
                using (callback)
                {
                    //Accept Request to raise Quota
                    //callback.Continue(true);
                    //return true;
                }

            return false;
        }

        void IRequestHandler.OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame,
            IRequest request, ref string newUrl)
        {
            //Example of how to redirect - need to check `newUrl` in the second pass
            //if (string.Equals(frame.GetUrl(), "https://www.google.com/", StringComparison.OrdinalIgnoreCase) && !newUrl.Contains("github"))
            //{
            //	newUrl = "https://github.com";
            //}
        }

        bool IRequestHandler.OnProtocolExecution(IWebBrowser browserControl, IBrowser browser, string url)
        {
            return url.StartsWith("mailto");
        }

        void IRequestHandler.OnRenderViewReady(IWebBrowser browserControl, IBrowser browser)
        {
        }


        bool IRequestHandler.OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame,
            IRequest request, IResponse response)
        {
            if (IsResponceSuccess != null)
            {
                string pData = null;
                if (request.Method == "POST")
                    using (var postData = request.PostData)
                    {
                        var elements = postData.Elements;
                        var charSet = request.GetCharSet();
                        foreach (var element in elements)
                            if (element.Type == PostDataElementType.Bytes)
                            {
                                var body = element.GetBody(charSet);
                                pData = body;
                                break;
                            }
                    }

                if (IsResponceSuccess(request.Url, pData)) ResponceSuccess(request.Url);
            }

            return false;
        }
    }
}