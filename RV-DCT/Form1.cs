using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RV_DCT
{
    public partial class Form1 : Form
    {
        Bitmap img;
        public Form1()
        {
            InitializeComponent();
        }

        private void LoadBtn_Click(object sender, EventArgs e)
        {

            Bitmap img = null;
            int width=0, height=0;
            OpenFileDialog open = new OpenFileDialog();
            if(open.ShowDialog() == DialogResult.OK)
            {
                
                img = (Bitmap)Bitmap.FromFile(open.FileName);
                width = img.Width;
                height = img.Height;
            }
            if (img.Width % 8 != 0)
            {
                width = img.Width + (8 - (img.Width % 8));
            }
            if (img.Height % 8 != 0)
            {
                height = img.Height + (8 - (img.Height % 8));
            }
            img = new Bitmap(img, new Size(width, height));
            pictureBox1.Image = img;
            this.img = img;
        }

        private void CompressBtn_Click(object sender, EventArgs e)
        {
            List<Color[,]> eightByEightArray = new List<Color[,]>();
            
            int startI = 0;
            int startJ = 0;
            int tmpI = 0, tmpJ = 0;
            while (startJ < img.Height)
            {
                Color[,] eightByEight = new Color[8, 8];
                for (int i = startI; i < startI + 8; i++)
                {
                    tmpJ = startJ;
                    for (int j = startJ; j < startJ + 8; j++)
                    {
                        eightByEight[i - startI, j - startJ] = img.GetPixel(i, j);
                        tmpJ++;
                    }
                    tmpI++;
                }
                startI = tmpI;
                eightByEightArray.Add(eightByEight);
                if (!(tmpI < img.Width))
                {
                    startJ = tmpJ;
                    startI = 0;
                    tmpI = 0;
                }

            }

        }

        private void DecompressBtn_Click(object sender, EventArgs e)
        {

        }
    }
}
