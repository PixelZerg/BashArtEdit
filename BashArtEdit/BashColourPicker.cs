using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BashArtEdit
{
    public partial class BashColourPicker : UserControl
    {
        public BashColourPicker()
        {
            InitializeComponent();
            numericUpDown1.Maximum = BashColour.list.Count;
            for (int no = 0; no != BashColour.list.Count; no++)
            {
                Label l = new Label();
                l.Text = no.ToString();
                Panel p = new Panel();
                p.Height = 20;
                p.BackColor = BashColour.list[no];
                p.Controls.Add(l);
                flowLayoutPanel1.Controls.Add(p);
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            panel1.BackColor = BashColour.list[(int)(numericUpDown1.Value)];
        }
    }
}
