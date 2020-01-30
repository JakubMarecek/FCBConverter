using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCBConverter
{
    class DependencyLoaderItem
    {
        public string fileName { set; get; }

        public List<string> depFiles { set; get; }

        public List<int> depTypes { set; get; }
    }
}
