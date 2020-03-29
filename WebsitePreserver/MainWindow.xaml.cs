using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Windows;
using WebsitePreserver.ViewModels;

namespace WebsitePreserver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private MainViewModel MainViewModel;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainViewModel = new MainViewModel(DialogCoordinator.Instance);
            DataContext = MainViewModel;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                MainViewModel.CTS.Cancel();
            }
            catch (Exception) { }

            Application.Current.Shutdown();
        }
    }
}
