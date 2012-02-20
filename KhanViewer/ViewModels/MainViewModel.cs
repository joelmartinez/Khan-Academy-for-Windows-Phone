using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using KhanViewer.Models;
using Microsoft.Phone.Shell;
using GoogleAnalyticsTracker;

namespace KhanViewer
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            this.Categories = new ObservableCollection<CategoryItem>();
        }

        #region Properties

        public ObservableCollection<CategoryItem> Categories { get; private set; }

        public bool Querying { get; set; }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        /// <summary>If <see cref="IsError" /> is true, then this will contain the fault error.
        /// You should not show this to the user, but communicate to devs.</summary>
        public string ErrorMessage { get; private set; }

        /// <summary>If this is true, there has been a fault and you should let the user know.</summary>
        public bool IsError { get; private set; }

        /// <summary>If the application encounters an error condition, call this method.</summary>
        /// <param name="message">The error details to send to the developers.</param>
        public void SetError(string message)
        {
            this.IsError = true;
            this.ErrorMessage = message;

            this.NotifyPropertyChanged("ErrorMessage");
            this.NotifyPropertyChanged("IsError");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>call this any time you begin a server query</summary>
        /// <returns>a handle which should wrap the operation in a using statement.</returns>
        public IDisposable StartQuerying()
        {
            // TODO: implement refcounting
            Querying = true;
            NotifyPropertyChanged("Querying");
            return new QueryingHandle(this);
        }

        /// <returns>true only the first time the user accesses the application.</returns>
        public bool HasUserSeenIntro()
        {
            return LocalStorage.HasUserSeenIntro();
        }

        public CategoryItem GetCategory(string categoryName)
        {
            var state = PhoneApplicationService.Current.State;
            
            object ocategory;
            if (state.TryGetValue(
                categoryName, 
                out ocategory)) return ocategory as CategoryItem;

            var category = this.Categories.Where(c => c.Name == categoryName).FirstOrDefault();
            category.LoadVideos();
            state[categoryName] = category;
            return category;
        }

        public VideoItem GetVideo(string category, string name)
        {
            var vid = LocalStorage.GetVideo(category, name);
            if (vid != null) return vid;

            // didn't have the vid on disk, query the memory store.
            vid = this.Categories
                .Where(c => c.Name == category)
                .SelectMany(c => c.Videos)
                .SingleOrDefault(v => v.Name == name);

            if (vid != null) LocalStorage.SaveVideo(vid);

            return vid;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            if (!this.IsDataLoaded)
            {
                this.IsDataLoaded = true;
                
                CategoryItem.Initialize(this.Categories);
            }
        }

        public void TrackPageView(string title, string path)
        {
            Tracker tracker = new Tracker("UA-859807-2", "http://khanacademyforwindowsphone.com");
            tracker.TrackPageView(title, path);
        }

        #region Private Methods

        private void StopQuerying()
        {
            // TODO: implement refcounting
            this.Querying = false;
            NotifyPropertyChanged("Querying");
        }

        private class QueryingHandle : IDisposable
        {
            private MainViewModel model;

            public QueryingHandle(MainViewModel model)
            {
                this.model = model;
            }

            void IDisposable.Dispose()
            {
                this.model.StopQuerying();
            }
        }

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}