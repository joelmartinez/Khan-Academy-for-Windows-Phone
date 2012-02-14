using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace KhanViewer.Models
{
    public class Config
    {
        public static Uri ODataUri
        {
            get
            {
                return new Uri("http://localhost:2121/Services/KhanData.svc/", UriKind.Absolute);
            }
        }
    }
}
