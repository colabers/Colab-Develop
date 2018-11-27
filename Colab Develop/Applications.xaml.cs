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
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Threading;
using System.Reflection;

namespace Colab.Develop
{
    /// <summary>
    /// Interaction logic for Applications.xaml
    /// </summary>
    public partial class Applications : Page
    {
        private readonly QueuedBackgroundWorker queuedworker = new QueuedBackgroundWorker();

        public Applications()
        {
            InitializeComponent();
            datagrid.Items.Clear();
            logGrid.DataContext = queuedworker;
        }

        private void LoadSelectedDatabase()
        {
            String dbname = Properties.Settings.Default.Database;
            foreach (StarcounterDB app in (DataContext as MainDataView).DBs.All)
            {
                if (dbname.ToLower() == app.Name.ToLower())
                {
                    DatabaseChooser.SelectedItem = app;
                    return;
                }
            }
            LoadingCheckedStatus = false;
        }

        private Boolean LoadingCheckedStatus;
        private void LoadCheckedStatus()
        {
            LoadingCheckedStatus = true;
            String[] checkedApps = Properties.Settings.Default.Checked.Split(';');
            foreach (IApp app in (DataContext as MainDataView).Apps.List)
            {
                app.Checked = checkedApps.Contains(app.Name) || app.Required;
            }
            LoadingCheckedStatus = false;
        }

        private void Checkbox_onChecked(object sender, EventArgs e)
        {
            if (!LoadingCheckedStatus)
                SaveCheckedStatus();
        }
        
        private IEnumerable<IApp> CheckedApps()
        {
            foreach (IApp app in (DataContext as MainDataView).Apps.List)
            {
                if (app.Checked)
                {
                    yield return app;
                }
            }
        }

        private void SaveCheckedStatus()
        {
            String checkedApps = "";
            foreach (IApp app in (DataContext as MainDataView).Apps.List)
            {
                if (app.Checked)
                    checkedApps += app.Name + ";";
            }
            Properties.Settings.Default.Checked = checkedApps;
            Properties.Settings.Default.Save();
        }

        private void StartApp(IApp app, String database)
        {
            app.Enabled = false;
            queuedworker.Enqueue($"Starting {app.Name}", () =>
                {
                    ThreadedLog($"Starting {app.Name} in {database}...");
                    if (!app.Valid)
                    {
                        ThreadedLog($"Error: {app.ValidError}");
                        return;
                    }
                    app.Starting = true;
                    var output = ProcessHelper.Run(app.Executable, app.GetParsedParameters(database), consoleOutput);
                    InMainThread(() =>
                        {
                            var doc = consoleOutput.Document;
                            Paragraph para = new Paragraph();
                            bool skipnext = false;
                            foreach (var line in output)
                            {
                                if (line.Contains("info") || line.StartsWith(":")) //Fix for stupid SC messages
                                {
                                    skipnext = true;
                                    continue;
                                }
                                if (!skipnext)
                                    para.Inlines.Add(new Run(line));
                                skipnext = false;
                            }
                            doc.Blocks.Add(para);
                        });
                    app.Running = true;
                    app.Starting = false;
                    app.Enabled = true;
                    ThreadedLog($"{app.Name} is running!");
                });
        }

        private void StartApp_Click(object sender, RoutedEventArgs e)
        {
            var database = GetSelectedDatabaseString();
            if (String.IsNullOrEmpty(database))
                return;
            Button b = sender as Button;
            var app = b.DataContext as IApp;
            StartApp(app, database);
        }

        private void SetEnableAllApps(Boolean value)
        {
            foreach (IApp app in (DataContext as MainDataView).Apps.List)
            {
                app.Enabled = value;
            }
        }

        private void SetAllApps(String property, object value)
        {
            Type type = typeof(IApp);
            PropertyInfo pi = type.GetProperty(property);
            if (pi != null)
            {
                foreach (IApp app in (DataContext as MainDataView).Apps.List)
                {
                    pi.SetValue(app, value);
                }
            }
        }

        private String GetSelectedDatabaseString(Boolean verbose = true)
        {
            if (DatabaseChooser.SelectedItem == null)
            {
                if (verbose)
                    MessageBox.Show("No database selected");
                return "";
            }
            if (!(DatabaseChooser.SelectedItem as StarcounterDB).Valid)
            {
                if (verbose)
                    MessageBox.Show("Selected database is not valid");
                return "";
            }
            return (DatabaseChooser.SelectedItem as StarcounterDB).Name;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var database = GetSelectedDatabaseString();
            if (String.IsNullOrEmpty(database))
                return;
            consoleOutput.Document.Blocks.Clear(); 
            if (queuedworker.IsBusy)
            {
                queuedworker.CancelAsync();
            }
            StopDatabase(database);
            SetAllApps("Running", false);
            foreach (IApp app in CheckedApps())
            {
                if (app.Valid)
                {
                
                    app.Running = false;
                    app.Enabled = false;
                    StartApp(app, database);
                }
                else
                {
                    app.Running = false;
                    ThreadedLog($"Skipping nonvalid app {app.Name}");
                    ThreadedLog($"Error: {app.ValidError}");
                }
            }
        }

        private void StopDatabase(String database)
        {
            queuedworker.Enqueue("Stopping database", () => {
                ThreadedLog($"Stopping database {database}...");
                ProcessHelper.Run("staradmin", $"--database=\"{database}\" stop db");
                ThreadedLog("Database stopped!");
            });
        }

        private void StopAllButton_Click(object sender, RoutedEventArgs e)
        {
            var database = GetSelectedDatabaseString();
            if (String.IsNullOrEmpty(database))
                return;
            consoleOutput.Document.Blocks.Clear(); 
            if (queuedworker.IsBusy)
            {
                queuedworker.CancelAsync();
            }
            StopDatabase(database);
            SetAllApps("Running", false);
        }

        private void InMainThread(Action action)
        {
            consoleOutput.Dispatcher.BeginInvoke(new Action(() =>
            {
                action();
            }));
        }

        private void ThreadedLog(String message)
        {
            consoleOutput.Dispatcher.BeginInvoke(new Action(() =>
            {
                consoleOutput.LogInfo(message);
            }));
        }

        public void UpdateRunningApps()
        {
            var database = GetSelectedDatabaseString(false);
            if (String.IsNullOrEmpty(database))
                return;

            queuedworker.Enqueue("Updating running apps", () =>
            {
                try
                {
                    ThreadedLog($"Updating running apps for db {database}...");
                    var result = ProcessHelper.Run("staradmin", $"--database={database} list app", consoleOutput);
                    //Parse the interesting lines
                    var dbsearchline = $"(in {database.ToLower()})";
                    List<String> apps = new List<string>();
                    foreach (String line in result)
                    {
                        var index = !String.IsNullOrEmpty(line) ? line.IndexOf(dbsearchline) : -1;
                        if (index > 0)
                            apps.Add(line.Substring(0, index - 1));
                    }

                    InMainThread(() =>
                    {
                        foreach (IApp app in (DataContext as MainDataView).Apps.List)
                        {
                            var appname = app.Name.ToLower();
                            app.Running = false;
                            if (apps.Contains(appname))
                                app.Running = true;
                        }
                    });
                    ThreadedLog("Done!");
                }
                catch (Exception ex)
                {
                    ThreadedLog($"Failed: {ex.Message}");
                }
            });
        }

        private void DatabaseChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatabaseChooser.SelectedItem is StarcounterDB item)
            {
                Properties.Settings.Default.Database = item.Name;
                Properties.Settings.Default.Save();
            }
            UpdateRunningApps();
        }

        private void Page_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is MainDataView)
            {
                var dc = DataContext as MainDataView;
                datagrid.ItemsSource = dc.Apps.List;
                LoadCheckedStatus();
                DatabaseChooser.ItemsSource = dc.DBs.Valid;
                LoadSelectedDatabase();
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            consoleOutput.Document.Blocks.Clear(); 
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateRunningApps();
        }
    }
}
