using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace KhanViewer
{
    public partial class CategoryPage : PhoneApplicationPage
    {
        // Constructor
        public CategoryPage()
        {
            InitializeComponent();
        }

        // When page is navigated to set data context to selected item in list
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string selectedIndex = "";
            if (NavigationContext.QueryString.TryGetValue("name", out selectedIndex))
            {
                App.ViewModel.TrackPageView(selectedIndex, "/Playlist/" + selectedIndex);
                var category = App.ViewModel.GetCategory(selectedIndex);
                category.LoadVideos();
                LayoutRoot.DataContext = category;
            }
        }
        
        private void MainListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If selected index is -1 (no selection) do nothing
            if (MainListBox.SelectedIndex == -1)
                return;
            var item = MainListBox.SelectedItem as VideoItem;
            // Navigate to the new page
            item.Navigate();

            // Reset selected index to -1 (no selection)
            MainListBox.SelectedIndex = -1;
        }
    }
}