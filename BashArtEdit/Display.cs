using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Xml;

namespace BashArtEdit
{
    public partial class Display : Form
    {
        public string prevtxt = "";
        public bool control = false;
        public bool allowupt = true;
        public Regex rgx =new Regex(@"\\e\[\d+;\d+;\d+m");
        public List<BashColour> cols = new List<BashColour>();
        public bool rshiftd = false;
        public Display()
        {
            InitializeComponent();
            cols.Add(new BashColour(0, 0, true));
            cols.Add(new BashColour(7, 0, false));
            BashColour.serialize();
            input.BackColor = BashColour.list[0];
            input.ForeColor = BashColour.list[7];
            BashColour.serialize();
            for (int no = 0; no != BashColour.list.Count; no++)
            {
                ListViewItem i = new ListViewItem();
                i.BackColor = BashColour.list[no];
                i.Text = no.ToString();
                listView1.Items.Add(i);
            }
        }

        private void input_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            foreach(var col in cols){
            e.ChangedRange.SetStyle(new FastColoredTextBoxNS.TextStyle(new SolidBrush(col.ToColor()),Brushes.White, FontStyle.Regular),"\\w+");
            }
        }

        private void input_KeyDown(object sender, KeyEventArgs e)
        {
            bool dothis = false;
            switch (e.KeyCode)
            {
                case Keys.Up:
                    dothis = false;
                    break;
                case Keys.Down:
                    dothis = false;
                    break;
                case Keys.Left:
                    dothis = false;
                    break;
                case Keys.Right:
                    dothis = false;
                    break;
                case Keys.Back:
                    dothis = false;
                    input.Text.Insert(input.SelectionStart, " ");
                    break;
            }
            if (dothis)
            {
                //input.SelectionStyle.BackgroundBrush = Brushes.AliceBlue;
                int i = input.SelectionStart;
                try
                {
                    input.Text = input.Text.Remove(input.SelectionStart, 1);
                    input.SelectionStart = i;
                }
                catch { }
            }
        }

        private void input_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.RShiftKey)
            {
                rshiftd = false;
                if(Program.debug)
                    if (Program.debug)
                Console.WriteLine(rshiftd);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdCur();
            int ba = Int32.Parse(curcolbdisp.Text);
            int fr = Int32.Parse(curcolfdisp.Text);
            try
            {
                for (int i = 0; i != cols.Count; i++)
                {
                    if (cols[i].charno == input.SelectionStart && cols[i].background == checkBox1.Checked) cols.RemoveAt(i);
                }
            }
            catch { }
            foreach (ListViewItem sel in listView1.SelectedItems)
            {
                {   BashColour b = new BashColour(sel.Index, input.SelectionStart, checkBox1.Checked);
                    cols.Add(b);
                }
            }
            if (input.SelectionLength > 0)
            {
                cols.Add(new BashColour(ba, input.SelectionLength + input.SelectionStart, true));
                cols.Add(new BashColour(fr, input.SelectionLength + input.SelectionStart, false));
            }
            Console.Clear();
            foreach (var item in cols)
            {
                if (Program.debug)
                Console.Write(item.code + " ");
                if (item.background)
                {
                    if (Program.debug)
                    Console.Write("background");
                }
                if (Program.debug)
                Console.Write(" col ");
                if (Program.debug)
                Console.WriteLine("at " +item.charno);
            }
            SetCols();
        }

        private void input_TextChanged(object sender, EventArgs e)
        {
            if (allowupt)
            {
                SetCols();
                allowupt = false;
                timer1.Start();
            }
            prevtxt = input.Text;
        }
        public void ParseInput()
        {
            //Console.Clear();
            int sels = input.SelectionStart;
            input.Text = input.Text.Replace("\\x1b", "\\e");
            input.Text = input.Text.Replace("\\e[0m", "\\e[48;5;0m\\e[38;5;7m");
            input.Text = input.Text.Replace(@"\\", @"\");
            input.Text = input.Text.Replace(@"\`", @"`");
            input.Text = input.Text.Replace("\\\"", "\"");
            input.Text = input.Text.Replace("\\'", "\'");
            string txt = input.Text;//.Remove(input.Text.LastIndexOf(prevtxt),prevtxt.Length);
            if (Program.debug)
            Console.WriteLine("Parsing Input!");
            int prevlengths = 0;
            MatchCollection m = rgx.Matches(txt);
            progressBar1.Maximum = m.Count;
            foreach (Match item in m)
            {
                    bool back = item.ToString().StartsWith("\\e[48");
                    if (Program.debug)
                        Console.WriteLine("back = " + back);
                    int code = System.Int32.Parse((item.ToString().Substring(item.ToString().IndexOf(";") + 1,
                        +item.ToString().IndexOf('m') - item.ToString().IndexOf(";") - 1))
                        .Substring((item.ToString().Substring(item.ToString().IndexOf(";") + 1,
                        +item.ToString().IndexOf('m') - item.ToString().IndexOf(";") - 1)).IndexOf(";") + 1));
                    if (Program.debug)
                        Console.WriteLine("code = " + code);
                    if (Program.debug)
                        Console.WriteLine(code + " detected at " + (item.Index - prevlengths) + " : background = " + back);
                    cols.Add(new BashColour(code, item.Index - prevlengths, back));
                input.Text = input.Text.Remove(item.Index - prevlengths, item.Length);
                prevlengths += item.Length;
                progressBar1.Increment(1);
            }
            prevlengths = 0;
            //SetCols();
            SetCols();
            progressBar1.Value = 0;
            input.SelectionStart = sels+(input.Text.Length-prevtxt.Length);
        }
        public void SortCols()
        {
                System.Collections.Generic.List<BashColour> ret = cols;
                while (!isSorted(ret))
                {
                    int last = 0;
                    for (int no = 0; no < ret.Count; no++)
                    {
                        try
                        {
                            if (ret[no].charno - last < 0 || (ret[no].charno - last == 0 && ret[no].code < ret[no - 1].code))
                            {
                                //move this back (swap)
                                var temp = ret[no - 1];
                                ret[no - 1] = ret[no];
                                ret[no] = temp;
                            }
                        }
                        catch { }
                        last = ret[no].charno;
                    }
                    //foreach (var item in ret)
                    //{
                    //    Console.Write(item.charno + ": ");
                    //    if (item.background)
                    //    {
                    //        Console.Write("background");
                    //    }
                    //    Console.Write(" col ");
                    //    Console.WriteLine("w/ " + item.code);
                    //}
                    //System.Threading.Thread.Sleep(500);
                    //Console.Clear();
                }
                cols = ret;
        }
        public bool isSorted(List<BashColour> l)
        {
            int no = 0;
            foreach (var item in l)
            {
                int dif = item.charno - no;
                if (dif < 0)
                {
                    return false;
                }
                no = item.charno;
            }
            return true;
        }
        public void SetCols()
        {
            if (checkBox2.Checked)
            {
                progressBar1.Value = 0;
                progressBar1.Maximum = cols.Count;
                SortCols();
                int sels = input.SelectionStart;
                int sell = input.SelectionLength;
                for (int i = 0; i != cols.Count; i++)
                {
                    try
                    {
                        input.SelectionStart = cols[i].charno;
                        input.SelectionLength = input.Text.Length - cols[i].charno;
                            if (!cols[i].background)
                            {
                                input.SelectionColor = cols[i].ToColor();
                            }
                            else
                            {
                                input.SelectionBackColor = cols[i].ToColor();
                            }
                    }
                    catch { }
                    progressBar1.Increment(1);
                }
                input.SelectionStart = sels;
                input.SelectionLength = sell;
                progressBar1.Value = 0;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Collections.Generic.List<string> l = new List<string>();
            //foreach (string str in input.Lines)
            //{
            //    l.Add(str.Replace("\\", "\\\\"));
            //}
            //string a = string.Join("\n", l);
            //a = a.Replace("`", @"\`");
            //a = a.Replace("\"", "\\\"");
            string b = "";
            SortCols();
            for (int i = 0; i < input.Text.Length;i++)
            {
                foreach (var col in cols)
                {
                    if (col.charno == i)
                    {
                        b += col.ToString();
                    }
                }
                b += input.Text[i];
            }
            string c = "";
            for (int i = 0; i!=b.Length;i++)
            {
                if (b[i] == '\\' && b.Substring(i, b.Length - i).StartsWith("e["))
                {
                    c += @"\\";
                }
                else
                {
                    c += b[i];
                }
            }
            c = c.Replace("`", @"\`");
            c = c.Replace("\"", "\\\"");
            new ExportDialog(c).ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SortCols();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            cols.Clear();
            cols.Add(new BashColour(0, 0, true));
            cols.Add(new BashColour(7, 0, false));
            SetCols();
            input.Clear();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            input.Clear();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            cols.Clear();
            cols.Add(new BashColour(0, 0, true));
            cols.Add(new BashColour(7, 0, false));
            SetCols();
        }

        private void input_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                //Console.WriteLine("cntrl down!");
                control = true;
            }
            UpdCur();
        }
        public void UpdCur()
        {
            int chrat = input.SelectionStart;
            BashColour bf = null;
            BashColour bb = null;
            foreach (var col in cols)
            {
                if (col.background)
                {
                    try
                    {
                        if (col.charno >= bb.charno && col.charno <= chrat)
                        {
                            bb = col;
                        }
                    }
                    catch
                    {
                        bb = col;
                    }
                }
                else
                {
                    try
                    {
                        if (col.charno >= bf.charno && col.charno <= chrat)
                        {
                            bf = col;
                        }
                    }
                    catch
                    {
                        bf = col;
                    }
                }
            }
            try
            {
                curcolb.BackColor = bb.ToColor();
                curcolbdisp.Text = bb.code.ToString();
                curcolf.BackColor = bf.ToColor();
                curcolfdisp.Text = bf.code.ToString();
            }
            catch { }
        }
        private void input_KeyUp_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                //Console.WriteLine("cntrl up!");
                control = false;
            }
            if (e.KeyCode == Keys.V)
            {
                if (!(input.Text.EndsWith("</pre>") && input.Text.StartsWith("<pre")))
                {
                    ParseInput();
                }
                else
                {
                    ParseXML();
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            allowupt = true;
            timer1.Stop();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                SetCols();
            }
            numericUpDown1.Visible = checkBox2.Checked;
            label1.Visible = checkBox2.Checked;
        }

        private void input_Click(object sender, EventArgs e)
        {
            UpdCur();
        }
        public void ParseXML()
        {
            if (input.Text.StartsWith("<pre") && input.Text.EndsWith("</pre>"))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(input.Text);
                input.Clear();
                XmlNodeList spans = doc.GetElementsByTagName("span");
                progressBar1.Maximum = spans.Count;
                progressBar1.Value = 0;
                int no = 0;
                foreach (XmlNode span in spans)
                {
                    input.Text += span.InnerText;
                    foreach (XmlAttribute attr in span.Attributes)
                    {
                        Color c = System.Drawing.ColorTranslator.FromHtml(attr.Value.Substring(attr.Value.IndexOf("#"), 7));
                        BashColour b = new BashColour(BashColour.ClosestBash(c),no,true);
                        //Console.WriteLine(c.ToString()+" ==> "+b.ToColor());
                        cols.Add(b);
                    }
                    no+=span.InnerText.Length;
                    progressBar1.Increment(no);
                }
                SetCols();
            }
        }
    }
}
