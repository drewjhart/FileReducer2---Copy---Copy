using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace FileReducer2
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            long compressionLevel = Convert.ToInt64(15);
            var PBImage = Image.FromStream(this.imageFile);
            pictureBox1.Image = PBImage;
            pictureBox1.Refresh();
               
        }
        public MemoryStream imageFile { get; set; }
       
        
        private void button1_Click(object sender, EventArgs e)
        {
            this.imageFile.Close();
            this.DialogResult = DialogResult.OK;           
        }

        
        
        
    }
}
