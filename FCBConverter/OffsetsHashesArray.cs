using Gibbed.Dunia2.FileFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FCBConverter
{
    class OffsetsHashesArray
    {
        public static List<OffsetsHashesArrayItem> offsetsHashesArray = new List<OffsetsHashesArrayItem>();
        public static Dictionary<uint, ulong> offsetsHashesDict = new Dictionary<uint, ulong>();
        //public static Dictionary<uint, ulong> offsetsHashesDict2 = new Dictionary<uint, ulong>();

        public enum ParamNames : uint
        {
            ANIMPARAM = 3324928103,
            POSEANIMPARAM = 698988808,
            MOTIONMATCHINGPARAM = 2395932676,
            BLENDPARAM = 1608936243,
            CURVEPARAM = 3020094394,
            CLIPPARAM = 438514225,
            LAYERPARAM = 1304798653,
            LOOAKATPARAM = 3106312372,
            MOVEBLENDPARAM = 1606123814,
            MOVESTATEPARAM = 3105302744,
            PMSVALUEPARAM = 2803151549,
            RAGDOLLPARAM = 1761566118,
            SECONDARYMOTIONPARAM = 670538874,
            BLENDADJUSTPARAM = 1387024244
        }

        public static void Deserialize(Dictionary<uint, byte[]> node)
        {
            var offsetsArray = Helpers.UnpackArray(node[CRC32.Hash("offsetsArray")], 4);
            var hashesArray = Helpers.UnpackArray(node[CRC32.Hash("hashesArray")], 8);

            for (int i = 0; i < offsetsArray.Count(); i++)
            {
                offsetsHashesDict.Add(BitConverter.ToUInt32(offsetsArray[i], 0), BitConverter.ToUInt64(hashesArray[i], 0));

                //Program.offsetsHashesArray.Add(new OffsetsHashesArray(BitConverter.ToUInt32(offsetsArray[i], 0), BitConverter.ToUInt64(hashesArray[i], 0), node.NameHash));

                /*writer.WriteStartElement("fixup");
                writer.WriteAttributeString("offset", BitConverter.ToUInt32(offsetsArray[i], 0).ToString());
                writer.WriteAttributeString("hash", Helpers.ByteArrayToString(hashesArray[i]));*/
                /*
                byte[] hashesArrayRev = new byte[hashesArray[i].Length];
                for (int j = 0; j < hashesArray[i].Length; j++)
                {
                    hashesArrayRev[j] = hashesArray[i][j];
                }
                Array.Reverse(hashesArrayRev);
                ulong aaa = ulong.Parse(ByteArrayToString(hashesArrayRev), NumberStyles.HexNumber);
                if (FCBConverter.Program.m_HashList.ContainsKey(aaa))
                {
                    string name = FCBConverter.Program.m_HashList[aaa];
                    writer.WriteAttributeString("hash", name);
                }
                else
                    writer.WriteAttributeString("hash", ByteArrayToString(hashesArray[i]));
                */
                //writer.WriteEndElement();

                //Program.combinedMoveFileHelper.Add(node.NameHash.ToString("X8"), offsetsArray[i], hashesArray[i]);
            }
        }

        public static void Serialize(BinaryObject node)
        {
            if (!Enum.IsDefined(typeof(ParamNames), node.NameHash))
                return;

            List<byte[]> fixupsOffsets = new List<byte[]>();
            List<byte[]> fixupsHashes = new List<byte[]>();

            for (int i = 0; i < offsetsHashesArray.Count(); i++)
            {
                if ((uint)offsetsHashesArray[i].param == node.NameHash)
                {
                    fixupsOffsets.Add(BitConverter.GetBytes(offsetsHashesArray[i].offset));
                    fixupsHashes.Add(BitConverter.GetBytes(offsetsHashesArray[i].hash));
                }
            }

            if (fixupsOffsets.Count > 0)
            {
                fixupsOffsets.Insert(0, BitConverter.GetBytes(fixupsOffsets.Count));
                fixupsHashes.Insert(0, BitConverter.GetBytes(fixupsHashes.Count));

                node.Fields.Add(CRC32.Hash("offsetsArray"), fixupsOffsets.SelectMany(byteArr => byteArr).ToArray());
                node.Fields.Add(CRC32.Hash("hashesArray"), fixupsHashes.SelectMany(byteArr => byteArr).ToArray());
            }
        }
    }

    class OffsetsHashesArrayItem
    {
        public uint offset { get; set; }

        public ulong hash { get; set; }

        public OffsetsHashesArray.ParamNames param { get; set; }

        public OffsetsHashesArrayItem(uint offset, ulong hash, uint fixupsParam)
        {
            this.offset = offset;
            this.hash = hash;
            this.param = (OffsetsHashesArray.ParamNames)fixupsParam;
        }

        public OffsetsHashesArrayItem(uint offset, ulong hash, OffsetsHashesArray.ParamNames fixupsParam)
        {
            this.offset = offset;
            this.hash = hash;
            this.param = fixupsParam;
        }
    }
}
