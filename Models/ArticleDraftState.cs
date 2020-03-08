using System.ComponentModel;

namespace CefAutoPostToWp.Models
{
    public enum ArticleDraftState
    {
        [Description("未发布")] UnPublish = 0,
        [Description("已发布")] Published = 1,
        [Description("已丢弃")] Droped = 2,
        [Description("发布中")] Publishing = 3,
        [Description("编辑中")] Editing = 4
    }
}