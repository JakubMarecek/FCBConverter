using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCBConverter
{
    class DependentFile
    {
        public int DependencyFilesStartIndex { set; get; }

        public int CountOfDependencyFiles { set; get; }

        public ulong FileHash { set; get; }
    }
}
