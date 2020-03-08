using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CefAutoPostToWp.Utility
{
    public static class Extentions
    {
        public static void InvokeIfRequired(this Control ctl, Action action, bool isSync)
        {
            if (ctl.InvokeRequired)
            {
                if (isSync)
                    ctl.Invoke(new Action(action));
                else
                    ctl.BeginInvoke(new Action(action));
            }
            else
            {
                action();
            }
        }

        public static string FormatWith(this string format, params object[] args)
        {
            if (format == null || args == null) throw new ArgumentNullException(format == null ? "format" : "args");
            var capacity = format.Length + args.Where(a => a != null).Select(p => p.ToString()).Sum(p => p.Length);

            var stringBuilder = new StringBuilder(capacity);
            stringBuilder.AppendFormat(format, args);
            return stringBuilder.ToString();
        }

        /// <summary>
        ///     替换文本中\r \n 为<br>
        /// </summary>
        /// <param name="sourceStr"></param>
        /// <returns></returns>
        public static string RemoveLineBreak(this string sourceStr)
        {
            return Regex.Replace(sourceStr, "\\s{2,}", " ");
        }
    }
}