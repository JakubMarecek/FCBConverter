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

namespace FCBConverter
{
    public enum CompressionScheme : byte
    {
        None = 0,
        LZO1x = 1,
        LZ4 = 2,
        Unknown3 = 3,
    }

    public struct FatEntry
    {
        public ulong NameHash;
        public uint UncompressedSize;
        public uint CompressedSize;
        public long Offset;
        public CompressionScheme CompressionScheme;
        public long AvailableSpace;
    }
}
