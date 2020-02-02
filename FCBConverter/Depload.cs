using System.Collections.Generic;

namespace FCBConverter.Depload
{
    class DependentFile
    {
        public int DependencyFilesStartIndex { set; get; }

        public int CountOfDependencyFiles { set; get; }

        public ulong FileHash { set; get; }
    }

    class DependencyLoaderItem
    {
        public string fileName { set; get; }

        public List<string> depFiles { set; get; }

        public List<int> depTypes { set; get; }
    }
}
