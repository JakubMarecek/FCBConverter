using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace FCBConverterGUI
{
    public partial class MainWindow : Form
    {
        const string files =
            "Far Cry Binary file|*.fcb|" +
            "Database file|*.ndb|" +
            "Dependency loader file|*_depload.dat|" +
            "Sound info file|soundinfo.bin|" +
            "Animation markup file|*.markup.bin|" +
            "Far Cry 5 / ND / 6 Strings file|oasisstrings.oasis.bin|" +
            "Far Cry 3 / 4 Strings file|oasisstrings_compressed.bin|" +
            "Lua file (adds LUAC header)|*.lua|" +
            "Material file|*.material.bin|" +
            "Texture file|*.xbt|" +
            "Terrain texture file|*.xbts|" +
            "Animation move file|*.move.bin|" +
            "Combined Move File|CombinedMoveFile.bin|" +
            "Sequence file|*.cseq|" +
            "Flash UI file|*.feu|" +
            "Bundle file|*.bdl|" +
            "Binary WolfSkin file|*.bwsk|" +
            "Wwise SoundBank file|*.bnk|" +
            "Wwise Encoded Media|*.wem|" +
            "File allocation table (DAT header file)|*.fat|" +
            "Converted files|*.converted.xml";

        int sourceXbgSkel = 0;
        Dictionary<int, int> sourceXbgSkelMats;
        Dictionary<int, Dictionary<int, int>> sourceXbgSkelVerts;
        Dictionary<int, Dictionary<int, int>> sourceXbgSkelFaces;
        Dictionary<int, Dictionary<int, Dictionary<int, HideFacesStruct>>> sourceXbgSkelHideFacesFP;
        string[] sourceXbgMatNames;
        string[] sourceXbgMatPaths;
        int hideFacesSelSkel = 0;
        int hideFacesSelMat = 0;

        private void SetItemChecked(string item)
        {
            int index = GetItemIndex(item);

            if (index < 0) return;

            checkedListBox1.SetItemChecked(index, true);
        }

        private int GetItemIndex(string item)
        {
            int index = 0;

            foreach (object o in checkedListBox1.Items)
            {
                if (item == o.ToString())
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        struct HideFacesStruct
        {
            public ulong id;
            public ushort start;
            public ushort count;
        }

        private int CallFCBConverter(string launchParams)
        {
            Process process = new Process();
            process.StartInfo.FileName = "FCBConverter.exe";
            process.StartInfo.Arguments = launchParams + " -keep";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.Start();
            process.WaitForExit();
            return process.ExitCode;
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            radioButton8.Checked = true;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "FAT files|*.fat",
                FileName = textBox1.Text
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog.FileName;
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
            {
                SelectedPath = textBox2.Text,
                Description = "Selected folder should be empty."
            };
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CallFCBConverter("\"" + textBox1.Text + "\" \"" + textBox2.Text + "\"");
        }

        private void button11_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
            {
                SelectedPath = textBox4.Text,
                Description = ""
            };
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBox4.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string fatVer = "";

            if (radioButton3.Checked) fatVer = "-v5";
            if (radioButton4.Checked) fatVer = "-v9";
            if (radioButton12.Checked) fatVer = "-v11";
            if (radioButton5.Checked) fatVer = "";

            string compress = "-disablecompress";

            if (radioButton2.Checked) compress = "-enablecompress";

            string excludeCompress = "-excludeFilesFromCompress=" + textBox7.Text;

            string outFile = textBox4.Text + ".fat";

            CallFCBConverter("\"" + textBox4.Text + "\" \"" + outFile + "\" " + fatVer + " " + compress + " " + excludeCompress);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
            {
                SelectedPath = textBox5.Text,
                Description = "All files in selected folder with given mask will be converted."
            };
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBox5.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string subf = "";

            if (checkBox1.Checked)
                subf = "-subfolders ";

            if (checkBox2.Checked)
                subf = "-fc2";

            CallFCBConverter("\"" + textBox5.Text + "\" \"" + textBox8.Text + "\" " + subf);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = files,
                FileName = textBox6.Text
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox6.Text = openFileDialog.FileName;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string addParam = "";

            if (checkBox3.Checked)
                addParam = "-fc2";

            CallFCBConverter("\"" + textBox6.Text + "\" " + addParam);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Far Cry 5 / New Dawn Mesh File|*.xbg",
                FileName = textBox10.Text
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox10.Text = "";
                textBox10.Text = openFileDialog.FileName;
            }
        }

        private void LoadDataFromXBG(string file)
        {
            try
            {
                listView1.Items.Clear();
                foreach (int i in checkedListBox1.CheckedIndices)
                {
                    checkedListBox1.SetItemCheckState(i, CheckState.Unchecked);
                }

                Process process = new Process();
                process.StartInfo.FileName = "FCBConverter.exe";
                process.StartInfo.Arguments = "-xbgData \"" + file + "\"";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();

                    if (line != null && line.StartsWith("data"))
                    {
                        string data = line.Replace("data", "");

                        string[] dataParse = data.Split('|');
                        foreach (string dP in dataParse)
                        {
                            string[] vals = dP.Split(';');
                            string[] valsData = vals[1].Split(',');

                            if (vals[0] == "MESHPARTHIDE")
                            {
                                foreach (string valsV in valsData)
                                {
                                    if (valsV != "" && valsV != "0")
                                        SetItemChecked(GetMeshParts()[valsV]);
                                }
                            }

                            /*if (vals[0] == "FACEHIDEFP")
                            {
                                foreach (string valsV in valsData)
                                {
                                    if (valsV != "" && valsV != "0")
                                    {
                                        string[] partsV = valsV.Split('*');

                                        ListViewItem listViewItem = new ListViewItem();
                                        listViewItem.Text = GetMeshParts()[partsV[0]];
                                        listViewItem.SubItems.Add(partsV[1]);
                                        listViewItem.SubItems.Add(partsV[2]);
                                        listView1.Items.Add(listViewItem);
                                    }
                                }
                            }*/

                            if (vals[0] == "SKELFACES")
                            {
                                sourceXbgSkelFaces = new();

                                for (int i = 0; i < valsData.Length; i++)
                                {
                                    string[] matChilds = valsData[i].Split('+');
                                    Dictionary<int, int> matsArr = new();

                                    for (int j = 0; j < matChilds.Length; j++)
                                    {
                                        matsArr.Add(j, int.Parse(matChilds[j]));
                                    }

                                    sourceXbgSkelFaces.Add(i, matsArr);
                                }
                            }

                            if (vals[0] == "SKELVERTS")
                            {
                                sourceXbgSkelVerts = new();

                                for (int i = 0; i < valsData.Length; i++)
                                {
                                    string[] matChilds = valsData[i].Split('+');
                                    Dictionary<int, int> matsArr = new();

                                    for (int j = 0; j < matChilds.Length; j++)
                                    {
                                        matsArr.Add(j, int.Parse(matChilds[j]));
                                    }

                                    sourceXbgSkelVerts.Add(i, matsArr);
                                }
                            }

                            if (vals[0] == "SKELMATS")
                            {
                                sourceXbgSkelMats = new();

                                for (int i = 0; i < valsData.Length; i++)
                                {
                                    sourceXbgSkelMats.Add(i, int.Parse(valsData[i]));
                                }
                            }

                            if (vals[0] == "SKELHIDEFACESFP")
                            {
                                sourceXbgSkelHideFacesFP = new();

                                for (int i = 0; i < valsData.Length; i++)
                                {
                                    string[] matChilds = valsData[i].Split('+');
                                    Dictionary<int, Dictionary<int, HideFacesStruct>> matsArr = new();

                                    for (int j = 0; j < matChilds.Length; j++)
                                    {
                                        Dictionary<int, HideFacesStruct> hfSA = new();

                                        if (matChilds[j] != "" && matChilds[j] != "0")
                                        {
                                            string[] matParts = matChilds[j].Split('-');

                                            for (int k = 0; k < matParts.Length; k++)
                                            {
                                                string[] partData = matParts[k].Split('*');

                                                hfSA.Add(k, new()
                                                {
                                                    id = ulong.Parse(partData[0]),
                                                    start = ushort.Parse(partData[1]),
                                                    count = ushort.Parse(partData[2])
                                                });
                                            }
                                        }

                                        matsArr.Add(j, hfSA);
                                    }

                                    sourceXbgSkelHideFacesFP.Add(i, matsArr);
                                }
                            }

                            if (vals[0] == "SKEL")
                            {
                                sourceXbgSkel = int.Parse(vals[1]);
                            }

                            if (vals[0] == "MATS")
                            {
                                List<string> matNames = new List<string>();
                                List<string> matPaths = new List<string>();
                                foreach (string valsV in valsData)
                                {
                                    string[] partsV = valsV.Split('*');
                                    matNames.Add(partsV[0]);
                                    matPaths.Add(partsV[1]);
                                }
                                sourceXbgMatNames = matNames.ToArray();
                                sourceXbgMatPaths = matPaths.ToArray();
                            }
                        }
                    }
                }

                process.WaitForExit();

                button17.Enabled = true;
                button18.Enabled = true;
                button19.Enabled = true;

                numericUpDown1.Value = 0;
                numericUpDown1.Maximum = sourceXbgSkel - 1;
                numericUpDown2.Value = 0;
                numericUpDown2.Maximum = sourceXbgSkelMats[(int)numericUpDown1.Value] - 1;

                hideFacesSelSkel = 0;
                hideFacesSelMat = 0;

                NumericsChangeSetData(0, 0);
            }
            catch (Exception)
            {
                MessageBox.Show("Error occured during opening XBG. Did you select right game?", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.MeshParts = GetMeshParts();
            if (form2.ShowDialog() == DialogResult.OK)
            {
                ListViewItem listViewItem = new ListViewItem();
                listViewItem.Text = form2.SelectedMeshName;
                listViewItem.SubItems.Add(form2.FaceStartIndex.ToString());
                listViewItem.SubItems.Add(form2.FaceCount.ToString());
                listView1.Items.Add(listViewItem);

                EditData();
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem eachItem in listView1.SelectedItems)
            {
                listView1.Items.Remove(eachItem);

                EditData();
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            if (!File.Exists(textBox10.Text))
                return;

            string outParamVal = "";

            outParamVal += "SKELHIDEFACESFP;";
            for (int i = 0; i < sourceXbgSkelHideFacesFP.Count; i++)
            {
                outParamVal += (i > 0 ? "," : "");
                for (int j = 0; j < sourceXbgSkelHideFacesFP[i].Count; j++)
                {
                    outParamVal += (j > 0 ? "+" : "");
                    for (int k = 0; k < sourceXbgSkelHideFacesFP[i][j].Count; k++)
                    {
                        HideFacesStruct hideFacesStruct = sourceXbgSkelHideFacesFP[i][j][k];
                        outParamVal += (k > 0 ? "-" : "") + hideFacesStruct.id + "*" + hideFacesStruct.start + "*" + hideFacesStruct.count;
                    }
                }
            }

            outParamVal += "|MESHPARTHIDE;";
            for (int i = 0; i < checkedListBox1.CheckedItems.Count; i++)
            {
                string meshID = GetMeshParts().FirstOrDefault(x => x.Value == checkedListBox1.CheckedItems[i].ToString()).Key;
                outParamVal += (i > 0 ? "," : "") + meshID;
            }

            CallFCBConverter("-xbgFP \"" + textBox10.Text + "\" \"" + outParamVal + "\"");

            MessageBox.Show("XBG successfully edited.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var senderList = (ListView)sender;
            var clickedItem = senderList.HitTest(e.Location).Item;
            if (clickedItem != null)
            {
                Form2 form2 = new Form2();
                form2.MeshParts = GetMeshParts();
                form2.SelectedMeshName = listView1.Items[clickedItem.Index].SubItems[0].Text;
                form2.FaceStartIndex = int.Parse(listView1.Items[clickedItem.Index].SubItems[1].Text);
                form2.FaceCount = int.Parse(listView1.Items[clickedItem.Index].SubItems[2].Text);

                if (form2.ShowDialog() == DialogResult.OK)
                {
                    clickedItem.SubItems[0].Text = form2.SelectedMeshName;
                    clickedItem.SubItems[1].Text = form2.FaceStartIndex.ToString();
                    clickedItem.SubItems[2].Text = form2.FaceCount.ToString();

                    EditData();
                }
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Autocalculate will add all faces from selected material and submesh. This is used for clothes of type feet, bottom and handwear. Top parts can produce bugs like missing body when you look down.", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int clc = (int)Math.Floor((double)sourceXbgSkelFaces[(int)numericUpDown1.Value][(int)numericUpDown2.Value] / listView1.Items.Count);

                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    listView1.Items[i].SubItems[1].Text = (clc * i * 3).ToString();
                    listView1.Items[i].SubItems[2].Text = (clc * 3).ToString();

                    EditData();
                }
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            string info = "";

            info += "Source XBG contains" + Environment.NewLine + Environment.NewLine;
            info += "Materials:" + Environment.NewLine;

            for (int i = 0; i < sourceXbgMatNames.Length; i++)
            {
                info += "  Material ID " + i.ToString() + Environment.NewLine;
                info += "    -Name: " + sourceXbgMatNames[i] + Environment.NewLine;
                info += "    -Path: " + sourceXbgMatPaths[i] + Environment.NewLine;
            }

            info += Environment.NewLine;

            info += "Number of skeleton IDs: " + sourceXbgSkel + Environment.NewLine + Environment.NewLine;

            info += "Count of materials in each skeleton ID:" + Environment.NewLine;

            for (int i = 0; i < sourceXbgSkel; i++)
            {
                info += "  -Skeleton " + i.ToString() + " contains " + sourceXbgSkelMats[i] + " materials" + Environment.NewLine;
                for (int j = 0; j < sourceXbgSkelMats[i]; j++)
                {
                    int faces = sourceXbgSkelFaces[i][j];
                    int verts = sourceXbgSkelVerts[i][j];
                    info += "    -material ID " + j.ToString() + " contains " + faces + " (" + (faces * 3) + ")" + " faces, " + verts + " verts" + Environment.NewLine;
                }
            }

            Form4 form4 = new Form4();
            form4.Info = info;
            form4.ShowDialog();
        }

        private void button22_Click(object sender, EventArgs e)
        {
            string type = "";

            if (radioButton7.Checked) type = "0";
            if (radioButton6.Checked) type = "1";
            if (radioButton10.Checked) type = "2";
            if (radioButton11.Checked) type = "3";

            CallFCBConverter("-ue=" + type + " \"" + textBox11.Text + "\" \"" + textBox12.Text + "\"");
        }

        private void button20_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Unreal Engine UAsset file|*.uasset",
                FileName = textBox11.Text
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox11.Text = openFileDialog.FileName;
            }
        }

        private void button21_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Far Cry 5 / New Dawn Mesh File|*.xbg",
                FileName = textBox12.Text
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox12.Text = openFileDialog.FileName;
            }
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            if (File.Exists(textBox10.Text))
                LoadDataFromXBG(textBox10.Text);
        }

        private Dictionary<string, string> GetMeshParts()
        {
            if (radioButton8.Checked)
                return MeshParts.meshPartsFC5;

            if (radioButton9.Checked)
                return MeshParts.meshPartsNewDawn;

            return null;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            NumericsChangeSetData((int)numericUpDown1.Value, 0);

            numericUpDown2.Value = 0;
            numericUpDown2.Maximum = sourceXbgSkelMats[(int)numericUpDown1.Value] - 1;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            NumericsChangeSetData((int)numericUpDown1.Value, (int)numericUpDown2.Value);
        }

        private void NumericsChangeSetData(int skel, int material)
        {
            Dictionary<int, HideFacesStruct> hideFacesStructs = sourceXbgSkelHideFacesFP[skel][material];

            listView1.Items.Clear();

            foreach (HideFacesStruct hideFacesStruct in hideFacesStructs.Values)
            {
                ListViewItem listViewItem = new ListViewItem();
                listViewItem.Text = GetMeshParts()[hideFacesStruct.id.ToString()];
                listViewItem.SubItems.Add(hideFacesStruct.start.ToString());
                listViewItem.SubItems.Add(hideFacesStruct.count.ToString());
                listView1.Items.Add(listViewItem);
            }

            hideFacesSelSkel = skel;
            hideFacesSelMat = material;
        }

        private void EditData()
        {
            Dictionary<int, HideFacesStruct> hideFacesStructsNew = new();

            for (int i = 0; i < listView1.Items.Count; i++)
            {
                string meshID = GetMeshParts().FirstOrDefault(x => x.Value == listView1.Items[i].SubItems[0].Text).Key;

                hideFacesStructsNew.Add(i, new()
                {
                    id = ulong.Parse(meshID),
                    start = ushort.Parse(listView1.Items[i].SubItems[1].Text),
                    count = ushort.Parse(listView1.Items[i].SubItems[2].Text)
                });
            }

            sourceXbgSkelHideFacesFP[hideFacesSelSkel][hideFacesSelMat] = hideFacesStructsNew;
        }

        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            checkedListBox1.Items.Clear();
            foreach (KeyValuePair<string, string> pair in MeshParts.meshPartsNewDawn)
                checkedListBox1.Items.Add(pair.Value);
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            checkedListBox1.Items.Clear();
            foreach (KeyValuePair<string, string> pair in MeshParts.meshPartsFC5)
                checkedListBox1.Items.Add(pair.Value);
        }
    }
}
