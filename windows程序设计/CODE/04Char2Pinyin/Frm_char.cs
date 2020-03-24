using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.International.Converters.PinYinConverter;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using System.Collections.ObjectModel;
namespace Char2Pinyin
{
    public partial class Frm_char : Form
    {
        public Frm_char()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
             
            if (textBox2.Text.Trim().Length == 0)
            {
                return;
            }
            char one_char = textBox2.Text.Trim().ToCharArray()[0];
            int ch_int = (int)one_char;
            string str_char_int = string.Format("{0}", ch_int);
            if (ch_int > 127)
            {
                ChineseChar chineseChar = new ChineseChar(one_char);
                
                ReadOnlyCollection<string> pinyin = chineseChar.Pinyins;
                string pin_str = "";
                foreach (string pin in pinyin)
                {
                    pin_str += pin + "\r\n";
                }
                textBox1.Text = "";
                textBox1.Text = pin_str;
            }
            label1.Text = str_char_int;             
        }

        

        private void button2_Click(object sender, EventArgs e)
        {
            label2.Text=ChineseConverter.Convert("李赞", ChineseConversionDirection.SimplifiedToTraditional);
        }
    }
}
