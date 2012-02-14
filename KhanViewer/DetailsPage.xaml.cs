using System.Linq;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace KhanViewer
{
    /// <summary>This page lets the user go to the video page on the web.</summary>
    public partial class DetailsPage : PhoneApplicationPage
    {
        // Constructor
        public DetailsPage()
        {
            InitializeComponent();
            App.ViewModel.LoadData();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string category = "", video = "";
            NavigationContext.QueryString.TryGetValue("category", out category);
            NavigationContext.QueryString.TryGetValue("video", out video);

            DataContext = App.ViewModel.GetVideo(category, video);
        }

        private void PlayVideo(object sender, System.Windows.RoutedEventArgs e)
        {
            var video = LayoutRoot.DataContext as VideoItem;
            WebBrowserTask browser = new WebBrowserTask();
            browser.URL = video.VideoUri.ToString();
            browser.Show();
        }
    }
}