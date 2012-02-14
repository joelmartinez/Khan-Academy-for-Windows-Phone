using System.Runtime.Serialization;

namespace KhanProxy.Services
{
    [DataContract(Namespace = "http://khanacademy.com/services")]
    public class KhanCategory
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
    }

    [DataContract(Namespace = "http://khanacademy.com/services")]
    public class KhanVideo
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string Category { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Uri { get; set; }
    }

    [DataContract(Namespace="http://khanacademy.com/services")]
    public class KCategory
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
    }

    [DataContract(Namespace = "http://khanacademy.com/services")]
    public class KVideo
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string Category { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Uri { get; set; }
    }
}