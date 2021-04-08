using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FCBConverterGUI
{
    public partial class Form2 : Form
    {
        public Dictionary<string, string> MeshParts { set; get; }

        string selectVal = "";

        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            foreach (KeyValuePair<string, string> mesh in MeshParts)
            {
                comboBox1.Items.Add(mesh.Value);
            }
            if (selectVal == "")
                comboBox1.SelectedIndex = 0;
            else
            {
                comboBox1.SelectedIndex = comboBox1.FindStringExact(selectVal);
                Text = "Edit mesh part";
                button1.Text = "Edit";
            }
        }

        public int FaceStartIndex
        {
            get
            {
                return (int)numericUpDown1.Value;
            }
            set
            {
                numericUpDown1.Value = value;
            }
        }

        public int FaceCount
        {
            get
            {
                return (int)numericUpDown2.Value;
            }
            set
            {
                numericUpDown2.Value = value;
            }
        }

        public string SelectedMeshName
        {
            get
            {
                return comboBox1.SelectedItem.ToString();
            }
            set
            {
                selectVal = value;
            }
        }
    }
}
