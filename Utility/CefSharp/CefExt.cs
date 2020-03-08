using System;
using System.Threading.Tasks;
using CefSharp;

namespace CefAutoPostToWp.Utility.CefSharp
{
    public static class CefExt
    {
        public static Task LoadPageAsync(this IWebBrowser browser, string address = null)
        {
            if (!string.IsNullOrEmpty(address) && !browser.IsBrowserInitialized)
                browser.WaitForBrowserInitialized().Wait();

            var tcs = new TaskCompletionSource<bool>();

            EventHandler<LoadingStateChangedEventArgs> handler = null;
            handler = (sender, args) =>
            {
                //Wait for while page to finish loading not just the first frame
                if (!args.IsLoading)
                {
                    browser.LoadingStateChanged -= handler;
                    tcs.TrySetResult(true);
                }
            };

            browser.LoadingStateChanged += handler;

            if (!string.IsNullOrEmpty(address)) browser.Load(address);
            return tcs.Task;
        }

        private static Task WaitForBrowserInitialized(this IWebBrowser browser)
        {
            var tcs = new TaskCompletionSource<bool>();

            EventHandler<LoadingStateChangedEventArgs> handler = null;
            handler = (sender, args) =>
            {
                //Wait for while page to finish loading not just the first frame
                if (!args.IsLoading)
                {
                    browser.LoadingStateChanged -= handler;
                    tcs.TrySetResult(true);
                }
            };

            browser.LoadingStateChanged += handler;
            return tcs.Task;
        }
    }
}