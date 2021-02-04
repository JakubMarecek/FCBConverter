using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace FCBConverterGUI
{
    public partial class MainWindow : Form
    {
        const string files =
            "Far Cry Binary|*.fcb|" +
            "Database file|*.ndb|" +
            "Dependency loader|*_depload.dat|" +
            "Sound info|soundinfo.bin|" +
            "Animation markup file|*.markup.bin|" +
            "Texture|*.xbt|" +
            "Terrain texture|*.xbts|" +
            "Animation move file|*.move.bin|" +
            "Combined Move File|CombinedMoveFile.bin|" +
            "Sequence file|*.cseq|" +
            "Flash UI file|*.feu|" +
            "Bundle file|*.bdl|" +
            "File allocation table (DAT header file)|*.fat|" +
            "Converted files|*.converted.xml";

        private void CallFCBConverter(string launchParams)
        {
            Process process = new Process();
            process.StartInfo.FileName = "FCBConverter.exe";
            process.StartInfo.Arguments = launchParams;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.Start();
            process.WaitForExit();
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void tabPage2_Click_1(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
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
            CallFCBConverter(textBox1.Text + " " + textBox2.Text);
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

            CallFCBConverter(textBox4.Text + " " + outFile + " " + fatVer + " " + compress + " " + excludeCompress);
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
            CallFCBConverter(textBox5.Text + " " + textBox8.Text);
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
            CallFCBConverter(textBox6.Text);
        }
    }
}
