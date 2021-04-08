using System;
using System.Windows.Forms;

namespace FCBConverterGUI
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        public int SelectedLOD
        {
            get
            {
                return (int)numericUpDown1.Value;
            }
        }

        public int SelectedSubMesh
        {
            get
            {
                return (int)numericUpDown2.Value;
            }
        }
    }
}
