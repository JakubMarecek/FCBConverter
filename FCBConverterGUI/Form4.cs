using System;
using System.Windows.Forms;

namespace FCBConverterGUI
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
        }

        private void Form4_Load(object sender, EventArgs e)
        {

        }

        public string Info
        {
            set
            {
                textBox1.Text = value;
            }
        }
    }
}
