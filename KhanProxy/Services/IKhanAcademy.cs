using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Collections.Generic;

namespace KhanProxy.Services
{
    [ServiceContract(Namespace="http://khanacademy.com/services")]
    public interface IKhanAcademy
    {
        [OperationContract]
        ServiceStatus GetStatus();

        [OperationContract]
        KCategory[] GetCategories();

        [OperationContract]
        KVideo[] GetVideos(string category);
    }

    [DataContract]
    public class ServiceStatus
    {
        [DataMember]
        public DateTime CategoriesLastUpdated { get; set; }
        [DataMember]
        public DateTime VideosLastUpdated { get; set; }
    }
}
