using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Colab.Develop
{
    public class StarcounterApp : INotifyPropertyChanged
    {
        public StarcounterApp()
        {
            Enabled = true;
            Checked = false;
            Running = false;
            Starting = false;
        }

        private String _name;
        public String Name
        {
            get { return _name; }
            set
            {
                if (value == _name)
                    return;

                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private Boolean _checked;
        public Boolean Checked
        {
            get { return _checked; }
            set
            {
                if (value == _checked)
                    return;

                _checked = value;
                OnPropertyChanged("Checked");
            }
        }

        private Boolean _running;
        public Boolean Running
        {
            get { return _running; }
            set
            {
                if (value == _running)
                    return;

                _running = value;
                OnPropertyChanged("Running");
            }
        }


        private Boolean _starting;
        public Boolean Starting
        {
            get { return _starting; }
            set
            {
                if (value == _starting)
                    return;

                _starting = value;
                OnPropertyChanged("Starting");
            }
        }

        private Boolean _enabled;
        public Boolean Enabled
        {
            get { return _enabled; }
            set
            {
                if (value == _enabled)
                    return;

                _enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        public String AppPath { get; set; }
        public String ResourceDirs { get; set; }



        private String _executable;
        public String Executable
        {
            get
            {
                if (String.IsNullOrEmpty(_executable))
                {
                    return "star";
                }
                else
                {
                    return _executable;
                }
            }
            set { _executable = value; }
        }

        public String GetParameters(String database)
        {
            if (String.IsNullOrEmpty(_customparameters))
            {
                var rd = ResourceDirs.Replace("$GitHubDir$", Properties.Settings.Default.GitHubDir);
                var ap = AppPath.Replace("$GitHubDir$", Properties.Settings.Default.GitHubDir);
                return String.Format("-d={0} --resourcedir=\"{1}\" \"{2}\"", database, rd, ap);
            }
            else
                return _customparameters.Replace("{Database}", database);
        }

        private String _customparameters;
        public String CustomParameters
        {

            set { _customparameters = value; }
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
