using System;
using System.Linq;
using System.Collections.Generic;

namespace KhanProxy.Services
{
    public class KhanAcademy : IKhanAcademy
    {
        public static ServiceStatus CurrentStatus { get; private set; }

        public static void CategoriesUpdated()
        {
            if (CurrentStatus == null) CurrentStatus = new ServiceStatus();

            CurrentStatus.CategoriesLastUpdated = DateTime.Now;
        }

        public static void VideosUpdated()
        {
            if (CurrentStatus == null) CurrentStatus = new ServiceStatus();

            CurrentStatus.VideosLastUpdated = DateTime.Now;
        }

        public ServiceStatus GetStatus()
        {
            return CurrentStatus ?? new ServiceStatus
            {
                CategoriesLastUpdated = DateTime.MinValue,
                VideosLastUpdated = DateTime.MinValue
            };
        }


        public KCategory[] GetCategories()
        {
            var source = new KhanDataSource();
            return source.Categories.Select(c => new KCategory
                {
                    Description = c.Description,
                    ID = c.ID,
                    Name = c.Name
                }).ToArray();
        }

        public KVideo[] GetVideos(string category)
        {
            var source = new KhanDataSource();
            return source.Videos
                .Where(v => v.Category == category)
                .Select(v => new KVideo
                    {
                        Category = v.Category,
                        Description = v.Description,
                        ID = v.ID,
                        Name = v.Name,
                        Uri = v.Uri
                    })
                .ToArray();
        }
    }
}
