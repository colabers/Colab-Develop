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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using Microsoft.Win32;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Diagnostics;

namespace Colab.Develop
{
    /// <summary>
    /// Interaction logic for Databases.xaml
    /// </summary>
    public partial class Databases : Page
    {

        private readonly QueuedBackgroundWorker backgroundWorker = new QueuedBackgroundWorker();

        public Databases()
        {
            InitializeComponent();
            logGrid.DataContext = backgroundWorker;
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
            if (db.Valid)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Sqlite 3 (*.sqlite3)|*.sqlite3";
                saveFileDialog.DefaultExt = "sqlite3";
                saveFileDialog.OverwritePrompt = true;
                saveFileDialog.Title = $"Export {db.Name} database to sqlite3 file";
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                saveFileDialog.AddExtension = true;
                saveFileDialog.ValidateNames = true;
                var defaultFileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $@"\{db.Name}.sqlite3";
                int i = 1;
                while (File.Exists(defaultFileName))
                {
                    i++;
                    defaultFileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $@"\{db.Name}_{i}.sqlite3";
                }
                saveFileDialog.FileName = defaultFileName;
                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        if (File.Exists(saveFileDialog.FileName))
                            File.Delete(saveFileDialog.FileName);

                        var justname = Path.GetFileName(saveFileDialog.FileName);
                        backgroundWorker.Enqueue($"Exporting {db.Name} to file {justname}", () =>
                        {
                            try
                            {
                                db.Exporting = true;
                                ThreadedLog($"Exporting {db.Name} to file {saveFileDialog.FileName}");
                                var scpath = Environment.GetEnvironmentVariable("StarcounterBin");
                                Run($@"{scpath}\stardump\stardump.dll",
                                    $"unload --database \"{db.Name}\" -f \"{saveFileDialog.FileName}\"");
                                ThreadedLog("Export complete!");
                            }
                            catch (Exception ex)
                            {
                                ThreadedLog($"Export failed! {ex.Message}");
                            }
                            finally
                            {
                                db.Exporting = false;
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }


        private Process StartApp(IApp app, String database)
        {
            ProcessStartInfo processStartInfo;
            processStartInfo = ProcessHelper.CreateStartInfo(app.Executable, app.GetParsedParameters(database));
            Process process = new Process();
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += process_OutputDataReceived;
            process.ErrorDataReceived += process_ErrorDataReceived;
            process.StartInfo = processStartInfo;
            if (process.Start())
            {
                app.Starting = true;
            }
            return process;
        }

        void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            ThreadedLog($"Error: {e.Data}");
        }

        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            ThreadedLog(e.Data);
        }

        private void ThreadedLog(String message)
        {
            dOutput.Dispatcher.BeginInvoke(new Action(() =>
            {
                dOutput.Text += $"{message}\n";
            }));
        }

        private void InMainThread(Action action)
        {
            dOutput.Dispatcher.BeginInvoke(new Action(() =>
            {
                action();
            }));
        }

        private Boolean Run(String app, String pars)
        {
            try
            {
                ProcessStartInfo processStartInfo = ProcessHelper.CreateStartInfo(app, pars);
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
                        ThreadedLog(line);
                    }
                    process.Dispose();
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Starcounter sqlite3 dump (*.sqlite3)|*.sqlite3";
            openFileDialog.DefaultExt = "sqlite3";
            openFileDialog.Title = String.Format("Import database from sqlite3 file");
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.AddExtension = true;
            openFileDialog.ValidateNames = true;
            if (openFileDialog.ShowDialog() == true)
            {
                String suggestion = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                String justname = Path.GetFileName(openFileDialog.FileName);
                string name = PromptDialog.Prompt("Name of imported db", $"Importing {openFileDialog.FileName}", suggestion);
                if (String.IsNullOrEmpty(name) || DatabaseExistWithName(name))
                {
                    MessageBox.Show($"A database with name \"{name}\" already exist or is not supported!");
                    return;
                }
                backgroundWorker.Enqueue($"Importing file {justname} into new db {name}", () =>
                {
                    try
                    {
                        ThreadedLog($"Creating db {name}");
                        Run("staradmin", $"new db {name}");
                        ThreadedLog($"Importing {openFileDialog.FileName}");
                        var scpath = Environment.GetEnvironmentVariable("StarcounterBin");
                        Run($@"{scpath}\stardump\stardump.dll", $"reload --database \"{name}\" -f \"{openFileDialog.FileName}\"");
                        InMainThread(() =>
                        {
                            (DataContext as MainDataView).DBs.LoadDatabases();
                            dOutput.Text += String.Format("Import complete!\n");
                        });
                    }
                    catch (Exception ex)
                    {
                        ThreadedLog($"Error while importing: {ex.Message}\n");
                    }
                });
            }
        }

        private void CreateNewDB(String name, Action after = null)
        {
            backgroundWorker.Enqueue($"Creating new db {name}", () =>
            {
                ThreadedLog($"Creating database {name}...");
                Run("staradmin", $"new db {name}");
                after?.Invoke();
                ThreadedLog($"Database created!");
            });
        }

        private void NewDB_Click(object sender, RoutedEventArgs e)
        {
            string name = PromptDialog.Prompt("Name of new db", "Create new");
            if (String.IsNullOrEmpty(name) || DatabaseExistWithName(name))
            {
                MessageBox.Show($"A database with name \"{name}\" already exist or is not supported!");
                return;
            }
            CreateNewDB(name, () =>
            {
                InMainThread(() =>
                {
                    (DataContext as MainDataView).DBs.LoadDatabases();
                });
            });
        }

        private void StopDatabase(StarcounterDB db)
        {
            backgroundWorker.Enqueue($"Stopping db {db.Name}", () =>
            {
                Run("staradmin", $"stop db {db.Name}");
                db.Running = false;
            });
        }

        private void StopDB_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            var db = b.DataContext as StarcounterDB;
            if (db.Running)
            {
                StopDatabase(db);
            }
        }

        private void DeleteDB(StarcounterDB db, Action after = null)
        {
            backgroundWorker.Enqueue($"Deleting db {db.Name}", () =>
            {
                Run("staradmin", $"delete db {db.Name} --force");
                after?.Invoke();
            });
        }

        private void DeleteDB_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                Button b = sender as Button;
                var db = b.DataContext as StarcounterDB;
                if (db.Running)
                {
                    StopDatabase(db);
                }
                DeleteDB(db, () =>
                {
                    InMainThread(() =>
                    {
                        (DataContext as MainDataView).DBs.All.Remove(db);
                    });
                });
            }
        }

        private void ResetDB_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Are you sure?", "Reset Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                Button b = sender as Button;
                var db = b.DataContext as StarcounterDB;
                StopDatabase(db);
                DeleteDB(db);
                CreateNewDB(db.Name);
            }
        }

        private Boolean DatabaseExistWithName(String dbname)
        {
            foreach (StarcounterDB db in (DataContext as MainDataView).DBs.All)
            {
                if (db.Name.ToLower() == dbname.ToLower())
                    return true;
            }
            return false;
        }

        private Boolean DatabaseExistWithPort(String port)
        {
            foreach (StarcounterDB db in (DataContext as MainDataView).DBs.All)
            {
                if (db.Configuration.Runtime.DefaultUserHttpPort == port)
                    return true;
            }
            return false;
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            var item = sender as StarcounterDB;
            e.Accepted = item.Valid;
        }

        private void Page_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is MainDataView)
            {
                var dc = DataContext as MainDataView;
                databasesgrid.ItemsSource = dc.DBs.Valid;
            }
        }
    }

    public class ExportDBData
    {
        public StarcounterDB DB;
        public String SaveFileName;
    }

    public class ImportDBData
    {
        public String FilePath;
        public String DBName;
    }
}
