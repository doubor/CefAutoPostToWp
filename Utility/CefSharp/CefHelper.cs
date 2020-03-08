using System;
using CefSharp;

namespace CefAutoPostToWp.Utility.CefSharp
{
    public class CefHelper
    {
        private static ICookieManager _cookieManager;

        public static void Init(bool osr, bool multiThreadedMessageLoop)
        {
            var settings = new CefSettings();
            settings.MultiThreadedMessageLoop = multiThreadedMessageLoop;

            if (osr) settings.WindowlessRenderingEnabled = true;
            settings.LogSeverity = LogSeverity.Verbose;
            settings.SetOffScreenRenderingBestPerformanceArgs();
            settings.BrowserSubprocessPath = AppDomain.CurrentDomain.BaseDirectory + "\\CefSharp.BrowserSubprocess.exe";
            //settings.EnableFocusedNodeChanged = true;
            settings.CachePath = "cache";
            settings.CefCommandLineArgs.Add("ppapi-flash-path",
                AppDomain.CurrentDomain.BaseDirectory + "\\Plugins\\pepflash\\pepflashplayer.dll");
            //Load a specific pepper flash version (Step 1 of 2)
            //settings.CefCommandLineArgs.Add("ppapi-flash-version", "18.0.0.209"); //Load a specific pepper flash version (Step 2 of 2)

            Cef.OnContextInitialized = delegate
            {
                _cookieManager = Cef.GetGlobalCookieManager();
                _cookieManager.SetStoragePath("cache", true);
            };
            if (!Cef.Initialize(settings, true, true)) throw new Exception("Unable to Initialize Cef");
        }
    }
}