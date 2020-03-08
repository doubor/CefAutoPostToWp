using System;
using CefAutoPostToWp.Models;

namespace CefAutoPostToWp.Sample
{
    public class ArticleDraftGenerator
    {
        public static ArticleDraft GetDraft()
        {
            var strTime = DateTime.Now.ToString();

            return new ArticleDraft
            {
                Title = $"Title {strTime}",
                TitleEn = $"Title {strTime}",
                Content = $"Content {strTime}",
                PublishTime = DateTime.Now,
                State = ArticleDraftState.UnPublish,
                Tags = "Tag1,Tag2",
                PostMetas = $"PostMetas {strTime}",
                Categories = "category2,category3"
            };
        }
    }
}