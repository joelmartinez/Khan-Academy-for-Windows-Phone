using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System;

#if !WINDOWS_PHONE
using Windows.Storage;
using KhanViewer.Common;
using System.Threading.Tasks;
#endif

#if WINDOWS_PHONE
using KhanProxy.Services;
using System.IO.IsolatedStorage;
#endif

namespace KhanViewer.Models
{

    public static class LocalStorage
    {
        
#if !WINDOWS_PHONE
        private async static Task<StorageFile> GetFile(string path)
        {
            var folder = ApplicationData.Current.LocalFolder;
            try
            {
                return await folder.GetFileAsync(CategoryFileName);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        private async static Task<bool> FileExists(string path)
        {
            var folder = ApplicationData.Current.LocalFolder;
            try
            {
                var file = await folder.GetFileAsync(path);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        private async static Task<Stream> WriteFile(string path)
        {
            var folder = ApplicationData.Current.LocalFolder;
            return await folder.OpenStreamForWriteAsync(path, CreationCollisionOption.ReplaceExisting);
        }

        private async static Task<StorageFolder> CreateDirectory(string path)
        {
            return await CreateDirectory(ApplicationData.Current.LocalFolder, path);
        }

        private async static Task<StorageFolder> CreateDirectory(StorageFolder folder, string path)
        {
            return await folder.CreateFolderAsync(path, CreationCollisionOption.OpenIfExists);
        }
#endif


        static readonly string CategoryFileName = "categories.xml";
        static readonly string VideosFileName = "videos.xml";
        static readonly string LandingBitFileName = "landed.bin";
        private static bool hasSeenIntro;

        /// <summary>Will return false only the first time a user ever runs this.
        /// Everytime thereafter, a placeholder file will have been written to disk
        /// and will trigger a value of true.</summary>
        public static void HasUserSeenIntro(Action<bool> action)
        {
            if (hasSeenIntro) action(true);

#if !WINDOWS_PHONE
            StorageFolder folder = ApplicationData.Current.LocalFolder;

            FileExists(LandingBitFileName).ContinueWith(value =>
                {
                    if (!value.Result)
                    {
                        FileAsync.Write(folder, LandingBitFileName, CreationCollisionOption.ReplaceExisting, writer => writer.WriteByte(1));

                        UIThread.Invoke(() => action(false));
                        return;
                    }
                });
#else

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
            }
#endif

            hasSeenIntro = true;
            action(true);
        }

        public static void GetCategories(Action<IEnumerable<CategoryItem>> result)
        {
#if !WINDOWS_PHONE

            GetFile(CategoryFileName).ContinueWith(value =>
            {
                var file = value.Result;

                if (file == null)
                {
                    result(new CategoryItem[] { new CategoryItem { Name = "Loading", Description = "From Server ..." } });
                    return;
                }

                var folder = ApplicationData.Current.LocalFolder;

                folder.OpenStreamForReadAsync(CategoryFileName).ContinueWith(filevalue =>
                    {
                        using (var stream = filevalue.Result)
                        {
                            DataContractSerializer serializer = new DataContractSerializer(typeof(CategoryItem[]));
                            var localCats = serializer.ReadObject(stream) as CategoryItem[];

                            if (localCats == null || localCats.Length == 0)
                            {
                                result(new CategoryItem[] { new CategoryItem { Name = "Loading from server ...", Description = "local cache was empty" } });
                                return;
                            }

                            // heh, almost read this variable as lolCats 
                            result(localCats);
                        }
                    });
            });
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
            string filename = categoryName + VideosFileName;
            filename = IsValidFilename(filename);

#if !WINDOWS_PHONE

            FileExists(filename).ContinueWith(exists =>
                {
                    if (!exists.Result)
                    {
                        result(new VideoItem[] { new VideoItem { Name = "Loading", Description = "From Server ..." } });
                        return;
                    }

                    var folder = ApplicationData.Current.LocalFolder;

                    folder.OpenStreamForReadAsync(filename).ContinueWith(readtask =>
                        {
                            using (var stream = readtask.Result)
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
                        });
                });
#else

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
            string catpath = IsValidFilename(categoryName);
            string vidpath = IsValidFilename(videoName);
            string filename = Path.Combine(catpath, vidpath) + ".xml";

#if !WINDOWS_PHONE
            var folder = ApplicationData.Current.LocalFolder;
            FileExists(filename).ContinueWith(exists =>
                {
                    if (!exists.Result)
                    {
                        result(null);
                        return;
                    }

                    folder.OpenStreamForReadAsync(filename).ContinueWith(readtask =>
                        {
                            using (var stream = readtask.Result)
                            {
                                DataContractSerializer serializer = new DataContractSerializer(typeof(VideoItem));
                                var deserializedVid = serializer.ReadObject(stream) as VideoItem;
                                result(deserializedVid);
                            }
                        });
                });
#else

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
            string catpath = IsValidFilename(item.Parent);
            string vidpath = IsValidFilename(item.Name);
            string filename = Path.Combine(catpath, vidpath) + ".xml";

#if !WINDOWS_PHONE
            var folder = CreateDirectory(catpath);

            WriteFile(filename).ContinueWith(opentask =>
                {
                    using (var stream = opentask.Result)
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(VideoItem));
                        serializer.WriteObject(stream, item);
                    }
                });
#else

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
            WriteFile(CategoryFileName).ContinueWith(opentask =>
                {
                    using (var stream = opentask.Result)
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(T[]));
                        serializer.WriteObject(stream, categories);
                    }
                });
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
            string filename = categoryName + VideosFileName;

            filename = IsValidFilename(filename);

#if !WINDOWS_PHONE
            WriteFile(filename).ContinueWith(opentask =>
                {
                    using (var stream = opentask.Result)
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(T[]));
                        serializer.WriteObject(stream, videos);
                    }
                });
#else

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
                foreach(var c in invalid)
                {
                    testName = testName.Replace(c.ToString(), String.Empty);
                }
            };

            return testName;
        }

        private static VideoItem[] GetPlaceHolder()
        {
            return new VideoItem[] { new VideoItem { Name = "Loading from server ...", Description = "local cache was empty." } };
        }
    }
}
