using CefSharp;
using CefSharp.WinForms;

namespace CefAutoPostToWp.Utility.CefSharp
{
    public class CefBrowser : ChromiumWebBrowser
    {
        public CefBrowser() : base("about:blank")
        {
            RequestHandler = new RequestHandler();
        }

        public CommandType CommandType { get; set; } = CommandType.None;

        public RequestHandler ReqHandler => (RequestHandler) RequestHandler;

        public string Html => this.GetSourceAsync().Result;
    }
}