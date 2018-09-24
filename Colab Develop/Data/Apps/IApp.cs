using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Colab.Develop
{
    public interface IApp: INotifyPropertyChanged
    {
        string Name { get; }
        string Executable { get; }
        string GetParsedParameters(String database);

       
        Boolean Valid { get; }
        String ValidError { get; }

        Boolean Checked { get; set; }
        Boolean Running { get; set; }
        Boolean Enabled { get; set; }
        Boolean Starting { get; set; }
        Boolean Required { get; set; }
    }
}
