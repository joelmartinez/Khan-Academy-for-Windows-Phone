using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
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
                var category = App.ViewModel.GetCategory(selectedIndex);//.Categories.Where(c => c.Name == selectedIndex).FirstOrDefault();
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
            //NavigationService.Navigate(new Uri("/DetailsPage.xaml?category=" + item.Parent + "&video=" + item.Name, UriKind.Relative));

            // Reset selected index to -1 (no selection)
            MainListBox.SelectedIndex = -1;
        }
    }
}