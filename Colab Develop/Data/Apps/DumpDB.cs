using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Colab.Develop
{
    public class DumpDB : GenericApp
    {
        public DumpDB() : base()
        { }

        public String ExportPath { get; set; }

        private String _executable;
        public override String Executable
        {
            get
            {
                return "%StarcounterBin%/Stardump/Stardump.dll";
            }
            set { _executable = value; }
        }

        public override String GetParsedParameters(String database)
        {
            var res = $"-db {database}";
            if (!String.IsNullOrEmpty(ExportPath))
            {
                res += $" -f{ExportPath}";
            }
            return res;
        }

    }
}
