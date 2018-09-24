using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Colab.Develop
{
    public class MainDataView: INotifyPropertyChanged
    {
        public SCDatabases DBs { get; set; }
        public SCApps Apps { get; set; }

        public MainDataView()
        {
            DBs = new SCDatabases();
            Apps = new SCApps();
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
