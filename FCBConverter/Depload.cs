/* 
 * FCBConverter
 * Copyright (C) 2020  Jakub Mareček (info@jakubmarecek.cz)
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with FCBConverter.  If not, see <https://www.gnu.org/licenses/>.
 */

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
