using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Colab.Develop
{
    public class StarcounterApp : GenericApp
    {
        public StarcounterApp() : base()
        { }

        public String ResourceDirs { get; set; }

        public override String IsValid()
        {
            var exe = _executable.Replace("$GitHubDir$", Properties.Settings.Default.GitHubDir);
            if (!ExistsOnPath(exe))
            {
                return $"{exe} can not be found!";
            }
            return "";
        }

        private String _executable;
        public override String Executable
        {
            get
            {
                return "star";
            }
            set
            {
                _executable = value;
                OnPropertyChanged("Valid");
                OnPropertyChanged("ValidError");
            }
        }

        public override String GetParsedParameters(String database)
        {
            var res = $"-d={database}";
            if (!String.IsNullOrEmpty(ResourceDirs))
            {
                res += $" --resourcedir={ResourceDirs.Replace("$GitHubDir$", Properties.Settings.Default.GitHubDir)}";
            }
            if (!String.IsNullOrEmpty(_executable))
            {
                res += $" {_executable.Replace("$GitHubDir$", Properties.Settings.Default.GitHubDir)}";
            }
            return res;
        }

    }
}
