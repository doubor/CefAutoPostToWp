using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CefAutoPostToWp.Models;
using CefAutoPostToWp.Sample;
using CefAutoPostToWp.Utility;
using CefAutoPostToWp.Utility.CefSharp;
using CefSharp;
using Timer = System.Threading.Timer;
using XHtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace CefAutoPostToWp.Forms
{
    public partial class FormMain : Form
    {
        private readonly SiteInfo _currentSiteInfo;
        private readonly TimeSpan _timeOutTimeSpan = TimeSpan.FromMinutes(5);
        private Action _actionOpenCompleted;
        private CefBrowser _browser;
        private ArticleDraft _currentArticleDraft;

        private string _currentEditUrl;
        private bool _isLoggedIn;
        private bool _isWaiting4OpenAddOrEdit;
        private WebBrowserState _state = WebBrowserState.None;

        private Timer _timer;
        private Timer _timerWait4OpenPageCompleted;

        public FormMain()
        {
            InitializeComponent();
            CreateBrowser();
            _currentSiteInfo = new SiteInfoExample();
        }

        private void FormSubject_Load(object sender, EventArgs e)
        {
            _timer = new Timer(timer_Tick, null, 2000, 2000);
            _timerWait4OpenPageCompleted = new Timer(timerWait_Tick, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void CreateBrowser()
        {
            _browser = new CefBrowser {Dock = DockStyle.Fill};
            _browser.FrameLoadEnd += browser_FrameLoadEnd;
            _browser.StatusMessage += _browser_StatusMessage;
            panel_browser.Controls.Add(_browser);
        }

        public void _browser_StatusMessage(object sender, StatusMessageEventArgs e)
        {
            toolStripStatusLabel_Status.Text = e.Value;
        }

        private void PublishAticle()
        {
            _currentArticleDraft = ArticleDraftGenerator.GetDraft();

            OpenPage(_currentArticleDraft.SitePostId, () => UpdateOrPublishPost(_currentArticleDraft),
                () => _timer.Change(100, Timeout.Infinite));
        }

        private void UpdateOrPublishPost(ArticleDraft draft)
        {
            try
            {
                NotifyStatus("发布中... 标题：" + draft.Title);
                _state = WebBrowserState.Request;
                var scripts = GetInvokeJS(draft);
                //每执行一个脚本 等待
                foreach (var scriptKv in scripts)
                {
                    _browser.ExecuteScriptAsync(scriptKv.Value);
                    switch (scriptKv.Key)
                    {
                        case JSType.ADD_META:
                        {
                            _browser.ReqHandler.IsResponceSuccess = IsAddMetaSuccess;
                            _browser.ReqHandler.ResponceSuccess = AddMetaSuccess;
                            //等待添加Meta完成
                            WaitResponceSuccess();
                        }
                            break;
                        case JSType.ADD_META_NO_SUBMIT:
                        case JSType.PUBLISH:
                            break;
                    }
                }

                _browser.ReqHandler.IsResponceSuccess = IsPublishSuccess;
                _browser.ReqHandler.ResponceSuccess = PublishSuccess;
                //等待文章发布/更新
                if (!WaitResponceSuccess())
                {
                    var logInfo = new StringBuilder();
                    foreach (var script in scripts) logInfo.AppendLine(script.Value);
                    Logger.Info("\n发布超时:" + logInfo);
                    NotifyStatus("发布超时！标题：" + draft.Title);

                    ResetReqHandler();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public bool IsAddMetaSuccess(string url, string postData)
        {
            return url.EndsWith("wp-admin/admin-ajax.php") &&
                   postData.Contains("&action=add-meta&");
        }

        public void AddMetaSuccess(string url)
        {
            ResetReqHandler();
        }

        public bool IsPublishSuccess(string url, string postData)
        {
            return url.Contains("action=edit&message");
        }

        public void PublishSuccess(string url)
        {
            _browser.Stop();
            _currentArticleDraft.State = ArticleDraftState.Published;
            _currentArticleDraft.SitePostId =
                WordPressHelper.RegSitePostId.Match(url).Groups["SitePostId"].Value;
            //UpdateArticleDraft(_currentArticleDraft);
            _timer.Change(100, Timeout.Infinite);
            ResetReqHandler();
            NotifyStatus("发布成功：" + _currentArticleDraft.Title);
        }

        public void ResetReqHandler()
        {
            _browser.ReqHandler.IsResponceSuccess = null;
            _browser.ReqHandler.ResponceSuccess = null;
        }

        private bool WaitResponceSuccess()
        {
            return SpinWait.SpinUntil(() => _browser.ReqHandler.ResponceSuccess == null, _timeOutTimeSpan);
        }

        private void OpenPage(string sitePostId, Action actionSuc, Action actionFail)
        {
            _state = WebBrowserState.Request;
            string url;
            if (string.IsNullOrEmpty(sitePostId))
            {
                _currentEditUrl = null;
                _browser.CommandType = CommandType.OpenNew;
                url = _currentSiteInfo.PostNewUrl;
            }
            else
            {
                _currentEditUrl = _currentSiteInfo.PostEditUrl.Replace("SitePostId", sitePostId);
                _browser.CommandType = CommandType.OpenEdit;
                url = _currentEditUrl;
            }

            NotifyStatus("正在打开页面：" + url);
            this.InvokeIfRequired(() => _browser.LoadPageAsync(url), true);
            //等待加载完成
            var resultLoad = SpinWait.SpinUntil(() => _state == WebBrowserState.LoadEnded, _timeOutTimeSpan);
            //超时未打开
            if (!resultLoad)
            {
                NotifyStatus("打开页面超时:" + url);
                actionFail();
            }
            else
            {
                _timerWait4OpenPageCompleted.Change(100, Timeout.Infinite);

                _browser.ExecuteScriptAsync("jQuery('#content-tmce').click();"); //确保为可视化界面
                _isWaiting4OpenAddOrEdit = true;
                //打开成功后执行
                _actionOpenCompleted = () =>
                {
                    NotifyStatus("打开页面成功");
                    _browser.CommandType =
                        string.IsNullOrEmpty(sitePostId) ? CommandType.Posting : CommandType.Updating;
                    actionSuc();
                };
            }
        }

        private Dictionary<JSType, string> GetInvokeJS(ArticleDraft draft)
        {
            var result = new Dictionary<JSType, string>();
            var subjectTime = draft.PublishTime;
            var metas = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(draft.PostMetas)) metas.Add("PostMetas", draft.PostMetas);
            var index = 0;
            foreach (var meta in metas)
            {
                index++;
                var isLast = index == metas.Count;
                var jsSubmit = isLast ? "" : "jQuery('#newmeta-submit').click();";
                var jsType = isLast ? JSType.ADD_META_NO_SUBMIT : JSType.ADD_META;
                var metav = meta.Value.Replace("'", "\\'").RemoveLineBreak();
                result.Add(jsType,
                    string.Format(
                        "if(jQuery(\"#the-list input[type='text'][value='{0}']\").length==0){{   jQuery('#metakeyinput').val('{0}');   jQuery('#metavalue').val('{1}'); {2}}}",
                        meta.Key, metav, jsSubmit));
            }

            var title = draft.Title.RemoveLineBreak();
            if (title.Length > 55) title = title.Substring(0, 55);
            title = title.Replace("\\", "\\\\").Replace("'", "\\'");
            var titleEn = draft.TitleEn.Replace("'", " ").RemoveLineBreak();
            if (titleEn.Length > 55) titleEn = titleEn.Substring(0, 55);
            //选择分类 js 
            var sbArticleTypes = new StringBuilder();
            var articleTypeArray = draft.Categories.Split(',');
            foreach (var articleType in articleTypeArray)
            {
                var siteCategery = _currentSiteInfo.Categories.Find(
                    x => string.Equals(x.Name, articleType.Trim(), StringComparison.CurrentCultureIgnoreCase));
                if (siteCategery == null)
                {
                    draft.Tags = string.IsNullOrEmpty(draft.Tags.Trim())
                        ? articleType
                        : string.Format("{0},{1}", draft.Tags, articleType);
                    continue;
                }

                var tempCategoryJs = $"jQuery(\"input[value = '{siteCategery.Id}'][name = 'post_category[]']\").click();";
                sbArticleTypes.Append(tempCategoryJs);
            }

            var catJs = sbArticleTypes.ToString();
            var js = WordPressHelper.JQueryTemplate.FormatWith(title, ToSafeString(draft.Content), catJs, draft.Tags,
                subjectTime.Month.ToString(WordPressHelper.Str00), subjectTime.Day.ToString(WordPressHelper.Str00),
                subjectTime.Year, subjectTime.Hour.ToString(WordPressHelper.Str00),
                subjectTime.Minute.ToString(WordPressHelper.Str00), titleEn);
            result.Add(JSType.PUBLISH, js);
            return result;
        }

        public string ToSafeString(string source)
        {
            source = WordPressHelper.RegRnt.Replace(source.Trim(), WordPressHelper.FlagBr);
            source = WordPressHelper.RegNn.Replace(source.Trim(), WordPressHelper.FlagBr);
            source = WordPressHelper.RegLine.Replace(source, WordPressHelper.Str11);
            source = source.Replace(WordPressHelper.StrSingle, WordPressHelper.StrDouble);
            return source;
        }

        private void LoginSite()
        {
            NotifyStatus("开始登录");
            _browser.CommandType = CommandType.Login;
            //打开登录页
            _browser.LoadPageAsync(_currentSiteInfo.LoginUrl);
            var isOpenSuccess = SpinWait.SpinUntil(() => _state == WebBrowserState.LoadEnded, _timeOutTimeSpan);
            if (isOpenSuccess)
            {
                var jsLogin =
                    $@"document.getElementById('user_login').value='{_currentSiteInfo.UserName}';
                                        document.getElementById('user_pass').value='{_currentSiteInfo
                        .Password}';
                                        document.getElementById('rememberme').click();
                                        document.getElementById('wp-submit').click();";
                _browser.ExecuteScriptAsync(jsLogin);

                var isLoginSuccess = SpinWait.SpinUntil(() => _isLoggedIn, _timeOutTimeSpan);
                if (!isLoginSuccess) NotifyStatus("登录失败");
            }
            else
            {
                NotifyStatus("打开登录页失败");
            }
        }

        private void browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Url == "about:blank") return;

            if (_browser.CommandType == CommandType.Login)
            {
                if (e.Url.Contains(_currentSiteInfo.LoginUrl))
                {
                    _state = WebBrowserState.LoadEnded;
                }
                else if (e.Url.Contains("wp-admin"))
                {
                    _isLoggedIn = true;
                    _timer.Change(100, Timeout.Infinite);
                }
            }
            else if (_browser.CommandType == CommandType.Posting && e.Url.Contains("&action=edit"))
            {
                _state = WebBrowserState.LoadEnded;
            }
            else if ((_browser.CommandType == CommandType.Updating || _browser.CommandType == CommandType.OpenEdit) &&
                     e.Url.Contains(_currentEditUrl))
            {
                _state = WebBrowserState.LoadEnded;
            }
            else if (_browser.CommandType == CommandType.OpenNew && e.Url.Contains(_currentSiteInfo.PostNewUrl))
            {
                _state = WebBrowserState.LoadEnded;
            }
        }

        private void timer_Tick(object o)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            //未登录+非正在登录
            if (!_isLoggedIn) //&& !_isLoggingIn
                LoginSite();
            //已登录+非正在发布（已发布）
            else
                PublishAticle();
        }

        private void timerWait_Tick(object obj)
        {
            _timerWait4OpenPageCompleted.Change(Timeout.Infinite, Timeout.Infinite);
            if (_isWaiting4OpenAddOrEdit)
            {
                var htmlDoc = new XHtmlDocument();
                htmlDoc.LoadHtml(_browser.Html);
                var completeElement = htmlDoc.DocumentNode.SelectSingleNode(WordPressHelper.XpathEditorLoadCompleted);

                while (completeElement == null)
                {
                    if (Disposing) break;

                    Thread.Sleep(200);
                    htmlDoc.LoadHtml(_browser.Html);
                    completeElement = htmlDoc.DocumentNode.SelectSingleNode(WordPressHelper.XpathEditorLoadCompleted);
                }

                _actionOpenCompleted();
                _isWaiting4OpenAddOrEdit = false;
            }
        }

        private void FormPublish_FormClosed(object sender, FormClosedEventArgs e)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _timer.Dispose();

            _timerWait4OpenPageCompleted.Change(Timeout.Infinite, Timeout.Infinite);
            _timerWait4OpenPageCompleted.Dispose();
        }

        private void NotifyStatus(string msg)
        {
            this.InvokeIfRequired(() =>
            {
                lbl_status.Text = msg;
                Application.DoEvents();
            }, false);
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            _browser.Reload();
        }
    }
}