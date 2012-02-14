using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;

namespace KhanViewer.Models
{
    public static class WebHelper
    {
        public static void Get(string url, Action<string> action, Action<Exception> error)
        {
            Get(new Uri(url), action, error);
        }

        public static void Get(Uri uri, Action<string> action, Action<Exception> error)
        {
            var request = WebRequest.CreateHttp(uri);
            request.UserAgent = "Khan Academy Windows Phone Client ";

            request.BeginGetResponse(i =>
            {
                try
                {
                    var response = request.EndGetResponse(i);
                    var sreader = new StreamReader(response.GetResponseStream());
                    var result = sreader.ReadToEnd();
                    action(result);
                }
                catch (Exception ex)
                {
                    error(ex);
                }
            }, null);
        }

        public static void Json<T>(string url, Action<T> action, Action<Exception> error)
        {
            Json<T>(new Uri(url), action, error);
        }

        public static void Json<T>(Uri uri, Action<T> action, Action<Exception> error)
        {
            Get(uri, json =>
            {
                try
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                    byte[] bytes = Encoding.UTF8.GetBytes(json);
                    using (var stream = new MemoryStream(bytes))
                    {
                        var deserialized = serializer.ReadObject(stream);

                        action((T)deserialized);
                    }
                }
                catch (Exception ex)
                {
                    error(ex);
                }
            }, error);
        }
    }
}
