using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Colab.Develop
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.GitHubDir = githubDirTextBox.Text;
            Properties.Settings.Default.DatabaseFolder = databaseDirTextBox.Text;
            Properties.Settings.Default.Save();
        }

        private void databaseTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            databaseDirTextBox.Text = Properties.Settings.Default.DatabaseFolder;
        }

        private void githubDirTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            githubDirTextBox.Text = Properties.Settings.Default.GitHubDir;
        }
    }
}
