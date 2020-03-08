using System;

namespace CefAutoPostToWp.Models
{
    public class ArticleDraft
    {
        public string Title { get; set; }
        public string TitleEn { get; set; }
        public string Content { get; set; }
        public DateTime PublishTime { get; set; }
        public string PostMetas { get; set; }
        public string Categories { get; set; }
        public string Tags { get; set; }
        public string SitePostId { get; set; }
        public ArticleDraftState State { get; set; }
    }
}