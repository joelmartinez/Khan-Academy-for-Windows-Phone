using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace KhanViewer.Models
{

    public sealed class KhanAcademyApi : DataServer
    {
        protected override void LoadCategories(ObservableCollection<CategoryItem> items, Action<CategoryItem[]> localSaveAction)
        {
            var queryHandle = App.ViewModel.StartQuerying();
            WebHelper.Json<JsonCategory[]>("http://www.khanacademy.org/api/v1/playlists", cats =>
            {
                using (queryHandle)
                {
                    var serverItems = cats.OrderBy(c => c.Title).Select(k => new CategoryItem { Name = k.Title, Description = k.Description });
                    if (serverItems.Count() > 0)
                    {
                        var op = System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            items.Clear();
                            foreach (var item in serverItems)
                            {
                                items.Add(item);
                            }
                        });

                        localSaveAction(serverItems.ToArray());
                    }
                    else
                    {
                        App.ViewModel.SetError("No Categories returned");
                    }
                }
            },
            e =>
            {
                App.ViewModel.SetError(e.Message);
            });
        }

        protected override void LoadVideos(string category, ObservableCollection<VideoItem> items, Action<VideoItem[]> localSaveAction)
        { 
            var queryHandle = App.ViewModel.StartQuerying();

            string apiUrl = string.Format("http://www.khanacademy.org/api/v1/playlists/{0}/videos", category);

            WebHelper.Json<JsonVideo[]>(apiUrl, vids =>
            {
                using (queryHandle)
                {
                    var serverItems = vids.Select(k => new VideoItem { 
                        Name = k.Title, 
                        Description = k.Description, 
                        YoutubeId = k.YouTubeId,
                        VideoUri = new Uri(k.Url),
                        VideoFileUri = new Uri(k.Downloads.Video),
                        VideoScreenshotUri = new Uri(k.Downloads.Screenshot),
                        Parent = category });
                    if (serverItems.Count() > 0)
                    {
                        System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            items.Clear();
                            foreach (var item in serverItems)
                            {
                                items.Add(item);
                            }
                        });

                        localSaveAction(serverItems.ToArray());
                    }
                    else
                    {
                        App.ViewModel.SetError("No Videos returned for " + category);
                    }
                }
            },
            e =>
            {
                App.ViewModel.SetError(e.Message);
            });
        }

        [DataContract]
        public class JsonCategory
        {
            [DataMember(Name = "ka_url")]
            public string Url { get; set; }
            [DataMember(Name = "url")]
            public string YoutubeUrl { get; set; }
            [DataMember(Name = "title")]
            public string Title { get; set; }
            [DataMember(Name = "description")]
            public string Description { get; set; }
            [DataMember(Name = "youtube_id")]
            public string YoutubeId { get; set; }
        }

        [DataContract]
        public class JsonVideo
        {
            [DataMember(Name = "ka_url")]
            public string Url { get; set; }
            [DataMember(Name = "url")]
            public string YoutubeUrl { get; set; }
            [DataMember(Name = "title")]
            public string Title { get; set; }
            [DataMember(Name = "description")]
            public string Description { get; set; }
            [DataMember(Name = "youtube_id")]
            public string YouTubeId { get; set; }
            [DataMember(Name = "readable_id")]
            public string ReadableId { get; set; }
            [DataMember(Name = "keywords")]
            public string Keywords { get; set; }
            [DataMember(Name = "download_urls")]
            public JsonDownloads Downloads { get; set; }

        }

        [DataContract]
        public class JsonDownloads
        {
            [DataMember(Name = "mp4")]
            public string Video { get; set; }
            [DataMember(Name = "png")]
            public string Screenshot { get; set; }
        }
    }
}
