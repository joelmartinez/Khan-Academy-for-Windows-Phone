using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System;

#if !WINDOWS_PHONE
using Windows.Storage;
using KhanViewer.Common;
#endif

#if WINDOWS_PHONE
using KhanProxy.Services;
using System.IO.IsolatedStorage;
#endif

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
        public static void HasUserSeenIntro(Action<bool> action)
        {
#if !WINDOWS_PHONE
            StorageFolder folder = ApplicationData.Current.LocalFolder;// KnownFolders.DocumentsLibrary;
            //var file = await 
            throw new NotImplementedException("implement if existss");
            FileAsync.Write(folder, LandingBitFileName, CreationCollisionOption.ReplaceExisting, writer => writer.WriteByte(1));
            action(false);
#else
            if (hasSeenIntro) action(true);

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(LandingBitFileName))
                {
                    // just write a placeholder file one byte long so we know they've landed before
                    using (var stream = store.OpenFile(LandingBitFileName, FileMode.Create))
                    {
                        stream.Write(new byte[] { 1 }, 0, 1);
                    }
                    action(false);
                }

                hasSeenIntro = true;
                action(true);
            }
#endif
        }

        public static void GetCategories(Action<IEnumerable<CategoryItem>> result)
        {
#if !WINDOWS_PHONE
            throw new NotImplementedException();
            result(null);
#else
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(CategoryFileName))
                {
                    result(new CategoryItem[] { new CategoryItem { Name = "Loading", Description = "From Server ..." } });
                    return;
                }

                using (var stream = store.OpenFile(CategoryFileName, FileMode.Open))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(CategoryItem[]));
                    var localCats = serializer.ReadObject(stream) as CategoryItem[];

                    if (localCats == null || localCats.Length == 0)
                    {
                        result(new CategoryItem[] { new CategoryItem { Name = "Loading from server ...", Description = "local cache was empty" } });
                        return;
                    }

                    // heh, almost read this variable as lolCats 
                    result( localCats);
                }
            }
#endif
        }

        public static void GetVideos(string categoryName, Action<IEnumerable<VideoItem>> result)
        {
#if !WINDOWS_PHONE
            throw new NotImplementedException();
            result(null);
#else
            string filename = categoryName + VideosFileName;
            filename = IsValidFilename(filename);

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(filename))
                {
                    result(new VideoItem[] { new VideoItem { Name = "Loading", Description = "From Server ..." } });
                    return;
                }

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
                        result(GetPlaceHolder());
                        return;
                    }

                    if (localVids == null || localVids.Length == 0)
                    {
                        result(GetPlaceHolder());
                        return;
                    }
                    
                    result(localVids);
                }
            }
#endif
        }

        /// <summary>If a specific video item is available on disk, it will be deserialized.
        /// Otherwise will return null.</summary>
        public static void GetVideo(string categoryName, string videoName, Action<VideoItem> result)
        {
#if !WINDOWS_PHONE
            throw new NotImplementedException();
            result(null);
#else
            string catpath = IsValidFilename(categoryName);
            string vidpath = IsValidFilename(videoName);
            string filename = Path.Combine(catpath, vidpath) + ".xml";

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(filename))
                {
                    result(null);
                    return;
                }

                using (var stream = store.OpenFile(filename, FileMode.Open))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(VideoItem));
                    var deserializedVid = serializer.ReadObject(stream) as VideoItem;
                    result( deserializedVid );
                }
            }
#endif
        }

        public static void SaveVideo(VideoItem item)
        {
#if !WINDOWS_PHONE
            throw new NotImplementedException();
#else
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
#endif
        }

        public static void SaveCategories<T>(T[] categories)
        {
#if !WINDOWS_PHONE
            throw new NotImplementedException();
#else
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            using (var stream = store.OpenFile(CategoryFileName, FileMode.Create))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T[]));
                serializer.WriteObject(stream, categories);
            }
#endif
        }

        public static void SaveVideos<T>(string categoryName, T[] videos)
        {
#if !WINDOWS_PHONE
            throw new NotImplementedException();
#else
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
#endif
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
