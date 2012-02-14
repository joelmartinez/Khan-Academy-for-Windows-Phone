using System.Data.Services;
using System.Data.Services.Common;

namespace KhanProxy.Services
{
    public class KhanData : DataService<KhanDataSource>
    {
        public static void InitializeService(DataServiceConfiguration config)
        {
            config.SetEntitySetAccessRule("*", EntitySetRights.AllRead);
            config.SetServiceOperationAccessRule("*", ServiceOperationRights.AllRead);
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
        }

        protected override KhanDataSource CreateDataSource()
        {
            return new KhanDataSource();
        }
    }
}
