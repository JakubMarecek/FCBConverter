using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
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

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [System.Runtime.InteropServices.In] ref uint pcFonts);

        private PrivateFontCollection fonts = new PrivateFontCollection();

        private void CallFCBConverter(string launchParams)
        {
            Process process = new Process();
            process.StartInfo.FileName = "FCBConverter.exe";
            process.StartInfo.Arguments = launchParams;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.Start();
            process.WaitForExit();
        }

        private FontFamily GetFont(byte[] fontData)
        {
            IntPtr fontPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(fontData.Length);
            System.Runtime.InteropServices.Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
            uint dummy = 0;
            fonts.AddMemoryFont(fontPtr, fontData.Length);
            AddFontMemResourceEx(fontPtr, (uint)fontData.Length, IntPtr.Zero, ref dummy);
            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(fontPtr);
            return fonts.Families[0];
        }

        public MainWindow()
        {
            InitializeComponent();

            Font FCZTitle = new Font(GetFont(Properties.Resources.FCZTitle), 70);
            Font DINNextW1G_Regular = new Font(GetFont(Properties.Resources.DINNextW1G_Regular), 12);
            Font TradeGothicLT_Bold = new Font(GetFont(Properties.Resources.TradeGothicLT_Bold), 12);

            label1.Font = FCZTitle;
            label2.Font = TradeGothicLT_Bold;
            label3.Font = TradeGothicLT_Bold;
            label4.Font = TradeGothicLT_Bold;
            label5.Font = TradeGothicLT_Bold;
            label6.Font = TradeGothicLT_Bold;
            label8.Font = TradeGothicLT_Bold;
            label9.Font = TradeGothicLT_Bold;
            label11.Font = TradeGothicLT_Bold;
            label12.Font = TradeGothicLT_Bold;
            label13.Font = TradeGothicLT_Bold;
            label14.Font = TradeGothicLT_Bold;
            label15.Font = TradeGothicLT_Bold;
            label17.Font = TradeGothicLT_Bold;
            label18.Font = TradeGothicLT_Bold;
            label21.Font = TradeGothicLT_Bold;
            tabControl1.Font = DINNextW1G_Regular;
            button1.Font = TradeGothicLT_Bold;
            button2.Font = TradeGothicLT_Bold;
            button5.Font = TradeGothicLT_Bold;
            button6.Font = TradeGothicLT_Bold;
            button9.Font = TradeGothicLT_Bold;
            button10.Font = TradeGothicLT_Bold;
            button11.Font = TradeGothicLT_Bold;
            button12.Font = TradeGothicLT_Bold;
            button13.Font = TradeGothicLT_Bold;
            textBox1.Font = TradeGothicLT_Bold;
            textBox2.Font = TradeGothicLT_Bold;
            textBox3.Font = TradeGothicLT_Bold;
            textBox4.Font = TradeGothicLT_Bold;
            textBox5.Font = TradeGothicLT_Bold;
            textBox6.Font = TradeGothicLT_Bold;
            textBox7.Font = TradeGothicLT_Bold;
            textBox8.Font = TradeGothicLT_Bold;
            radioButton1.Font = TradeGothicLT_Bold;
            radioButton2.Font = TradeGothicLT_Bold;
            radioButton3.Font = TradeGothicLT_Bold;
            radioButton4.Font = TradeGothicLT_Bold;
            radioButton5.Font = TradeGothicLT_Bold;
            checkBox1.Font = TradeGothicLT_Bold;
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
            string subf = "";

            if (checkBox1.Checked)
                subf = "-subfolders";

            CallFCBConverter(textBox5.Text + " " + textBox8.Text + " " + subf);
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
