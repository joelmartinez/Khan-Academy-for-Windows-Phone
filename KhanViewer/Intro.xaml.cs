using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace KhanViewer
{
    /// <summary>The purpose of this page is to stall the user with a bit of introductory text,
    /// while we do our initial server query to get the list of categories.</summary>
    public partial class Intro : PhoneApplicationPage
    {
        public Intro()
        {
            InitializeComponent();
            App.ViewModel.LoadData();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            //if (App.ViewModel.HasUserSeenIntro()) NavigationService.GoBack();

            base.OnNavigatedTo(e);
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            
        }
    }
}