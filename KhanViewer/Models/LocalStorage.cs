using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;
using KhanProxy.Services;
using System.Text.RegularExpressions;
using System;

namespace KhanViewer.Models
{
    public static class LocalStorage
    {
        static readonly string CategoryFileName = "categories.xml";
        static readonly string VideosFileName = "videos.xml";
        static readonly string LandingBitFileName = "landed.bin";
        private static bool hasSeenIntro;

        /// <summary>Will return false only the first time a user ever runs this.
        /// Everytime thereafter, a placeholder file will have been written to disk
        /// and will trigger a value of true.</summary>
        public static bool HasUserSeenIntro()
        {
            if (hasSeenIntro) return true;

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(LandingBitFileName))
                {
                    // just write a placeholder file one byte long so we know they've landed before
                    using (var stream = store.OpenFile(LandingBitFileName, FileMode.Create))
                    {
                        stream.Write(new byte[] { 1 }, 0, 1);
                    }
                    return false;
                }

                hasSeenIntro = true;
                return true;
            }
        }

        public static IEnumerable<CategoryItem> GetCategories()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(CategoryFileName)) return new CategoryItem[] { new CategoryItem { Name = "Loading", Description = "From Server ..." } };

                using (var stream = store.OpenFile(CategoryFileName, FileMode.Open))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(CategoryItem[]));
                    var localCats = serializer.ReadObject(stream) as CategoryItem[];

                    if (localCats == null || localCats.Length == 0) return new CategoryItem[] { new CategoryItem { Name = "Loading from server ...", Description = "local cache was empty" } };

                    // heh, almost read this variable as lolCats 
                    return localCats;
                }
            }
        }

        public static IEnumerable<VideoItem> GetVideos(string categoryName)
        {
            string filename = categoryName + VideosFileName;
            filename = IsValidFilename(filename);

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(filename)) return new VideoItem[] { new VideoItem { Name = "Loading", Description = "From Server ..." } };

                using (var stream = store.OpenFile(filename, FileMode.Open))
                {
                    VideoItem[] localVids;

                    try
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(VideoItem[]));
                        localVids = serializer.ReadObject(stream) as VideoItem[];
                    }
                    catch
                    {
                        return GetPlaceHolder();
                    }

                    if (localVids == null || localVids.Length == 0) return GetPlaceHolder();
                    
                    return localVids;
                }
            }
        }

        /// <summary>If a specific video item is available on disk, it will be deserialized.
        /// Otherwise will return null.</summary>
        public static VideoItem GetVideo(string categoryName, string videoName)
        {
            string catpath = IsValidFilename(categoryName);
            string vidpath = IsValidFilename(videoName);
            string filename = Path.Combine(catpath, vidpath) + ".xml";

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(filename)) return null;

                using (var stream = store.OpenFile(filename, FileMode.Open))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(VideoItem));
                    var deserializedVid = serializer.ReadObject(stream) as VideoItem;
                    return deserializedVid;
                }
            }
        }

        public static void SaveVideo(VideoItem item)
        {
            string catpath = IsValidFilename(item.Parent);
            string vidpath = IsValidFilename(item.Name);
            string filename = Path.Combine(catpath, vidpath) + ".xml";

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.DirectoryExists(catpath)) store.CreateDirectory(catpath);
                using (var stream = store.OpenFile(filename, FileMode.Create))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(VideoItem));
                    serializer.WriteObject(stream, item);
                }
            }
        }

        public static void SaveCategories<T>(T[] categories)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            using (var stream = store.OpenFile(CategoryFileName, FileMode.Create))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T[]));
                serializer.WriteObject(stream, categories);
            }
        }

        public static void SaveVideos<T>(string categoryName, T[] videos)
        {
            string filename = categoryName + VideosFileName;

            filename = IsValidFilename(filename);


            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(filename)) store.DeleteFile(filename);

                using (var stream = store.OpenFile(filename, FileMode.OpenOrCreate))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(T[]));
                    serializer.WriteObject(stream, videos);
                }
            }
        }

        /// <summary>Verifies if there are invalid characters, and if so, removes them from the filename</summary>
        /// <remarks>Method derived from information here:
        /// http://stackoverflow.com/questions/333175/is-there-a-way-of-making-strings-file-path-safe-in-c</remarks>
        private static string IsValidFilename(string testName)
        {
            var invalid = System.IO.Path.GetInvalidPathChars().Union(new char[] { ':', ' ' }).ToArray();
            Regex containsABadCharacter = new Regex("[" + Regex.Escape(new string(invalid)) + "]");
            if (containsABadCharacter.IsMatch(testName)) {

                Array.ForEach(invalid,
                    c => testName = testName.Replace(c.ToString(), String.Empty));
            };

            return testName;
        }

        private static VideoItem[] GetPlaceHolder()
        {
            return new VideoItem[] { new VideoItem { Name = "Loading from server ...", Description = "local cache was empty." } };
        }
    }
}
