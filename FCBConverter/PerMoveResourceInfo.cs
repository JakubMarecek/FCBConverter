using Gibbed.Dunia2.FileFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FCBConverter
{
    class PerMoveResourceInfo
    {
        public static List<PerMoveResourceInfoItem> perMoveResourceInfos = new List<PerMoveResourceInfoItem>();

        public static void Deserialize(Dictionary<uint, byte[]> node)
        {
            var sizes = Helpers.UnpackArray(node[CRC32.Hash("sizes")], 4);
            var rootNodeIds = Helpers.UnpackArray(node[CRC32.Hash("rootNodeIds")], 8);
            var resourcePathIds = Helpers.UnpackArray(node[CRC32.Hash("resourcePathIds")], 8);

            for (int i = 0; i < sizes.Count(); i++)
            {
                perMoveResourceInfos.Add(new PerMoveResourceInfoItem(BitConverter.ToUInt32(sizes[i], 0), BitConverter.ToUInt64(rootNodeIds[i], 0), BitConverter.ToUInt64(resourcePathIds[i], 0)));

                /*writer.WriteStartElement("PerMoveResourceInfo");
                writer.WriteAttributeString("size", BitConverter.ToUInt32(sizes[i], 0).ToString());
                writer.WriteAttributeString("rootNodeId", Helpers.ByteArrayToString(rootNodeIds[i]));
                writer.WriteAttributeString("resourcePathId", Helpers.ByteArrayToString(resourcePathIds[i]));*/

                /*
                byte[] resourcePathIdsRev = new byte[resourcePathIds[i].Length];
                for (int j = 0; j < resourcePathIds[i].Length; j++)
                {
                    resourcePathIdsRev[j] = resourcePathIds[i][j];
                }
                Array.Reverse(resourcePathIdsRev);
                ulong aaa = ulong.Parse(ByteArrayToString(resourcePathIdsRev), NumberStyles.HexNumber);
                if (FCBConverter.Program.m_HashList.ContainsKey(aaa))
                {
                    string name = FCBConverter.Program.m_HashList[aaa];
                    writer.WriteAttributeString("resourcePathId", name);
                }
                else
                    writer.WriteAttributeString("resourcePathId", ByteArrayToString(resourcePathIds[i]));
                */
                //writer.WriteEndElement();

                //Program.combinedMoveFileHelper.AddSizes(sizes[i], resourcePathIds[i]);
            }
        }

        public static void Serialize(BinaryObject node)
        {
            if (node.NameHash != 1650810464)
                return;

            List<byte[]> sizes = new List<byte[]>();
            List<byte[]> rootNodeIds = new List<byte[]>();
            List<byte[]> resourcePathIds = new List<byte[]>();

            for (int i = 0; i < perMoveResourceInfos.Count(); i++)
            {
                sizes.Add(BitConverter.GetBytes(perMoveResourceInfos[i].size));
                rootNodeIds.Add(BitConverter.GetBytes(perMoveResourceInfos[i].rootNodeId));
                resourcePathIds.Add(BitConverter.GetBytes(perMoveResourceInfos[i].resourcePathId));
            }

            if (sizes.Count > 0)
            {
                sizes.Insert(0, BitConverter.GetBytes(sizes.Count));
                rootNodeIds.Insert(0, BitConverter.GetBytes(rootNodeIds.Count));
                resourcePathIds.Insert(0, BitConverter.GetBytes(resourcePathIds.Count));

                node.Fields.Add(CRC32.Hash("sizes"), sizes.SelectMany(byteArr => byteArr).ToArray());
                node.Fields.Add(CRC32.Hash("rootNodeIds"), rootNodeIds.SelectMany(byteArr => byteArr).ToArray());
                node.Fields.Add(CRC32.Hash("resourcePathIds"), resourcePathIds.SelectMany(byteArr => byteArr).ToArray());
            }
        }
    }

    class PerMoveResourceInfoItem
    {
        public uint size { get; set; }

        public ulong rootNodeId { get; set; }

        public ulong resourcePathId { get; set; }

        public PerMoveResourceInfoItem(uint size, ulong rootNodeId, ulong resourcePathId)
        {
            this.size = size;
            this.rootNodeId = rootNodeId;
            this.resourcePathId = resourcePathId;
        }
    }
}
