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
using ICSharpCode.SharpZipLib.Zip;

namespace Colab.Develop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {

        public static String CommonClientDir = @"$GitHubDir$\Colab\Colab.Common\Client";

        private ObservableCollection<StarcounterDB> _SCDBs = new ObservableCollection<StarcounterDB>();

        public ObservableCollection<StarcounterDB> SCDBs
        {
            get { return _SCDBs; }
            set { _SCDBs = value; }
        }

        public List<StarcounterApp> SCApps = new List<StarcounterApp>(){
            new StarcounterApp() { 
                Name = "Launcher", 
                AppPath = @"$GitHubDir$\Launcher\bin\Debug\Launcher.exe",
                ResourceDirs = @"$GitHubDir$\Launcher\src\Launcher\wwwroot",
            },
            new StarcounterApp() { 
                Name = "SignIn", 
                AppPath = @"$GitHubDir$\SignIn\bin\Debug\SignIn.exe",
                ResourceDirs = $@"$GitHubDir$\SignIn\src\SignIn\wwwroot;{CommonClientDir}",
            },
            new StarcounterApp() { 
                Name = "Colab_Users", 
                AppPath = @"$GitHubDir$\Colab\Colab.UserManagement\bin\Debug\colab_users.exe",
                ResourceDirs = $@"$GitHubDir$\Colab\Colab.UserManagement\Client;{CommonClientDir}",
            },
            new StarcounterApp() { 
                Name = "Colab_Notifications", 
                AppPath = @"$GitHubDir$\Colab\Colab.Notifications\bin\Debug\colab_notifications.exe",
                ResourceDirs = $@"$GitHubDir$\Colab\Colab.Notifications\Client;{CommonClientDir}",
            }, 
            new StarcounterApp() { 
                Name = "Colab_Assets", 
                AppPath = @"$GitHubDir$\Colab\Colab.Assets\bin\Debug\colab_assets.exe",
                ResourceDirs = $@"$GitHubDir$\Colab\Colab.Assets\Client;{CommonClientDir}",
            }, 
            new StarcounterApp() { 
                Name = "Colab_Documents", 
                AppPath = @"$GitHubDir$\Colab\Colab.Documents\bin\Debug\colab_documents.exe",
                ResourceDirs = $@"$GitHubDir$\Colab\Colab.Documents\Client;{CommonClientDir}",
            },   
            new StarcounterApp() { 
                Name = "Colab_Organizer", 
                AppPath = @"$GitHubDir$\Colab\Colab.Organizer\bin\Debug\colab_organizer.exe",
                ResourceDirs = $@"$GitHubDir$\Colab\Colab.Organizer\Client;{CommonClientDir}",
            },
            new StarcounterApp() { 
                Name = "Colab_ActionBoard", 
                AppPath = @"$GitHubDir$\Colab\Colab.ActionBoard\bin\Debug\colab_actionboard.exe",
                ResourceDirs = $@"$GitHubDir$\Colab\Colab.ActionBoard\Client;{CommonClientDir}",
            },
            new StarcounterApp() { 
                Name = "Colab_Chatter", 
                AppPath = @"$GitHubDir$\Colab\Colab.Chatter\bin\Debug\colab_chatter.exe",
                ResourceDirs = $@"$GitHubDir$\Colab\Colab.Chatter\Client;{CommonClientDir}",
            },   
            new StarcounterApp() { 
                Name = "Colab_Terminal", 
                AppPath = @"$GitHubDir$\Colab\Colab.Terminal\bin\Debug\colab_terminal.exe",
                ResourceDirs = $@"$GitHubDir$\Colab\Colab.Terminal\Client;{CommonClientDir}",
            },
            new StarcounterApp() {
                Name = "Colab_Flowchart",
                AppPath = @"$GitHubDir$\Colab\Colab.FlowChart\bin\Debug\colab_flowchart.exe",
                ResourceDirs = $@"$GitHubDir$\Colab\Colab.FlowChart\Client;{CommonClientDir}",
            },
            new StarcounterApp() {
                Name = "Colab_QueryBuilder",
                AppPath = @"$GitHubDir$\Colab\Colab.QueryBuilder\bin\Debug\colab_querybuilder.exe",
                ResourceDirs = $@"$GitHubDir$\Colab\Colab.QueryBuilder\Client;{CommonClientDir}",
            },
            new StarcounterApp() { 
                Name = "Colab_Orders", 
                AppPath = @"$GitHubDir$\Colab\Colab.Orders\bin\Debug\colab_orders.exe",
                ResourceDirs = $@"$GitHubDir$\Colab\Colab.Orders\Client;{CommonClientDir}",
            },
            new StarcounterApp() {
                Name = "Cookie_consent",
                AppPath = @"$GitHubDir$\CookieConsent\src\CookieConsent\bin\Debug\CookieConsent.exe",
                ResourceDirs = $@"$GitHubDir$\CookieConsent\src\CookieConsent\wwwroot",
            },

        };

        private readonly StarcounterApp StopDBApp = new StarcounterApp()
        {
            Executable = "staradmin",
            CustomParameters = "--database=\"{Database}\" stop db"
        };

        private readonly BackgroundWorker databaseLoader = new BackgroundWorker();
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private readonly BackgroundWorker exportWorker = new BackgroundWorker();
        private readonly BackgroundWorker importWorker = new BackgroundWorker();
        private ICollectionView ValidDatabases;

        public MainWindow()
        {
            InitializeComponent();
            datagrid.Items.Clear();
            datagrid.ItemsSource = SCApps;
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.WorkerSupportsCancellation = true;

            databaseLoader.RunWorkerCompleted += databaseLoader_RunWorkerCompleted;
            databaseLoader.DoWork += databaseLoader_DoWork;

            exportWorker.DoWork += exportWorker_DoWork;
            importWorker.DoWork += importWorker_DoWork;

            LoadCheckedStatus();
            LoadDatabases();
            ValidDatabases = CollectionViewSource.GetDefaultView(SCDBs);
            ValidDatabases.Filter = item =>
                {
                    return (item as StarcounterDB).Valid;
                };
            DatabaseChooser.ItemsSource = ValidDatabases;
            LoadSelectedDatabase();
            UpdateRunningApps();
        }




        void databaseLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ValidDatabases.Refresh();
            UpdateRunningDBs();
        }

        void databaseLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (StarcounterDB db in SCDBs)
            {
                db.LoadConfiguration();
            }
        }

        private Boolean LoadingCheckedStatus;
        private void LoadCheckedStatus()
        {
            LoadingCheckedStatus = true;
            String[] checkedApps = Properties.Settings.Default.Checked.Split(';');
            foreach (StarcounterApp app in SCApps)
            {
                app.Checked = checkedApps.Contains(app.Name);
            }
            LoadingCheckedStatus = false;
        }

        private void LoadSelectedDatabase()
        {
            String dbname = Properties.Settings.Default.Database;
            foreach (StarcounterDB app in SCDBs)
            {
                if (dbname.ToLower() == app.Name.ToLower())
                {
                    DatabaseChooser.SelectedItem = app;
                    return;
                }
            }
            LoadingCheckedStatus = false;
        }

        private void SetStarcounterDatabaseFolder(String path)
        {
            Properties.Settings.Default.DatabaseFolder = path;
            Properties.Settings.Default.Save();
        }

        private String StarcounterDatabaseFolder
        {
            get
            {
                var sdf = Properties.Settings.Default.DatabaseFolder;
                if (String.IsNullOrEmpty(sdf))
                {
                    sdf = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        sdf = Directory.GetParent(sdf).ToString();
                    }
                    sdf += @"\Documents\Starcounter\Personal";
                    SetStarcounterDatabaseFolder(sdf);
                }
                return sdf;
            }
        }

        private void LoadDatabases()
        {
            SCDBs.Clear();
            foreach (String file in Directory.GetDirectories(StarcounterDatabaseFolder + @"\Databases"))
            {
                SCDBs.Add(new StarcounterDB(file));
            }
            databaseLoader.RunWorkerAsync();
        }

        private void SaveCheckedStatus()
        {
            String checkedApps = "";
            foreach (StarcounterApp app in SCApps)
            {
                if (app.Checked)
                    checkedApps += app.Name + ";";
            }
            Properties.Settings.Default.Checked = checkedApps;
            Properties.Settings.Default.Save();
        }

        public void UpdateRunningDBs()
        {
            try
            {
                ProcessStartInfo processStartInfo = CreateStartInfo("staradmin", "list db");
                Process process = new Process();
                process.StartInfo = processStartInfo;
                bool processStarted = process.Start();

                if (processStarted)
                {
                    //Get the output stream
                    process.WaitForExit();

                    //Display the result
                    StarcounterDB currentDB = null;
                    while (process.StandardOutput.Peek() > -1)
                    {
                        String line = process.StandardOutput.ReadLine().ToLower();
                        if (currentDB == null)
                        {
                            foreach (StarcounterDB db in SCDBs)
                            {
                                if (line.Contains(db.Name.ToLower()))
                                {
                                    currentDB = db;
                                }
                            }
                        }
                        else
                        {
                            if (line.Contains("running"))
                            {
                                currentDB.Running = line.Contains("true");
                                currentDB = null;
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
            }
        }

        public void UpdateRunningApps()
        {
            try
            {
                var database = GetSelectedDatabaseString(false);
                if (String.IsNullOrEmpty(database))
                    return;
                ProcessStartInfo processStartInfo = CreateStartInfo("staradmin", 
                    String.Format("--database={0} list app", database));
                Process process = new Process();
                process.StartInfo = processStartInfo;
                bool processStarted = process.Start();

                if (processStarted)
                {
                    //Get the output stream
                    process.WaitForExit();

                    //Display the result
                    String result = process.StandardOutput.ReadToEnd().ToLower();// outputReader.ReadToEnd().ToLower();
                    var db = database.ToLower();
                    foreach (StarcounterApp app in SCApps)
                    {
                        var appname = app.Name.ToLower();
                        app.Running = false;
                        using (StringReader sr = new StringReader(result))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                if (line.Contains(appname) && line.Contains(db))
                                {
                                    app.Running = true;
                                    break;
                                }
                            }
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
            }
        }


        private ProcessStartInfo CreateStartInfo(String app, String parameters)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(app, parameters);
            processStartInfo.ErrorDialog = false;
            processStartInfo.UseShellExecute = false;
            //processStartInfo.RedirectStandardError = true;
            //processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            return processStartInfo;
        }

        private void SetEnableAllApps(Boolean value)
        {
            foreach (StarcounterApp app in SCApps)
            {
                app.Enabled = value;
            }
        }

        private void SetAllApps(String property, object value)
        {
            Type type = typeof(StarcounterApp);
            PropertyInfo pi = type.GetProperty(property);
            if (pi != null)
            {
                foreach (StarcounterApp app in SCApps)
                {
                    pi.SetValue(app, value);
                }
            }
        }

        private Process StartApp(StarcounterApp app, String database)
        {
            ProcessStartInfo processStartInfo = CreateStartInfo(app.Executable, app.GetParameters(database));
            Process process = new Process();
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += process_OutputDataReceived;
            process.ErrorDataReceived += process_ErrorDataReceived;
            process.Exited += process_Exited;
            process.StartInfo = processStartInfo;
            if (process.Start())
            {
                app.Starting = true;
            }
            return process;
        }

        void process_Exited(object sender, EventArgs e)
        {
            waitForAppExitHandle.Set();
        }

        void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            consoleOutput.Dispatcher.BeginInvoke(new Action(() =>
            {
                consoleOutput.Text += "error: " + e.Data;
            }));
        }

        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            consoleOutput.Dispatcher.BeginInvoke(new Action(() =>
            {
                consoleOutput.Text += e.Data + "\n";
            }));
        }

        private IEnumerable<StarcounterApp> CheckedApps()
        {
            foreach (StarcounterApp app in SCApps)
            {
                if (app.Checked)
                {
                    yield return app;
                }
            }
        }

        private ConcurrentQueue<StartAppData> AppsToStart = new ConcurrentQueue<StartAppData>();
        AutoResetEvent waitForAppExitHandle = new AutoResetEvent(false);

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!AppsToStart.IsEmpty && !worker.CancellationPending)
            {
                StartAppData data;
                if (AppsToStart.TryDequeue(out data))
                {
                    try
                    {
                        var process = StartApp(data.App, data.Database);
                        var output = new List<string>();
                        process.BeginOutputReadLine();
                        waitForAppExitHandle.WaitOne();
                        data.App.Running = true;
                        data.App.Starting = false;
                        data.App.Enabled = true;
                        consoleOutput.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            foreach (String op in output)
                            {
                                consoleOutput.Text += op + "\n";
                            }
                            if (!String.IsNullOrEmpty(data.App.Name))
                                consoleOutput.Text += data.App.Name + " is running!\n";
                        }));
                        process.Dispose();
                    }
                    catch (Exception ec)
                    {
                        consoleOutput.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            consoleOutput.Text += ec.Message;
                        }));
                    }

                }
            }
        }

        private void worker_RunWorkerCompleted(object sender,
                                               RunWorkerCompletedEventArgs e)
        {
            restartAllButton.IsEnabled = true;
            stopAllButton.IsEnabled = true;
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

            consoleOutput.Text = "";
            restartAllButton.IsEnabled = false;
            if (worker.IsBusy)
            {
                worker.CancelAsync();
            }
            StartAppData dummy;
            while (AppsToStart.TryDequeue(out dummy)) { }
            AppsToStart.Enqueue(new StartAppData() { App = StopDBApp, Database = database });
            SetAllApps("Running", false);
            foreach (StarcounterApp app in CheckedApps())
            {
                app.Running = false;
                app.Enabled = false;
                AppsToStart.Enqueue(new StartAppData() { App = app, Database = database });
            }
            worker.RunWorkerAsync();
        }

        private void StopAllButton_Click(object sender, RoutedEventArgs e)
        {
            var database = GetSelectedDatabaseString();
            if (String.IsNullOrEmpty(database))
                return;

            consoleOutput.Text = "";
            stopAllButton.IsEnabled = false;
            if (worker.IsBusy)
            {
                worker.CancelAsync();
            }
            StartAppData dummy;
            while (AppsToStart.TryDequeue(out dummy)) { }
            AppsToStart.Enqueue(new StartAppData() { App = StopDBApp, Database = database });
            SetAllApps("Running", false);
            worker.RunWorkerAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var database = GetSelectedDatabaseString();
            if (String.IsNullOrEmpty(database))
                return;
            Button b = sender as Button;
            var app = b.DataContext as StarcounterApp;
            app.Enabled = false;
            AppsToStart.Enqueue(new StartAppData() { App = app, Database = database });
            if (!worker.IsBusy)
                worker.RunWorkerAsync();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            settings.IsOpen = true;
        }

        private void saveSettings_Click(object sender, RoutedEventArgs e)
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

        private void checkbox_onChecked(object sender, EventArgs e)
        {
            if (!LoadingCheckedStatus)
                SaveCheckedStatus();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatabasesTab.IsSelected)
            {
                UpdateRunningDBs();
            }
        }

        private void DatabaseExport_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            var db = b.DataContext as StarcounterDB;
            if (db.Running)
            {
                MessageBox.Show("You can not export a running DB at this time");
                return;
            }
            if (exportWorker.IsBusy)
            {
                MessageBox.Show("Already exporting a database");
                return;
            }
            if (db.Valid)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Starcounter zipped (*.scz)|*.scz";
                saveFileDialog.DefaultExt = "scz";
                saveFileDialog.OverwritePrompt = true;
                saveFileDialog.Title = String.Format("Export {0} database to scz (zipped) file", db.Name);
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                saveFileDialog.AddExtension = true;
                saveFileDialog.ValidateNames = true;
                var defaultFileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + String.Format(@"\{0}.scz", db.Name);
                int i = 1;
                while (File.Exists(defaultFileName))
                {
                    i++;
                    defaultFileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + String.Format(@"\{0}_{1}.scz", db.Name, i);
                }
                saveFileDialog.FileName = defaultFileName;
                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        if (File.Exists(saveFileDialog.FileName))
                            File.Delete(saveFileDialog.FileName);

                        dOutput.Text += String.Format("Exporting {0} to {1}...\n", db.Name, saveFileDialog.FileName);
                        exportWorker.RunWorkerAsync(new ExportDBData() { DB = db, SaveFileName = saveFileDialog.FileName });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        void exportWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ExportDBData data = e.Argument as ExportDBData;
            try
            {
                data.DB.Exporting = true;
                ZipOutputStream zip = new ZipOutputStream(File.Create(data.SaveFileName));
                zip.SetLevel(2);
                dOutput.Dispatcher.BeginInvoke(new Action(() =>
                {
                    dOutput.Text += String.Format("Compressing images...\n");
                }));
                AddFileToZip(zip, "database.0.sci", data.DB.Configuration.Runtime.ImageDirectory + String.Format(@"/{0}.0.sci", data.DB.Name));
                AddFileToZip(zip, "database.0.sci2", data.DB.Configuration.Runtime.ImageDirectory + String.Format(@"/{0}.0.sci2", data.DB.Name));
                AddFileToZip(zip, "database.1.sci", data.DB.Configuration.Runtime.ImageDirectory + String.Format(@"/{0}.1.sci", data.DB.Name));
                AddFileToZip(zip, "database.1.sci2", data.DB.Configuration.Runtime.ImageDirectory + String.Format(@"/{0}.1.sci2", data.DB.Name));
                dOutput.Dispatcher.BeginInvoke(new Action(() =>
                {
                    dOutput.Text += String.Format("Compressing transaction log...\n");
                }));
                AddFileToZip(zip, "database.0000000001.log", data.DB.Configuration.Runtime.TransactionLogDirectory + String.Format(@"/{0}.0000000001.log", data.DB.Name));
                AddFileToZip(zip, "database.0000000002.log", data.DB.Configuration.Runtime.TransactionLogDirectory + String.Format(@"/{0}.0000000002.log", data.DB.Name));
                dOutput.Dispatcher.BeginInvoke(new Action(() =>
                {
                    dOutput.Text += String.Format("Adding configuration file...\n");
                }));
                AddFileToZip(zip, "database.db.config", data.DB.ConfigurationFile);
                zip.Finish();
                zip.Close();
                data.DB.Exporting = false;
                dOutput.Dispatcher.BeginInvoke(new Action(() =>
                {
                    dOutput.Text += String.Format("Export complete!\n");
                }));
            }
            catch (Exception ex)
            {
                dOutput.Dispatcher.BeginInvoke(new Action(() =>
                {
                    dOutput.Text += String.Format("Export failed! " + ex.Message + "\n");
                }));
            }
            finally
            {
                data.DB.Exporting = false;
            }
        }

        private static void AddFileToZip(ZipOutputStream zStream, string zipfile, string file)
        {
            byte[] buffer = new byte[4096];

            ZipEntry entry = new ZipEntry(zipfile);
            entry.DateTime = DateTime.Now;
            zStream.PutNextEntry(entry);

            using (FileStream fs = File.OpenRead(file))
            {
                int sourceBytes;
                do
                {
                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                    zStream.Write(buffer, 0, sourceBytes);
                } while (sourceBytes > 0);
            }
        }

        private void StopDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button b = sender as Button;
                var db = b.DataContext as StarcounterDB;
                if (db.Running)
                {
                    ProcessStartInfo processStartInfo = CreateStartInfo("staradmin", String.Format("stop db {0}", db.Name));
                    Process process = new Process();
                    process.StartInfo = processStartInfo;
                    bool processStarted = process.Start();

                    if (processStarted)
                    {
                        //Get the output stream
                        process.WaitForExit();

                        //Display the result
                        while (process.StandardOutput.Peek() > -1)
                        {
                            String line = process.StandardOutput.ReadLine().ToLower();
                            dOutput.Text += line + '\n';
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
            }

        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            if (importWorker.IsBusy)
            {
                MessageBox.Show("Already importing a database");
                return;
            }
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Starcounter zipped (*.scz)|*.scz";
            openFileDialog.DefaultExt = "scz";
            openFileDialog.Title = String.Format("Import database from scz (zipped) file");
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.AddExtension = true;
            openFileDialog.ValidateNames = true;
            if (openFileDialog.ShowDialog() == true)
            {
                dOutput.Text += String.Format("Importing {0}...\n", openFileDialog.FileName);
                importWorker.RunWorkerAsync(openFileDialog.FileName);
            }
        }

        private Boolean DatabaseExistWithName(String dbname)
        {
            foreach (StarcounterDB db in SCDBs)
            {
                if (db.Name.ToLower() == dbname.ToLower())
                    return true;
            }
            return false;
        }

        private Boolean DatabaseExistWithPort(String port)
        {
            foreach (StarcounterDB db in SCDBs)
            {
                if (db.Configuration.Runtime.DefaultUserHttpPort == port)
                    return true;
            }
            return false;
        }
        public void AssureFolder(String foldername)
        {
            if (!Directory.Exists(foldername))
                Directory.CreateDirectory(foldername);
        }

        public Boolean CreateNewDB(String name)
        {
            try
            {
                ProcessStartInfo processStartInfo = CreateStartInfo("staradmin", 
                    String.Format("--database=\"{0}\" new db", name));
                Process process = new Process();
                process.StartInfo = processStartInfo;
                bool processStarted = process.Start();

                if (processStarted)
                {
                    //Get the output stream
                    process.WaitForExit();

                    //Display the result
                    while (process.StandardOutput.Peek() > -1)
                    {
                        String line = process.StandardOutput.ReadLine().ToLower();
                        if (line.Contains("Created"))
                        {
                            process.Dispose();
                            return true;
                        }
                    }
                    process.Dispose();
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        void importWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                String file = e.Argument as String;
                String destination = Properties.Settings.Default.DatabaseFolder;
                String dbname = Path.GetFileName(file).Split('.')[0];
                var i = 1;
                var tempdbname = dbname;
                while (DatabaseExistWithName(dbname))
                {
                    i++;
                    dbname = tempdbname + "_" + i.ToString();
                }

                dOutput.Dispatcher.BeginInvoke(new Action(() =>
                {
                    dOutput.Text += String.Format("Creating database {0}!\n", dbname);
                }));
                CreateNewDB(dbname);
             
                FileStream fr = File.OpenRead(file);
                ZipInputStream ins = new ZipInputStream(fr);
                ZipEntry ze = ins.GetNextEntry();
                String imageDir = destination + String.Format(@"\Data\{0}\{0}-imported", dbname);
                String TempDir = destination + String.Format(@"\Temp\{0}\{0}-imported", dbname);
                String configDir = destination + String.Format(@"\Databases\{0}", dbname);
                AssureFolder(imageDir);
                AssureFolder(TempDir);
                AssureFolder(configDir);

                while (ze != null)
                {
                    dOutput.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        dOutput.Text += String.Format("Extracting {0}...\n", ze.Name);
                    }));
                    switch (ze.Name.ToLower())
                    {
                        case "database.0.sci":
                            UnZipFile(ze, ins, imageDir + String.Format(@"\{0}.0.sci", dbname));
                            break;
                        case "database.0.sci2":
                            UnZipFile(ze, ins, imageDir + String.Format(@"\{0}.0.sci2", dbname));
                            break;
                        case "database.1.sci":
                            UnZipFile(ze, ins, imageDir + String.Format(@"\{0}.1.sci", dbname));
                            break;
                        case "database.1.sci2":
                            UnZipFile(ze, ins, imageDir + String.Format(@"\{0}.1.sci2", dbname));
                            break;
                        case "database.0000000001.log":
                            UnZipFile(ze, ins, imageDir + String.Format(@"\{0}.0000000001.log", dbname));
                            break;
                        case "database.0000000002.log":
                            UnZipFile(ze, ins, imageDir + String.Format(@"\{0}.0000000002.log", dbname));
                            break;
                        case "database.db.config":
                            UnZipFile(ze, ins, configDir + String.Format(@"\{0}.db.config", dbname));
                            break;
                    }
                    ze = ins.GetNextEntry();
                }
                ins.Close();
                fr.Close();
                dOutput.Dispatcher.BeginInvoke(new Action(() =>
                {
                    dOutput.Text += String.Format("Fixing db config!\n");
                }));
                StarcounterDB app = new StarcounterDB(configDir);
                app.LoadConfiguration();
                app.Configuration.Runtime.TransactionLogDirectory = imageDir;
                app.Configuration.Runtime.ImageDirectory = imageDir;
                app.Configuration.Runtime.TempDirectory = TempDir;
                while (DatabaseExistWithPort(app.Configuration.Runtime.DefaultUserHttpPort))
                {
                    app.Configuration.Runtime.DefaultUserHttpPort = 
                        (Convert.ToInt32(app.Configuration.Runtime.DefaultUserHttpPort)+1).ToString();
                }
                dOutput.Dispatcher.BeginInvoke(new Action(() =>
                {
                    dOutput.Text += String.Format("Choosed port {0}!\n", app.Configuration.Runtime.DefaultUserHttpPort);
                }));
                app.SaveConfiguration();


                dOutput.Dispatcher.BeginInvoke(new Action(() =>
                {
                    SCDBs.Add(app);
                    dOutput.Text += String.Format("Import complete!\n");
                    //((CollectionViewSource)this.Resources["ActiveDBList"]).View.Refresh();
                    //ActiveDBList.View.Refresh();
                }));
            }
            catch (Exception ex)
            {
                dOutput.Dispatcher.BeginInvoke(new Action(() =>
                {
                    dOutput.Text += String.Format("Error while importing: {0}\n", ex.Message);
                }));
            }
        }

        public static void UnZipFile(ZipEntry ze, ZipInputStream zis, string fileName)
        {
            FileStream fs = File.Create(fileName);
            byte[] writeData = new byte[ze.Size];
            int iteration = 0;
            while (true)
            {
                int size = 2048;
                size = zis.Read(writeData, (int)Math.Min(ze.Size, (iteration * 2048)), (int)Math.Min(ze.Size - (int)Math.Min(ze.Size, (iteration * 2048)), 2048));
                if (size > 0)
                {
                    fs.Write(writeData, (int)Math.Min(ze.Size, (iteration * 2048)), size);
                }
                else
                {
                    break;
                }
                iteration++;
            }
            fs.Close();
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            var item = sender as StarcounterDB;
            e.Accepted = item.Valid;
        }


        private void DatabaseChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StarcounterDB item = DatabaseChooser.SelectedItem as StarcounterDB;
            if (item != null)
            {
                Properties.Settings.Default.Database = item.Name;
                Properties.Settings.Default.Save();
            }
            UpdateRunningApps(); 
        }


    }

    public class StartAppData
    {
        public StarcounterApp App;
        public String Database;
    }

    public class ExportDBData
    {
        public StarcounterDB DB;
        public String SaveFileName;
    }
}

