﻿using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace KhanViewer.Models
{

    public abstract class DataServer
    {
        public void LoadCategories(ObservableCollection<CategoryItem> items)
        {
            this.LoadCategories(items, cats => LocalStorage.SaveCategories(cats));
        }

        protected abstract void LoadCategories(ObservableCollection<CategoryItem> items, Action<CategoryItem[]> localSaveAction);

        public void LoadVideos(string category, ObservableCollection<VideoItem> items)
        {
            this.LoadVideos(category, items, vids => LocalStorage.SaveVideos(category, vids));
        }

        protected abstract void LoadVideos(string category, ObservableCollection<VideoItem> items, Action<VideoItem[]> localSaveAction);
    }
}
