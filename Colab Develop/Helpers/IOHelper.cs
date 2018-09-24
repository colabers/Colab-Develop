using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Colab.Develop
{
    public static class IOHelper
    {
        public static void AssureFolder(String foldername)
        {
            if (!Directory.Exists(foldername))
                Directory.CreateDirectory(foldername);
        }
    }
}
