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
    public partial class ExportDialog : Form
    {
        public ExportDialog(string txt)
        {
            InitializeComponent();
            textBox1.Text = txt;
        }
    }
}
