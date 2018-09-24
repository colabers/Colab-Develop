using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using Microsoft.Win32;

namespace Colab.Develop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainDataView();
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            settings.IsOpen = true;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatabasesTab.IsSelected)
            {
              //  UpdateRunningDBs();
            }
        }

        private void Frame_Loaded(object sender, RoutedEventArgs e)
        {
            Frame f = sender as Frame;
            (f.Content as Page).DataContext = DataContext;
        }
    }

    public class StartAppData
    {
        public IApp App;
        public String Database;
    }


}

