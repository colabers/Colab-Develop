using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;

namespace Tunity.Develop
{
    public class StarcounterDB : INotifyPropertyChanged
    {
        public static XmlSerializer serializer = new XmlSerializer(typeof(Database));

        public StarcounterDB(String configurationPath)
        {
            Enabled = true;
            Checked = false;
            Running = false;
            Exporting = false;
            ConfigurationPath = configurationPath;
            Name = Path.GetFileName(configurationPath);
        }

        private Database _configuration;
        public Database Configuration
        {
            get { return _configuration; }
            set { _configuration = value; }
        }

        public String ConfigurationFile
        {
            get
            {
                return ConfigurationPath + String.Format(@"\{0}.db.config", Name);
            }
        }
        public void SaveConfiguration()
        {
            StreamWriter writer = new StreamWriter(ConfigurationFile);
            try
            {
                serializer.Serialize(writer, _configuration);
            }
            finally
            {
                writer.Close();
            }

            OnPropertyChanged("Valid");
        }

        public void LoadConfiguration()
        {
            if (File.Exists(ConfigurationFile))
            {
                StreamReader reader = new StreamReader(ConfigurationFile);
                try
                {
                    _configuration = (Database)serializer.Deserialize(reader);
                }
                finally
                {
                    reader.Close();
                }
            }
            OnPropertyChanged("Valid");
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

        public Boolean Valid
        {
            get
            {
                return ((_configuration != null) &&
                Directory.Exists(_configuration.Runtime.TempDirectory) &&
                Directory.Exists(_configuration.Runtime.TransactionLogDirectory) &&
                Directory.Exists(_configuration.Runtime.ImageDirectory));
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


        private Boolean _exporting;
        public Boolean Exporting
        {
            get { return _exporting; }
            set
            {
                if (value == _exporting)
                    return;

                _exporting = value;
                OnPropertyChanged("Exporting");
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
        public String ConfigurationPath { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }


    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.starcounter.com/configuration")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.starcounter.com/configuration", IsNullable = false)]
    public partial class Database
    {

        private DatabaseRuntime runtimeField;

        private DatabaseMonitoring monitoringField;

        private object traceSourcesField;

        /// <remarks/>
        public DatabaseRuntime Runtime
        {
            get
            {
                return this.runtimeField;
            }
            set
            {
                this.runtimeField = value;
            }
        }

        /// <remarks/>
        public DatabaseMonitoring Monitoring
        {
            get
            {
                return this.monitoringField;
            }
            set
            {
                this.monitoringField = value;
            }
        }

        /// <remarks/>
        public object TraceSources
        {
            get
            {
                return this.traceSourcesField;
            }
            set
            {
                this.traceSourcesField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.starcounter.com/configuration")]
    public partial class DatabaseRuntime
    {

        private bool loadEditionLibrariesField;

        private bool wrapJsonInNamespacesField;

        private bool enforceURINamespacesField;

        private bool mergeJsonSiblingsField;

        private bool uriMappingEnabledField;

        private bool ontologyMappingEnabledField;

        private string tempDirectoryField;

        private string imageDirectoryField;

        private string transactionLogDirectoryField;

        private string sQLProcessPortField;

        private string defaultUserHttpPortField;

        private string defaultSessionTimeoutMinutesField;

        private string chunksNumberField;

        private string schedulerCountField;

        /// <remarks/>
        public bool LoadEditionLibraries
        {
            get
            {
                return this.loadEditionLibrariesField;
            }
            set
            {
                this.loadEditionLibrariesField = value;
            }
        }

        /// <remarks/>
        public bool WrapJsonInNamespaces
        {
            get
            {
                return this.wrapJsonInNamespacesField;
            }
            set
            {
                this.wrapJsonInNamespacesField = value;
            }
        }

        /// <remarks/>
        public bool EnforceURINamespaces
        {
            get
            {
                return this.enforceURINamespacesField;
            }
            set
            {
                this.enforceURINamespacesField = value;
            }
        }

        /// <remarks/>
        public bool MergeJsonSiblings
        {
            get
            {
                return this.mergeJsonSiblingsField;
            }
            set
            {
                this.mergeJsonSiblingsField = value;
            }
        }

        /// <remarks/>
        public bool UriMappingEnabled
        {
            get
            {
                return this.uriMappingEnabledField;
            }
            set
            {
                this.uriMappingEnabledField = value;
            }
        }

        /// <remarks/>
        public bool OntologyMappingEnabled
        {
            get
            {
                return this.ontologyMappingEnabledField;
            }
            set
            {
                this.ontologyMappingEnabledField = value;
            }
        }

        /// <remarks/>
        public string TempDirectory
        {
            get
            {
                return this.tempDirectoryField;
            }
            set
            {
                this.tempDirectoryField = value;
            }
        }

        /// <remarks/>
        public string ImageDirectory
        {
            get
            {
                return this.imageDirectoryField;
            }
            set
            {
                this.imageDirectoryField = value;
            }
        }

        /// <remarks/>
        public string TransactionLogDirectory
        {
            get
            {
                return this.transactionLogDirectoryField;
            }
            set
            {
                this.transactionLogDirectoryField = value;
            }
        }

        /// <remarks/>
        public string SQLProcessPort
        {
            get
            {
                return this.sQLProcessPortField;
            }
            set
            {
                this.sQLProcessPortField = value;
            }
        }

        /// <remarks/>
        public string DefaultUserHttpPort
        {
            get
            {
                return this.defaultUserHttpPortField;
            }
            set
            {
                this.defaultUserHttpPortField = value;
            }
        }

        /// <remarks/>
        public string DefaultSessionTimeoutMinutes
        {
            get
            {
                return this.defaultSessionTimeoutMinutesField;
            }
            set
            {
                this.defaultSessionTimeoutMinutesField = value;
            }
        }

        /// <remarks/>
        public string ChunksNumber
        {
            get
            {
                return this.chunksNumberField;
            }
            set
            {
                this.chunksNumberField = value;
            }
        }

        /// <remarks/>
        public string SchedulerCount
        {
            get
            {
                return this.schedulerCountField;
            }
            set
            {
                this.schedulerCountField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.starcounter.com/configuration")]
    public partial class DatabaseMonitoring
    {

        private object maxRestartNumberField;

        private object resetFailureCountPeriodField;

        private object gracePeriodAfterConnectionLostField;

        private string startupTypeField;

        private string monitoringTypeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public object MaxRestartNumber
        {
            get
            {
                return this.maxRestartNumberField;
            }
            set
            {
                this.maxRestartNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public object ResetFailureCountPeriod
        {
            get
            {
                return this.resetFailureCountPeriodField;
            }
            set
            {
                this.resetFailureCountPeriodField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        public object GracePeriodAfterConnectionLost
        {
            get
            {
                return this.gracePeriodAfterConnectionLostField;
            }
            set
            {
                this.gracePeriodAfterConnectionLostField = value;
            }
        }

        /// <remarks/>
        public string StartupType
        {
            get
            {
                return this.startupTypeField;
            }
            set
            {
                this.startupTypeField = value;
            }
        }

        /// <remarks/>
        public string MonitoringType
        {
            get
            {
                return this.monitoringTypeField;
            }
            set
            {
                this.monitoringTypeField = value;
            }
        }
    }


}
