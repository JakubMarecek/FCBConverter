using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
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
            "Far Cry 5 Strings file|oasisstrings.oasis.bin|" +
            "Far Cry New Dawn Strings file|oasisstrings_nd.oasis.bin|" +
            "Far Cry 4 Strings file|oasisstrings_compressed.bin|" +
            "Lua file (adds LUAC header)|*.lua|" +
            "Material file|*.material.bin|" +
            "Texture file|*.xbt|" +
            "Terrain texture file|*.xbts|" +
            "Animation move file|*.move.bin|" +
            "Combined Move File|CombinedMoveFile.bin|" +
            "Sequence file|*.cseq|" +
            "Flash UI file|*.feu|" +
            "Bundle file|*.bdl|" +
            "Wwise SoundBank file|*.bnk|" +
            "Wwise Encoded Media|*.wem|" +
            "File allocation table (DAT header file)|*.fat|" +
            "Converted files|*.converted.xml";

        int sourceXbgSkel = 0;
        string[] sourceXbgSkelMats;
        string[] sourceXbgSkelVerts;
        string[] sourceXbgSkelFaces;
        string[] sourceXbgMatNames;
        string[] sourceXbgMatPaths;

        private int CallFCBConverter(string launchParams)
        {
            Process process = new Process();
            process.StartInfo.FileName = "FCBConverter.exe";
            process.StartInfo.Arguments = launchParams;
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
                subf = "-subfolders";

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
            CallFCBConverter("\"" + textBox6.Text + "\"");
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

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Far Cry 5 / New Dawn Mesh File|*.xbg",
                FileName = textBox9.Text
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox9.Text = openFileDialog.FileName;
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            if (!File.Exists(textBox9.Text))
                return;

            LoadDataFromXBG(textBox9.Text, false);
        }

        private void LoadDataFromXBG(string file, bool selLodSubMesh)
        {
            try
            {
                Form3 form3 = new Form3();
                if (form3.ShowDialog() == DialogResult.OK)
                {
                    listView1.Items.Clear();
                    listView2.Items.Clear();

                    Process process = new Process();
                    process.StartInfo.FileName = "FCBConverter.exe";
                    process.StartInfo.Arguments = "-xbgData \"" + file + "\" -skid=" + form3.SelectedSkID.ToString() + " -matid=" + form3.SelectedMatID.ToString();
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
                                        if (valsV != "")
                                            listView2.Items.Add(GetMeshParts()[valsV]);
                                    }
                                }

                                if (vals[0] == "FACEHIDEFP")
                                {
                                    foreach (string valsV in valsData)
                                    {
                                        if (valsV != "")
                                        {
                                            string[] partsV = valsV.Split('*');

                                            ListViewItem listViewItem = new ListViewItem();
                                            listViewItem.Text = GetMeshParts()[partsV[0]];
                                            listViewItem.SubItems.Add(partsV[1]);
                                            listViewItem.SubItems.Add(partsV[2]);
                                            listView1.Items.Add(listViewItem);
                                        }
                                    }
                                }

                                if (selLodSubMesh)
                                {
                                    if (vals[0] == "SKELFACES")
                                    {
                                        sourceXbgSkelFaces = valsData;
                                    }

                                    if (vals[0] == "SKELVERTS")
                                    {
                                        sourceXbgSkelVerts = valsData;
                                    }

                                    if (vals[0] == "SKELMATS")
                                    {
                                        sourceXbgSkelMats = valsData;
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
                    }

                    process.WaitForExit();

                    button17.Enabled = true;
                    button18.Enabled = true;
                    button19.Enabled = true;

                    if (selLodSubMesh)
                    {
                        numericUpDown1.Value = form3.SelectedSkID;
                        numericUpDown2.Value = form3.SelectedMatID;
                    }
                }
            }
            catch(Exception)
            {
                MessageBox.Show("Error occured during opening XBG. Did you select right game?", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.MeshParts = GetMeshParts();
            if (form1.ShowDialog() == DialogResult.OK)
            {
                listView2.Items.Add(form1.SelectedValue);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem eachItem in listView2.SelectedItems)
            {
                listView2.Items.Remove(eachItem);
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
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem eachItem in listView1.SelectedItems)
            {
                listView1.Items.Remove(eachItem);
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            if (!File.Exists(textBox10.Text))
                return;

            string outParamVal = "";

            outParamVal += "FACEHIDEFP;";
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                string meshID = GetMeshParts().FirstOrDefault(x => x.Value == listView1.Items[i].SubItems[0].Text).Key;
                outParamVal += (i > 0 ? "," : "") + meshID + "-" + listView1.Items[i].SubItems[1].Text + "-" + listView1.Items[i].SubItems[2].Text;
            }

            outParamVal += "|MESHPARTHIDE;";
            for (int i = 0; i < listView2.Items.Count; i++)
            {
                string meshID = GetMeshParts().FirstOrDefault(x => x.Value == listView2.Items[i].Text).Key;
                outParamVal += (i > 0 ? "," : "") + meshID;
            }

            CallFCBConverter("-xbgFP \"" + textBox10.Text + "\" -skid=" + numericUpDown1.Value.ToString() + " -matid=" + numericUpDown2.Value.ToString() + " \"" + outParamVal + "\"");

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
                }
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Autocalculate will add all faces from selected material and submesh. This is used for clothes of type feet, bottom and handwear. Top parts can produce bugs like missing body when you look down.", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int clc = (int)Math.Floor((double)GetMatDataCount(sourceXbgSkelFaces, (int)numericUpDown1.Value, (int)numericUpDown2.Value) / listView1.Items.Count);

                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    listView1.Items[i].SubItems[1].Text = (clc * i * 3).ToString();
                    listView1.Items[i].SubItems[2].Text = (clc * 3).ToString();
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
                for (int j = 0; j < int.Parse(sourceXbgSkelMats[i]); j++)
                {
                    int faces = GetMatDataCount(sourceXbgSkelFaces, i, j);
                    int verts = GetMatDataCount(sourceXbgSkelVerts, i, j);
                    info += "    -material ID " + j.ToString() + " contains " + faces + " (" + (faces * 3) + ")" + " faces, " + verts + " verts" + Environment.NewLine;
                }
            }

            Form4 form4 = new Form4();
            form4.Info = info;
            form4.ShowDialog();
        }

        private int GetMatDataCount(string[] data, int material, int submesh)
        {
            int fcAr = 0;

            for (int i = 0; i <= material; i++)
            {
                if (i == material)
                {
                    return int.Parse(data[fcAr + submesh]);
                }
                fcAr += int.Parse(sourceXbgSkelMats[i]);
            }

            return -1;
        }

        private void button22_Click(object sender, EventArgs e)
        {
            CallFCBConverter("-ue \"" + textBox11.Text + "\" \"" + textBox12.Text + "\"");
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
                LoadDataFromXBG(textBox10.Text, true);
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            if (File.Exists(textBox9.Text))
                button16.Enabled = true;
            else
                button16.Enabled = false;
        }

        private Dictionary<string, string> GetMeshParts()
        {
            if (radioButton8.Checked)
                return MeshParts.meshPartsFC5;

            if (radioButton9.Checked)
                return MeshParts.meshPartsNewDawn;

            return null;
        }
    }
}
