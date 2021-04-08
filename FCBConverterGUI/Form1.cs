using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FCBConverterGUI
{
    public partial class Form1 : Form
    {
        public Dictionary<string, string> MeshParts { set; get; }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (KeyValuePair<string, string> mesh in MeshParts)
            {
                comboBox1.Items.Add(mesh.Value);
            }
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        public string SelectedValue
        {
            get
            {
                return comboBox1.SelectedItem.ToString();
            }
        }
    }
}
