using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using log4net;

namespace CefAutoPostToWp.Utility
{
    public class Logger
    {
        private static readonly bool isWeb = HttpContext.Current != null;

        private static readonly ILog logger = LogManager.GetLogger("FileLogger");

        private static readonly List<string> _skipHttpHeaders = new List<string>
        {
            "Cache-Control",
            "Connection",
            "Accept",
            "Accept-Encoding",
            "Accept-Language",
            "Accept-Charset",
            "ASP.NET_SessionId"
        };

        public static string GetAllIPAddress()
        {
            var ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ip)) ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            return ip;
        }

        private static string GetRequesterInfo()
        {
            // 增加记录服务器名
            var serverInfo = new StringBuilder();
            serverInfo.AppendLine();
            serverInfo.AppendFormat("WHO: {0}", Dns.GetHostName());
            serverInfo.AppendLine();

            // 如果是web请求，则记录当前RawUrl
            var ctx = HttpContext.Current;
            if (ctx != null
                && ctx.Request != null
                && !string.IsNullOrEmpty(ctx.Request.RawUrl))
            {
                serverInfo.AppendLine("HTTP HEADER: ------------------------------- ");
                serverInfo.AppendLine(string.Format("CLIENT IP: {0}", GetAllIPAddress()));
                foreach (string key in ctx.Request.Headers.Keys)
                    if (!_skipHttpHeaders.Contains(key))
                        serverInfo.AppendLine(string.Format("{0}: {1}", key, ctx.Request.Headers[key]));
                serverInfo.AppendLine(string.Format("URL: {0}", ctx.Request.RawUrl));
                serverInfo.AppendLine("HTTP HEADER: ------------------------------- ");
                serverInfo.AppendLine(string.Empty);
            }

            return serverInfo.ToString();
        }

        public static void Error(Exception ex)
        {
            logger.Error(isWeb ? GetRequesterInfo() : string.Empty, ex);
        }

        public static void Info(string message)
        {
            logger.Info(string.Format("{0}{1}", isWeb ? GetRequesterInfo() : string.Empty, message));
        }

        public static void Log(string message)
        {
            logger.Info(string.Format("{0}{1}", isWeb ? GetRequesterInfo() : string.Empty, message));
        }
    }
}