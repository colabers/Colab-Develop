using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Colab.Develop
{
    public class SCApps : INotifyPropertyChanged
    {
        public static String CommonClientDir = @"$GitHubDir$\Colab.Common\Client";


        public List<IApp> List = new List<IApp>(){
             new GenericApp() {
                Name = "M",
                Executable = @"$GitHubDir$\DoctorM\run_develop.bat",
                Required = true,
                Checked = true,
            },
            new GenericApp() {
                Name = "Blending",
                Executable = @"$GitHubDir$\Blending\run.bat",
            },
            new GenericApp() {
                Name = "SignIn",
                Executable = @"$GitHubDir$\SignIn\run.bat",
            },
            new GenericApp() {
                Name = "UserAdmin",
                Executable = @"$GitHubDir$\UserAdmin\run.bat"
            },
            new GenericApp() {
                Name = "People",
                Executable = @"$GitHubDir$\People\run.bat",
            },
            new GenericApp() {
                Name = "DogFood",
                Executable = @"$GitHubDir$\DogFoodSB\run.bat",
            },
            new GenericApp() {
                Name = "Images",
                Executable = @"$GitHubDir$\Images\run.bat",
            },
            new GenericApp() {
                Name = "Colab_Website",
                Executable = @"$GitHubDir$\Colab.Website\run.bat",
            },
             new GenericApp() {
                Name = "Buildcraft_Framework",
                Executable = @"$GitHubDir$\Buildcraft.Framework\run.bat",
            },
            new GenericApp() {
                Name = "BIMProjects",
                Executable = @"$GitHubDir$\BIMProjects\run.bat",
            },
            new GenericApp() {
                Name = "Colab_Chatter",
                Executable = @"$GitHubDir$\Colab.Chatter\run_develop.bat",
            },
            new GenericApp() {
                Name = "Colab_ActionBoard",
                Executable = @"$GitHubDir$\Colab.ActionBoard\run_develop.bat",
            },
            new GenericApp() {
                Name = "Colab_Organizer",
                Executable = @"$GitHubDir$\Colab.Organizer\run_develop.bat",
            },
            new GenericApp() {
                Name = "Colab_Notifications",
                Executable = @"$GitHubDir$\Colab.Notifications\run_develop.bat",
            },
            new GenericApp() {
                Name = "Colab_ResourceAllocator",
                Executable = @"$GitHubDir$\Colab.ResourceAllocator\run_develop.bat",
            },
            new GenericApp() {
                Name = "Colab_Github",
                Executable = @"$GitHubDir$\Colab.GithubWrapper\run.bat",
            },
            new GenericApp() {
                Name = "Colab_DBTools",
                Executable = @"$GitHubDir$\Colab.DBTools\run.bat",
            },
             new GenericApp() {
                Name = "Malx_Targets",
                Executable = @"$GitHubDir$\Malx.Targets\run_standalone.bat",
            },
              new GenericApp() {
                Name = "ResourceBooking",
                Executable = @"$GitHubDir$\ResourceBooking\run.bat",
            },
        };

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
