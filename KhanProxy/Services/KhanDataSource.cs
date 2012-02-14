using System.Linq;

namespace KhanProxy.Services
{
    public class KhanDataSource
    {
        private KhanHtmlParser parser;

        public KhanDataSource()
        {
            this.parser = KhanHtmlParser.Create();
        }

        public IQueryable<KhanCategory> Categories
        {
            get
            {
                // TODO: Introduce Caching
                var categoryArray = this.parser.GetCategories();
                KhanAcademy.CategoriesUpdated();
                return categoryArray.AsQueryable();
            }
        }

        public IQueryable<KhanVideo> Videos
        {
            get
            {
                // TODO: Caching
                var videoArray = this.parser.GetVideos();
                KhanAcademy.VideosUpdated();
                return videoArray.AsQueryable();
            }
        }
    }
}