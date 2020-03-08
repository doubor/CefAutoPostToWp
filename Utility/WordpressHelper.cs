using System.Text;
using System.Text.RegularExpressions;

namespace CefAutoPostToWp.Utility
{
    public class WordPressHelper
    {
        public const string StrSingle = "'";
        public const string StrDouble = "\"";
        public const string XpathEditorLoadCompleted = "//div[@id='mceu_28']";
        public const string FlagBr = "<FlagBr>";
        public const string Str00 = "00";
        public const string Str11 = "\\\\";
        
        private static string _jQueryTemplate;
        public static readonly Regex RegRnt = new Regex(@"\r\n[ \t]*", RegexOptions.Compiled);
        public static readonly Regex RegNn = new Regex(@"\n[\n]*", RegexOptions.Compiled);
        public static readonly Regex RegLine = new Regex("(?<!\\\\)\\\\(?!\\\\|\\\")", RegexOptions.Compiled);

        public static readonly Regex RegSitePostId = new Regex(@"post.php\?post=(?<SitePostId>\d+)&action=edit",
            RegexOptions.Compiled);

        public static string JQueryTemplate
        {
            get
            {
                if (_jQueryTemplate == null) _jQueryTemplate = GetJQueryTemplate();
                return _jQueryTemplate;
            }
        }

        /// <summary>
        ///     取消已选中分类，重新选择，适配编辑文章
        /// </summary>
        /// <returns></returns>
        private static string GetJQueryTemplate()
        {
            var sb = new StringBuilder();
            sb.Append("jQuery('#title').val('{0}');jQuery('#post_name').val('{9}');");
            sb.Append(
                "var content='{1}';content=content.replace(/" + FlagBr +
                "/g,'\\r\\n'); jQuery('#content-tmce').click();jQuery(document.getElementById('content_ifr').contentWindow.document).find('#tinymce').html(content);");
            //"jQuery('#content-html').click();jQuery('#content').val('{1}');");
            sb.Append(
                "jQuery('#categorychecklist input[checked=checked]').each(function(){{jQuery(this).click();}});{2}");
            sb.Append("jQuery('#new-tag-post_tag').val('{3}');");
            sb.Append("jQuery('input.tagadd').click();");
            sb.Append("jQuery('a.edit-timestamp.hide-if-no-js').click();");
            sb.Append(
                "jQuery('#mm').val('{4}');jQuery('#jj').val('{5}');jQuery('#aa').val('{6}');jQuery('#hh').val('{7}');jQuery('#mn').val('{8}');");
            sb.Append("jQuery('a.save-timestamp').click();");
            sb.Append("jQuery('#publish').click();");
            return sb.ToString();
        }
    }
}