using System;
using System.Threading;
using System.Windows.Forms;
using CefAutoPostToWp.Forms;
using CefAutoPostToWp.Utility;
using CefAutoPostToWp.Utility.CefSharp;
using CefSharp;
using log4net.Config;

namespace CefAutoPostToWp
{
    internal static class Program
    {
        /// <summary>
        ///     应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main()
        {
            XmlConfigurator.Configure();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            CefHelper.Init(true, true);
            Application.Run(new FormMain());
            Cef.Shutdown();
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Logger.Error(e.Exception);
        }
    }
}