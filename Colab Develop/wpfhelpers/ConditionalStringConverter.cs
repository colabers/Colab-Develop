using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Data;

namespace Colab.Develop
{
    // This returns true if sale is on or above targe, otherwise false
    public class ConditionalStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var istrue = System.Convert.ToBoolean(value);
            String alternativesString = System.Convert.ToString(parameter);
            String[] alternatives = alternativesString.Split(';'); 
            return (istrue? alternatives[0] : alternatives[1]);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
