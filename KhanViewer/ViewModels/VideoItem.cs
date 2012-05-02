﻿using System;
using System.Runtime.Serialization;

#if WINDOWS_PHONE
using Microsoft.Phone.Tasks;
#endif

namespace KhanViewer
{
    [DataContract]
    public class VideoItem : Item
    {
        [DataMember]
        public string Parent { get; set; }

        [DataMember]
        public string YoutubeId { get; set; }

        [DataMember]
        public Uri VideoUri { get; set; }

        [DataMember]
        public Uri VideoScreenshotUri { get; set; }

        [DataMember]
        public Uri VideoFileUri { get; set; }

        public void Navigate()
        {

            App.ViewModel.TrackPageView(this.Name, "/" + this.Parent + "/Video/" + this.Name);
            
#if !WINDOWS_PHONE
            throw new NotImplementedException();
#else
            WebBrowserTask browser = new WebBrowserTask();

            if (this.VideoFileUri == null || string.IsNullOrWhiteSpace(this.VideoFileUri.ToString()))
            {
                browser.Uri = this.VideoUri;
            }
            else
            {
                browser.Uri = this.VideoFileUri;
            }

            browser.Show();
#endif
        }
    }
}
