using System.Collections.Generic;

namespace CefAutoPostToWp.Models
{
    public abstract class SiteInfo
    {
        public abstract string SiteName { get; }
        public abstract string UserName { get; }
        public abstract string Password { get; }
        public abstract List<Category> Categories { get; }

        public string LoginUrl => $"{SiteName}/wp-login.php";
        public string PostNewUrl => $"{SiteName}/wp-admin/post-new.php";
        public string PostEditUrl => $"{SiteName}/wp-admin/post.php?post=SitePostId&action=edit";
    }
}