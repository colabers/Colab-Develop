using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;

namespace Colab.Develop
{
    public class GenericApp : IApp
    {
        public GenericApp()
        {
            Enabled = true;
            Checked = false;
            Running = false;
            Starting = false;
        }

        public Boolean Required { get; set; }

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



        public static bool ExistsOnPath(string fileName)
        {
            if (File.Exists(fileName))
                return true;

            var values = Environment.GetEnvironmentVariable("PATH");
            foreach (var path in values.Split(';'))
            {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                    return true;
            }
            return false;
        }


        public Boolean Valid => String.IsNullOrEmpty(IsValid());
        public String ValidError => IsValid();

        public virtual String IsValid()
        {
            if (!ExistsOnPath(Executable))
            {
                return $"{Executable} can not be found!";
            }
            return null;
        }

        private String _executable;
        public virtual String Executable
        {
            get
            {
                return _executable.Replace("$GitHubDir$", Properties.Settings.Default.GitHubDir);
            }
            set
            {
                if (value == _executable)
                    return;

                _executable = value;
                OnPropertyChanged("Executable");
                OnPropertyChanged("Valid");
                OnPropertyChanged("ValidError");
            }
        }

        public virtual String GetParsedParameters(String database)
        {
            if (!String.IsNullOrEmpty(Parameters))
            {
                return Parameters.Replace("{Database}", database).Replace("$GitHubDir$", Properties.Settings.Default.GitHubDir);
            }
            return "";
        }

        public String Parameters { get; set; }

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
