using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using KhanViewer.Models;

namespace KhanViewer
{
    [DataContract]
    public class CategoryItem : Item
    {
        private bool loaded = false;

        public CategoryItem()
        {
            this.Videos = new ObservableCollection<VideoItem>();
        }

        /// <summary>List of videos in this category</summary>
        [DataMember]
        public ObservableCollection<VideoItem> Videos { get; set; }

        public void LoadVideos()
        {
            if (!loaded)
            {
                // first load what I know (ie. from disk)
                var vids = LocalStorage.GetVideos(this.Name);
                foreach (var vid in vids) Videos.Add(vid);

                // then kick off the server to the query
                Clouds.GetVideosFromServer(this.Videos, this.Name);
                loaded = true;
            }
        }

        public static void Initialize(ObservableCollection<CategoryItem> items)
        {
            
            // first load what I know
            var vids = LocalStorage.GetCategories();
            foreach (var vid in vids) items.Add(vid);

            // then start to query the server
            Clouds.LoadCategoriesFromServer(items);
            return;
        }
    }
}
