using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Data;
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Colab.Develop
{
    // This returns true if sale is on or above targe, otherwise false
    public class SCDatabases:INotifyPropertyChanged
    {
        private readonly BackgroundWorker databaseLoader = new BackgroundWorker();
        public ICollectionView Valid { get; set; }
        public ObservableCollection<StarcounterDB> All { get; set; }


        public SCDatabases()
        {
            All = new ObservableCollection<StarcounterDB>();
            databaseLoader.RunWorkerCompleted += databaseLoader_RunWorkerCompleted;
            databaseLoader.DoWork += databaseLoader_DoWork;
            LoadDatabases();
        }

        void databaseLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Valid.Refresh();
            UpdateRunningDBs();
        }

        void databaseLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            foreach (StarcounterDB db in All)
            {
              //  db.LoadConfiguration();
            }
        }

        public void UpdateRunningDBs()
        {
            try
            {
                ProcessStartInfo processStartInfo = ProcessHelper.CreateStartInfo("staradmin", "list db");
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
                            foreach (StarcounterDB db in All)
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
            catch (Exception) {}
        }

        public static void SetStarcounterDatabaseFolder(String path)
        {
            Properties.Settings.Default.DatabaseFolder = path;
            Properties.Settings.Default.Save();
        }

        public static String StarcounterDatabaseFolder
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

        public void LoadDatabases()
        {
            All.Clear();
            foreach (String file in Directory.GetDirectories(StarcounterDatabaseFolder + @"\Databases"))
            {
                All.Add(new StarcounterDB(file));
            }
            databaseLoader.RunWorkerAsync();
            Valid = CollectionViewSource.GetDefaultView(All);
            Valid.Filter = item =>
            {
                return (item as StarcounterDB).Valid;
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

}
