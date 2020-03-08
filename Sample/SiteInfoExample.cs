using System.Collections.Generic;
using CefAutoPostToWp.Models;

namespace CefAutoPostToWp.Sample
{
    public class SiteInfoExample : SiteInfo
    {
        public override string SiteName => "wp.com";

        public override string UserName => "admin";

        public override string Password => "1";

        public override List<Category> Categories => new List<Category>
        {
            new Category {Id = 1, Name = "category1"},
            new Category {Id = 4, Name = "category2"},
            new Category {Id = 5, Name = "category3"}
        };
    }
}