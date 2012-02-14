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
using Microsoft.Phone.Controls;

namespace KhanViewer
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            App.ViewModel.LoadData();
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            App.ViewModel.LoadData();

            if (!App.ViewModel.HasUserSeenIntro())
            {
                NavigationService.Navigate(new Uri("/Intro.xaml", UriKind.Relative));
            }

            DataContext = App.ViewModel;
            base.OnNavigatedTo(e);
        }

        
        // Handle selection changed on ListBox
        private void MainListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If selected index is -1 (no selection) do nothing
            if (MainListBox.SelectedIndex == -1)
                return;
            var item = MainListBox.SelectedItem as Item;
            // Navigate to the new page
            NavigationService.Navigate(new Uri("/CategoryPage.xaml?name=" + item.Name, UriKind.Relative));
            MainListBox.SelectedIndex = -1;
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}