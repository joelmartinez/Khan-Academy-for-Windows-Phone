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
                LocalStorage.GetVideos(this.Name, vids =>
                    {
                        UIThread.Invoke(() => { foreach (var vid in vids) Videos.Add(vid); });

                        loaded = true;
                    });

                // now kick off the server to the query
                Clouds.GetVideosFromServer(this.Videos, this.Name);
            }
            else if (this.Videos.Count == 0)
            {
                // if we've already loaded, but don't have any results, then need to try again
                Clouds.GetVideosFromServer(this.Videos, this.Name);
            }
        }

        public static void Initialize(ObservableCollection<CategoryItem> items)
        {
            // first load what I know
            LocalStorage.GetCategories(vids =>
                {
                    UIThread.Invoke(() => { foreach (var vid in vids) items.Add(vid); });

                    // then start to query the server
                    Clouds.LoadCategoriesFromServer(items);
                });
        }
    }
}
